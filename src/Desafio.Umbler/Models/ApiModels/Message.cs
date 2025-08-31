namespace Desafio.Umbler.Models.ApiModels
{
    public class Message
    {
        public Message()
        {
   
        }

        public Message(MessageType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Message(string value)
        {
            Value = value;
            Type = MessageType.Default;
        }

        public MessageType Type { get; set; }
        public string Value { get; set; }
    }
}
