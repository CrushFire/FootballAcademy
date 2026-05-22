using Core.Enums;

namespace Core.Entities
{
    public class Class
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string SportHall { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly BeginTime { get; set; }
        public WeekType WeekType { get; set; } = WeekType.Any;
        public DateTime CreatedAt { get; set; }
        public Group Group { get; set; } = null!;
    }
}
