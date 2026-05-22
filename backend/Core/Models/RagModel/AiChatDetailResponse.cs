namespace Core.Models.RagModel
{
    public class AiChatDetailResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<AiMessageResponse> Messages { get; set; } = new();
    }
}
