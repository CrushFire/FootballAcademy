namespace Core.Entities
{
    public class AiChat
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Title { get; set; } = "Новый чат";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Personal Trainer { get; set; } = null!;
        public List<AiMessage> Messages { get; set; } = new();
    }
}
