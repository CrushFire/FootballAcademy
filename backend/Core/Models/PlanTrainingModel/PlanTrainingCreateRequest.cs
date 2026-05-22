namespace Core.Models.PlanTrainingModel
{
    public class PlanTrainingCreateRequest
    {
        public long TrainerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Workouts { get; set; } // поминутный план упражнений
    }
}
