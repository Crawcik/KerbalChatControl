using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApi
{
    public class Youtube
    {
        public bool IsConnected = false;
        private string
            client_id,
            client_secret,
            api_key = null;
        private const string
            redirect_url = "urn:ietf:wg:oauth:2.0:oob",
            scope = "https://www.googleapis.com/auth/youtube.readonly";
        private string code;
        private Token token;
        private WebClient wb = new WebClient();
        public Youtube(string client_id, string client_secret, string api_key = null)
        {
            if (api_key != null)
                this.api_key = "&key=" + api_key;
            this.client_id = client_id;
            this.client_secret = client_secret;
        }

        public bool TryGetToken(string code)
        {
            this.code = code;
            try
            {
                string s;
                wb.Headers.Clear();
                wb.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                s = wb.UploadString("https://oauth2.googleapis.com/token", "POST", $"code={code}&client_id={client_id}&client_secret={client_secret}&redirect_uri={redirect_url}&grant_type=authorization_code");
                token = JsonConvert.DeserializeObject<Token>(s);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool TryRefreshToken()
        {
            try
            {
                string s;
                wb.Headers.Clear();
                wb.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                s = wb.UploadString("https://oauth2.googleapis.com/token", "POST", $"refresh_token={token.RefreshToken}&client_id={client_id}&client_secret={client_secret}&grant_type=refresh_token");
                token = JsonConvert.DeserializeObject<Token>(s);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool TryGetLiveChatId(out string liveChatId, string channelId)
        {
            try
            {
                wb.Headers.Clear();
                wb.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token.AccessToken);
                wb.Headers.Add(HttpRequestHeader.Accept, "application/json");
                var result = JObject.Parse(wb.DownloadString("https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet&broadcastStatus=active&broadcastType=all" + api_key));
                var table = result.Descendants().Where(d => d is JArray).First();
                var broadcast = table.Children<JObject>().Where(x=>x["snippet"].Value<string>("channelId") == channelId).First();
                if (broadcast == null)
                {
                    liveChatId = null;
                    return false;
                }
                liveChatId = broadcast["snippet"].Value<string>("liveChatId");
            }
            catch
            {
                liveChatId = null;
                return false;
            }
            return true;
        }

        public bool TryGetChatMessages(string liveChatId, out Message[] messages)
        {
            List<Message> vs = new List<Message>();
            try
            {
                var result = JObject.Parse(wb.DownloadString($"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=snippet&key=" + api_key));
                var table = result.Descendants().Where(d => d is JArray).First();
                foreach (JObject chatMessageEvent in table)
                {
                    Message message = new Message();
                    message.id = chatMessageEvent.Value<string>("id");
                    message.text = chatMessageEvent["snippet"].Value<string>("displayMessage");
                    vs.Add(message);
                }
                messages = vs.ToArray();
            }
            catch
            {
                messages = null;
                return false;
            }
            return true;
        }

        public async Task KeepKeyAlive()
        {
            Console.WriteLine("KeepAliveOn");
            while (true)
            {
                await Task.Delay((this.token.ExpiresIn - 5) * 1000);
                if (TryRefreshToken())
                    Console.WriteLine("Token refreshed");
                else
                    Console.WriteLine("Token refresh failed!");
            }
        }

        internal class Token
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }
        }
    }
}
