using Core.Enums;

namespace Core.Models.TeamModel
{
    public class TeamResponse
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Name { get; set; }
        public AgeGroup AgeGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Core.Models.ImageDto>? Images { get; set; }
    }
}
