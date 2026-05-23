using Core.Enums;

namespace Core.Models.BroadcastModel
{
    public class BroadcastResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public long CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public SenderRole CreatedByRole { get; set; }
        public BroadcastTargetType TargetType { get; set; }
        public long? TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public int RecipientsCount { get; set; }
        // Статус прочтения для конкретного запросившего юзера (только при onlyForMe=true).
        // null если запрос не был от лица конкретного получателя.
        public bool? IsReadByMe { get; set; }
    }
}
