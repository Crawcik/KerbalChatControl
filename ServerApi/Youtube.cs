using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApi
{
    public class Youtube
    {
        private string
            client_id,
            client_secret;
        private const string
            redirect_url = "urn:ietf:wg:oauth:2.0:oob",
            api_key = "AIzaSyAcgDfbw0b3ht6YbvFAlNiQJss0nIQmn_c",
            scope = "https://www.googleapis.com/auth/youtube.readonly";
        private string code;
        private Token token;
        private WebClient wb = new WebClient();
        public Youtube(string client_id, string client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
        }

        public bool TryGetToken(string _code)
        {
            try
            {
                string s;
                wb.Headers.Clear();
                wb.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                s = wb.UploadString("https://oauth2.googleapis.com/token", "POST", $"code={_code}&client_id={client_id}&client_secret={client_secret}&redirect_uri={redirect_url}&grant_type=authorization_code");
                token = JsonConvert.DeserializeObject<Token>(s);
                this.code = _code;
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
                var result = JObject.Parse(wb.DownloadString("https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet&broadcastStatus=active&broadcastType=all&key=" + api_key));
                var table = result.Descendants().Where(d => d is JArray).First();
                var broadcast = table.Children<JObject>().Where(x=>x["snippet"].Value<string>("liveChatId") == channelId).First();
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

        public bool TryGetChatMessages(string liveChatId, out string[] messages)
        {
            List<string> vs = new List<string>();
            try
            {
                var result = JObject.Parse(wb.DownloadString($"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=snippet&key=" + api_key));
                var table = result.Descendants().Where(d => d is JArray).First();
                foreach (JObject chatMessageEvent in table.Children<JObject>().Where(d => d["snippet"].Value<string>("type") == "textMessageEvent"))
                {
                    vs.Add(chatMessageEvent["snippet"]["textMessageDetails"].Value<string>("messageText"));
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
            while (true)
            {
                await Task.Delay((this.token.ExpiresIn - 5) * 1000);
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
