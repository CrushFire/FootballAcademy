namespace Core.Entities
{
    public class SportsmanGroup
    {
        public long SportsmanId { get; set; }
        public long GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Sportsman Sportsman { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
