namespace Core.Models.PersonalWorkoutModel
{
    public class PersonalWorkoutResponse
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public string SportsmanFIO { get; set; }
        public long PersonalId { get; set; }
        public string PersonalFIO { get; set; }
        public string Workout { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
