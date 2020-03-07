using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;

namespace ChatController
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModHandler : MonoBehaviour
    {
        [KSPField(guiName = "Chat Control", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool running;
        bool isActive = false;
        static bool force_off = false;
        public static IEnumerable<Commands.ICommand> AllCommands;

        void Awake()
        {
            if (!MenuSettings.isModOn)
                return;
            MenuSettings.PrintMessage("Youtube is" + ChatHandler.YoutubeOn.ToString());
            isActive = true;
            while (isActive && !force_off)
            {
                if (running)
                {
                    Message[] messages = ChatHandler.ReadChats();
                    
                    if (messages.Length == 0)
                    {
                        foreach (Message message in messages)
                        {
                            if (message.text.ToCharArray()[0] == MenuSettings.prefix)
                            {
                                string command = message.text.Remove(0, 1);
                                if (command.Contains(" "))
                                {
                                    this.ExecuteCommand(command.Split(' '));
                                }
                                else this.ExecuteCommand(new[] { command });
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(MenuSettings.check_chat_delay);
            }
            force_off = true;
        }

        private void ExecuteCommand(string[] args)
        {
            if(AllCommands.Any(x => x.GetName() == args[0]))
            AllCommands.First(x => x.GetName() == args[0]).Execute(args.Skip(1).ToArray());
        }

        void OnDestroy()
        {
            isActive = false;
            force_off = false;
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class MenuSettings : MonoBehaviour
    {
        public static PluginConfiguration configuration = PluginConfiguration.CreateForType<ConfigurationFile>();
        public static bool isModOn = false;
        public static int check_chat_delay;
        public static char prefix = '$';

        void Awake()
        {
            //Loading config
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
            ModHandler.AllCommands =  Assembly.GetExecutingAssembly().GetTypes().Where(t => t == typeof(Commands.ICommand)).Select(x=>x as Commands.ICommand);
        }

        public static void PrintMessage(string v)
        {
            ScreenMessages.PostScreenMessage(v, 10f, ScreenMessageStyle.UPPER_RIGHT, false);
        }
    }

    public struct ConfigurationFile
    {
        bool Mod_On;
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
