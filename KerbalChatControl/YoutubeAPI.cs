using System.IO;
using System.Net;

namespace ChatController
{
    public class YoutubeAPI : IChat
    {
        public string[] GetMessages(string channelId)
        {
            string[] messages;
            try
            {
                WebRequest request = WebRequest.Create($"http://crawcik.space:8080/ksp/?channel={channelId}&api=TEST2&type=youtube");
                request.Method = "GET";
                request.ContentType = "x-www-form-urlencoded";
                using (WebResponse response = request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    byte[] buf = new byte[1024];
                    stream.Read(buf, 0, buf.Length);

                }
            }
            catch
            {
                return null;
            }
            
        }
    }
}
