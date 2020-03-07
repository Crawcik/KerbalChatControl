using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ChatController
{

    static class ChatHandler : IDisposable
    {
        public static bool YoutubeOn { private set; get; }  = false;
        private static string YoutubeId;
        private static readonly string MyToken = RandomToken(64);
        private static List<Message> old_messages = new List<Message>();

        public static void Add(Platform platform, string channelId)
        {
            switch (platform)
            {
                case Platform.Youtube:
                    YoutubeOn = CheckConnection("https://www.youtube.com/channel/" + channelId);
                    YoutubeId = channelId;
                    break;
            }
        }

        public static Message[] ReadChats()
        {
            string platform_keys = "";
            string channelIdAll = "";
            if (YoutubeOn) {
                platform_keys += "y";
                channelIdAll += ":BREAK:" + YoutubeId; 
            }
            List<Message> messages = GetMessages(channelIdAll, platform_keys);
            messages.RemoveAll(now => old_messages.Exists(old=>old.id == now.id));
            old_messages = messages;
            return messages.ToArray();
        }
        public static List<Message> GetMessages(string channelId, string platforms)
        {
            string result;
            try
            {
                WebRequest request = WebRequest.Create($"http://localhost:8080/ksp/?channel={channelId}&token={MyToken}&type=y");
                request.Method = "GET";
                request.ContentType = "x-www-form-urlencoded";
                using (WebResponse response = request.GetResponse())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception e) 
            {
                string m = e.Message;
                return null;
            }
            try
            {
                List<Message> messages = new List<Message>();
                foreach (string message_data in result.Split(new string[] { ":BREAK:" }, StringSplitOptions.RemoveEmptyEntries))
                    messages.Add(Message.Prase(message_data));
                return messages
            }
            catch
            {
                return null;
            }

        }

        private static bool CheckConnection(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Timeout = 1000;
                request.Method = "HEAD"; // As per Lasse's comment

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private static string RandomToken(byte length)
        {
            const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
            string random_values = null;
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                random_values += glyphs[random.Next(0, glyphs.Length)];
            }
            return random_values;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public enum Platform
    {
        Youtube = 0
    }
}
