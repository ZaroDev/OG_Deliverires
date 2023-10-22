namespace Exercise2.Chat
{
    [System.Serializable]
    public class Message
    {
        public string userName;
        public string message;
        public string date;

        public Message(string name, string msg, string dateTime)
        {
            userName = name;
            message = msg;
            date = dateTime;
        }
    }
}