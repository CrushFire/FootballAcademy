using Core.Enums;

namespace Core.Entities
{
    public class Sportsman
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FIO { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year - (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
        public int Height { get; set; }
        public int Weight { get; set; }
        public Position? Position { get; set; }
        public long? TeamId { get; set; }
        public char Gender { get; set; }
        public Specialization Specialization { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public Team? Team { get; set; }
        public List<SportsmanGroup> SportsmanGroups { get; set; } = new();
        public List<NormativeSportsman> Normatives { get; set; } = new();
        public List<LocalNormativeSportsman> LocalNormatives { get; set; } = new();
        public List<PersonalWorkout> PersonalWorkouts { get; set; } = new();
        public List<Image>? Images { get; set; }
        public List<TrainingMetrics>? TrainingMetrics { get; set; }
    }
}
