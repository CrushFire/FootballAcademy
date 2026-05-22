using Core.Enums;

namespace Core.Entities
{
    public class Attendance
    {
        public long Id { get; set; }
        public long TrainingId { get; set; }
        public long SportsmanId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public Training Training { get; set; } = null!;
        public Sportsman Sportsman { get; set; } = null!;
    }
}
