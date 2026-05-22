using Core.Interfaces.Services;
using Core.Models;
using Core.Models.AttendanceModel;
using Core.Models.ClassModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("schedule")]
    public class ScheduleController : BaseController
    {
        private readonly IClassService _classService;
        private readonly ISchedualeService _schedualeService;

        public ScheduleController(IClassService classService, ISchedualeService schedualeService)
        {
            _classService = classService;
            _schedualeService = schedualeService;
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClass(long classId)
        {
            var result = await _classService.GetClassAsync(classId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses([FromQuery] Filter? filter)
        {
            var result = await _classService.GetClassesAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateRequest req)
        {
            var result = await _classService.CreateClassAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{classId}")]
        public async Task<IActionResult> UpdateClass([FromBody] ClassUpdateRequest req, long classId)
        {
            var result = await _classService.UpdateClassAsync(req, classId);
            return result.ToActionResult();
        }

        [HttpDelete("{classId}")]
        public async Task<IActionResult> DeleteClass(long classId)
        {
            var result = await _classService.DeleteClassAsync(classId);
            return result.ToActionResult();
        }

        [HttpPost("attendance/{trainingId}")]
        public async Task<IActionResult> MarkAttendance(long trainingId, [FromBody] List<AttendanceCreateRequest> req)
        {
            var result = await _schedualeService.MarkAttendanceAsync(trainingId, req);
            return result.ToActionResult();
        }

        [HttpGet("attendance/{trainingId}")]
        public async Task<IActionResult> GetAttendance(long trainingId)
        {
            var result = await _schedualeService.GetAttendanceAsync(trainingId);
            return result.ToActionResult();
        }

        [HttpGet("attendance/sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetAttendanceBySportsman(long sportsmanId)
        {
            var result = await _schedualeService.GetAttendanceBySportsmanAsync(sportsmanId);
            return result.ToActionResult();
        }
    }
}
