namespace Core.Models.PlanTrainingModel
{
    public class PlanTrainingResponse
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string TrainerFIO { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Workouts { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
