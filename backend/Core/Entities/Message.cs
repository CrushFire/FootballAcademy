using Core.Enums;

namespace Core.Entities
{
    public class Message
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public SenderRole SenderRole { get; set; }
        public long ReceiverId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public long? BroadcastId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public Broadcast? Broadcast { get; set; }
    }
}
