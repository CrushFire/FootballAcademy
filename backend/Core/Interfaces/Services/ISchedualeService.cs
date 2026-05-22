using Core.Models.AttendanceModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface ISchedualeService
    {
        Task<Result<List<AttendanceResponse>>> MarkAttendanceAsync(long trainingId, List<AttendanceCreateRequest> req);
        Task<Result<List<AttendanceResponse>>> GetAttendanceAsync(long trainingId);
        Task<Result<List<AttendanceResponse>>> GetAttendanceBySportsmanAsync(long sportsmanId);
    }
}
