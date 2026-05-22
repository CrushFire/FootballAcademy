using Core.Enums;

namespace Core.Models.MessageModel
{
    public class MessageResponse
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public SenderRole SenderRole { get; set; }
        public long ReceiverId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsRead { get; set; }
        public long? BroadcastId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
