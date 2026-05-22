namespace Core.Entities
{
    public class PersonalWorkout
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public long PersonalId { get; set; }
        public string Workout { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Sportsman Sportsman { get; set; } = null!;
        public Personal Personal { get; set; } = null!;
    }
}
