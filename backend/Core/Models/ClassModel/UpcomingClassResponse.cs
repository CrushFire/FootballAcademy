namespace Core.Models.ClassModel
{
    public class UpcomingClassResponse
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string SportHall { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly BeginTime { get; set; }
        public DateTime ClassDate { get; set; } // конкретная дата на этой неделе
    }
}
