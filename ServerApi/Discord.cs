using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi
{
    public class Discord
    {
        private DiscordClient Client { get; set; }
        public Discord() => RunBotAsync().GetAwaiter();
        public async Task RunBotAsync()
        {
            var cfg = new DiscordConfiguration
            {
                Token = "NzE3NjYwNjEwNTI3MjMyMDYx.XtdjjQ.05G_J-YVG14BgOhy--ueSVBmANs",
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Critical,
                UseInternalLogHandler = true
            };
            this.Client = new DiscordClient(cfg);
            this.Client.SetWebSocketClient<WebSocketSharpClient>();
            this.Client.Ready += this.Client_Ready;
            await this.Client.ConnectAsync();
            await Task.Delay(-1);
        }

        public bool TryGetChatMessages(ulong channelID, out Message[] messages)
        {
            try {
                List<Message> list = new List<Message>();
                DiscordChannel channel = this.Client.GetChannelAsync(channelID).GetAwaiter().GetResult();
                foreach(DiscordMessage message in channel.GetMessagesAsync(limit: 20).GetAwaiter().GetResult())
                    list.Add( new Message { id = message.Id.ToString(), text = message.Content });
                messages = list.ToArray();
            }
            catch 
            {
                messages = null;
                return false;
            }
            return true;

        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            Console.WriteLine("Discord bot ready");
            return Task.CompletedTask;
        }
    }
}
