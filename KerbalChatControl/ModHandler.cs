using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using System.Threading.Tasks;

namespace ChatController
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModHandler : MonoBehaviour
    {
        public bool running;
        bool isActive = false;
        static bool force_off = false;
        public static Vessel ActiveVessel = new Vessel();
        public static IEnumerable<Commands.ICommand> AllCommands;
        void Awake()
        {
            if (!MenuSettings.isModOn)
                return;
            //ActiveVessel = FlightGlobals.ActiveVessel;
            MenuSettings.PrintMessage("Youtube is" + ChatHandler.YoutubeOn.ToString());
            isActive = true;
            Check().GetAwaiter();
        }

        private async Task Check()
        {
            running = true;
            await Task.Delay(5000);
            while (isActive && !force_off)
            {
                if (running)
                {
                    Message[] messages = ChatHandler.ReadChats();

                    if (messages.Length != 0)
                    {
                        foreach (Message message in messages)
                        {
                            System.IO.File.Create(message.text);
                            if (message.text.ToCharArray()[0] == MenuSettings.prefix)
                            {
                                string command = message.text.Remove(0, 1);
                                if (command.Contains(" "))
                                {
                                    this.ExecuteCommand(command.Split(' '));
                                }
                                else this.ExecuteCommand(new string[] { command });
                            }
                        }
                    }
                }
                await Task.Delay(MenuSettings.check_chat_delay);
            }
            force_off = true;
        }

        private void ExecuteCommand(string[] args)
        {
            if (AllCommands.Any(x => x.GetName() == args[0]))
            {
                try
                {
                    AllCommands.First(x => x.GetName() == args[0]).Execute(args.Skip(1).ToArray());
                }
                catch
                {
                    System.IO.File.Create("Error");
                }
            }
        }

        void OnDestroy()
        {
            isActive = false;
            force_off = false;
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class MenuSettings : MonoBehaviour
    {
        public static PluginConfiguration configuration = PluginConfiguration.CreateForType<ConfigurationFile>();
        public static bool isModOn = false;
        public static int check_chat_delay = 1000;
        public static char prefix = '$';

        void Awake()
        {
            //Loading config
            PrintMessage("Loading Config");
            configuration.load();
            isModOn = configuration.GetValue<bool>("ModOn");
            string ytcfg = configuration.GetValue<string>("YoutubeChannelId");
            if (!isModOn)
                return;
            PrintMessage("Kerbal Chat Control is ON!");

            //Adding platforms
            if (ytcfg != null && ytcfg != "none")
                ChatHandler.Add(Platform.Youtube, ytcfg);

            //Setting commands
            ModHandler.AllCommands = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Commands.ICommand).IsAssignableFrom(t) && t != typeof(Commands.ICommand)).Select(t => System.Activator.CreateInstance(t) as Commands.ICommand);
            System.IO.File.Create(ModHandler.AllCommands.FirstOrDefault().GetName());
        }

        public static void PrintMessage(string v)
        {
            ScreenMessages.PostScreenMessage(v, 2f, ScreenMessageStyle.UPPER_CENTER);
        }
    }

    public struct ConfigurationFile
    {
        bool ModOn;
        string YoutubeChannelId;
    }

    public struct Message
    {
        public string id;
        public string text;
        public static Message Prase(string s)
        {
            Message tmp = new Message();
            string[] args = s.Split(new string[] { ":SPLIT:" }, System.StringSplitOptions.None);
            tmp.id = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(args[0]));
            tmp.text = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(args[1]));
            return tmp;
        }
    }
}
