using Core.Enums;

namespace Core.Models.ClassModel
{
    public class ClassResponse
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string SportHall { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly BeginTime { get; set; }
        public WeekType WeekType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime WeekDate { get; set; }
    }
}
