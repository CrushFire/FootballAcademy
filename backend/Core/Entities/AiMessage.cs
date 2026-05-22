namespace Core.Entities
{
    public class AiMessage
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        // "user" | "assistant"
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public AiChat Chat { get; set; } = null!;
    }
}
