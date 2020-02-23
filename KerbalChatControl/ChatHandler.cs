using System;
using System.Collections.Generic;

namespace ChatController
{
    static class ChatHandler
    {

        private static string[] old_messages;

        public static string[] ReadChats()
        {
            YoutubeAPI yt = new YoutubeAPI();
            return yt.GetMessages("");
        }
    }

    interface IChat
    {
        string[] GetMessages(string channelId);
    }
}
