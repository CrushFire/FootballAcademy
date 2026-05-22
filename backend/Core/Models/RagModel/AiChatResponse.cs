namespace Core.Models.RagModel
{
    public class AiChatResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
