namespace Core.Entities
{
    public class Training
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public long? PlanTrainingId { get; set; }
        public long? GroupId { get; set; }
        public long? TeamId { get; set; }
        public long? MatchId { get; set; }
        public string Type { get; set; } = "Не указана";
        public DateTime Date { get; set; }
        public string? OtherInformation { get; set; }
        public DateTime CreatedAt { get; set; }
        public PlanTraining? PlanTraining { get; set; }
        public Personal Trainer { get; set; } = null!;
        public Group? Group { get; set; }
        public Team? Team { get; set; }
        public Match? Match { get; set; }
        public List<TrainingMetrics> Metrics { get; set; } = new();
    }
}
