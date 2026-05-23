namespace Core.Models.BroadcastModel
{
    public class BroadcastDetailsResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public long CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public List<BroadcastRecipientDto> Recipients { get; set; } = new();
    }
}
