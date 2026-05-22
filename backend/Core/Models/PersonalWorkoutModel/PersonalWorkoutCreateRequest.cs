namespace Core.Models.PersonalWorkoutModel
{
    public class PersonalWorkoutCreateRequest
    {
        public long SportsmanId { get; set; }
        public long PersonalId { get; set; }
        public string Workout { get; set; }
    }
}
