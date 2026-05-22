namespace Core.Models.MessageModel
{
    public class MessageSendRequest
    {
        public long ReceiverId { get; set; }
        public string Text { get; set; } = null!;
    }
}
