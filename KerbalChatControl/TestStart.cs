namespace ChatController
{
    public class TestStart
    {
        static void Main()
        {
            ChatHandler.Add(Platform.Youtube, "UCvbk1Z676ajHgSnYdryCKbw");
            string[] read_messages = ChatHandler.ReadChats();
        }
    }
}
