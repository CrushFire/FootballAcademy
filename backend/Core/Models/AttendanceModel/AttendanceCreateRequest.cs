using Core.Enums;

namespace Core.Models.AttendanceModel
{
    public class AttendanceCreateRequest
    {
        public long SportsmanId { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
