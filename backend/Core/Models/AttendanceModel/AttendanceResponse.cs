using Core.Enums;

namespace Core.Models.AttendanceModel
{
    public class AttendanceResponse
    {
        public long Id { get; set; }
        public long TrainingId { get; set; }
        public long SportsmanId { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TrainingDate { get; set; }
        public string? TrainingType { get; set; }
    }
}
