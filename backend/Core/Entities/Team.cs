using Core.Enums;

namespace Core.Entities
{
    public class Team
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Name { get; set; } = null!;
        public AgeGroup AgeGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public Personal Trainer { get; set; } = null!;
        public List<Sportsman> Sportsmen { get; set; } = new();
        public List<Match> Matches { get; set; } = new();
        public List<Image>? Images { get; set; }
    }
}
