using Core.Enums;

namespace Core.Models.ClassModel
{
    public class ClassCreateRequest
    {
        public long GroupId { get; set; }
        public string SportHall { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly BeginTime { get; set; }
        public WeekType WeekType { get; set; } = WeekType.Any;
    }
}
