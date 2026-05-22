namespace Core.Entities
{
    public class Image
    {
        public long Id { get; set; }
        public long? SportsmanId { get; set; }
        public long? PersonalId { get; set; }
        public long? TeamId { get; set; }
        public string Path { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Sportsman? Sportsman { get; set; }
        public Personal? Personal { get; set; }
        public Team? Team { get; set; }
    }
}
