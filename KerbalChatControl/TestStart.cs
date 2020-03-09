using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChatController
{
    class TestStart
    {
        static void Main(string[] args)
        {
            ChatHandler.Add(Platform.Youtube, "UCvbk1Z676ajHgSnYdryCKbw");
            Message[] msgs = ChatHandler.ReadChats();
            Message[] msgs2 = ChatHandler.ReadChats();
        }
    }
}
