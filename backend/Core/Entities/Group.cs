namespace Core.Entities
{
    public class Group
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Personal Trainer { get; set; } = null!;
        public List<SportsmanGroup> SportsmanGroups { get; set; } = new();
    }
}
