using System;
using System.IO;
using System.Net;

namespace ChatController
{

    static class ChatHandler
    {
        private static bool YoutubeOn = false;
        private static string YoutubeId;

        private static string[] old_messages;

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

        public static string[] ReadChats()
        {
            string platform_keys = "";
            string channelIdAll = "";
            if (YoutubeOn) {
                platform_keys += "y";
                channelIdAll += ":BREAK:" + YoutubeId; 
            }
            return GetMessages(channelIdAll, platform_keys);
        }
        public static string[] GetMessages(string channelId, string platforms)
        {
            string result;
            try
            {
                WebRequest request = WebRequest.Create($"http://crawcik.space:8080/ksp/?channel={channelId}&api=TEST2&type=youtube");
                request.Method = "GET";
                request.ContentType = "x-www-form-urlencoded";
                using (WebResponse response = request.GetResponse())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch
            {
                return null;
            }
            try
            {
                return result.Split(new string[] { ":BREAK:" },StringSplitOptions.RemoveEmptyEntries);
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

    }

    public enum Platform
    {
        Youtube = 0
    }
}
