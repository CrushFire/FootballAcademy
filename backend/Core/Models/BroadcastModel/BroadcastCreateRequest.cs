using Core.Enums;

namespace Core.Models.BroadcastModel
{
    public class BroadcastCreateRequest
    {
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public BroadcastTargetType TargetType { get; set; }
        public long? TargetId { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}
