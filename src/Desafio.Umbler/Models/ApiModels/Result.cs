using System.Collections.Generic;

namespace Desafio.Umbler.Models.ApiModels
{
    public class Result<T>
    {
        public Result()
        {
            Messages = new List<Message>();
        }

        public Result(T data)
        {
            Data = data; 
            Messages = new List<Message>();
        }

        public Result(string errorMessage)
        {
            Messages = new List<Message>();
            Data = default;
            Messages.Add(new Message(MessageType.Error, errorMessage));
        }

        public List<Message> Messages { get; }
        public T Data { get; set; }
    }
}
