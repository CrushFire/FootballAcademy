using Core.Enums;

namespace Core.Models.BroadcastModel
{
    public class BroadcastResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public long CreatedById { get; set; }
        public SenderRole CreatedByRole { get; set; }
        public BroadcastTargetType TargetType { get; set; }
        public long? TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public int RecipientsCount { get; set; }
    }
}
