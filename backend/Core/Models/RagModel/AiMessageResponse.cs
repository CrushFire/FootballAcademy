namespace Core.Models.RagModel
{
    public class AiMessageResponse
    {
        public long Id { get; set; }
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
