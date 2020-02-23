using System.Collections.Generic;

using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using Newtonsoft.Json;


namespace ChatController
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModHandler : MonoBehaviour
    {
        [KSPField(guiName = "Sun Tracking", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool SunTracking;
        bool isActive = false;
        static bool force_off = false;
        void Awake()
        {
            if (!MenuSettings.isModOn)
                return;
            isActive = true;
            while (isActive && !force_off)
            {
                string[] commands = ChatHandler.ReadChats();
                if(commands.Length == 0)
                {
                    foreach (string command in commands)
                    {
                        if (command.Contains(" "))
                        {
                            this.ExecuteCommand(command.Split(MenuSettings.prefix));
                        }
                        else this.ExecuteCommand(new[] { command });
                    }
                }
                System.Threading.Thread.Sleep(MenuSettings.check_chat_delay);
            }
            force_off = true;
        }

        private void ExecuteCommand(string[] args)
        {
        
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
            configuration.load();
            isModOn = configuration.GetValue<bool>("ModOn");
            YoutubeConfig ytcfg= configuration.GetValue<YoutubeConfig>("YoutubeConfig"); 
            if (!isModOn)
                return;
            print("Kerbal Chat Control is ON!");
            ChatHandler.SetYTChat();
        }

        public static void PrintMessage(string v)
        {
            ScreenMessages.PostScreenMessage(v, 10f, ScreenMessageStyle.UPPER_RIGHT, false);
        }
    }

    public struct ConfigurationFile
    {
        bool Mod_On;
        YoutubeConfig YoutubeConfig;
    }
}
