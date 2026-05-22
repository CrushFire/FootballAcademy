using Core.Enums;

namespace Core.Entities
{
    public class Personal
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FIO { get; set; } = null!;
        public string Position { get; set; } = null!;
        public PersonalType Type { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public List<Group> Groups { get; set; } = new();
        public List<PlanTraining>? PlanTrainings { get; set; }
        public List<Training>? Trainings { get; set; }
        public List<PersonalWorkout>? PersonalWorkouts { get; set; }
        public List<Image>? Images { get; set; }
        public List<Team>? Teams { get; set; }
    }
}
