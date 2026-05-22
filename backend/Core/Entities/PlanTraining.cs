namespace Core.Entities
{
    public class PlanTraining
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Workouts { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Personal Trainer { get; set; } = null!;
        public List<Training> Trainings { get; set; } = new();
    }
}
