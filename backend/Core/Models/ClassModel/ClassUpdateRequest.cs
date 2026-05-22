using Core.Enums;

namespace Core.Models.ClassModel
{
    public class ClassUpdateRequest
    {
        public string SportHall { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly BeginTime { get; set; }
        public WeekType WeekType { get; set; } = WeekType.Any;
    }
}
