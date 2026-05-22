using Core.Enums;

namespace Core.Entities
{
    public class Broadcast
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public long CreatedById { get; set; }
        public SenderRole CreatedByRole { get; set; }
        public BroadcastTargetType TargetType { get; set; }
        public long? TargetId { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public User CreatedBy { get; set; } = null!;
        public List<Message> Messages { get; set; } = new();
    }
}
