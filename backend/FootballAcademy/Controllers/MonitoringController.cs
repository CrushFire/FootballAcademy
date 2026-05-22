using FootballAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers;

[ApiController]
[Route("admin/monitoring")]
public class MonitoringController : ControllerBase
{
    [HttpGet("status")]
    [Authorize]
    public IActionResult GetStatus()
    {
        var now = DateTime.UtcNow;
        var errors = AppMonitoringState.GetErrors();
        var critical = errors.Where(e => e.Status >= 500).ToList();

        return Ok(new
        {
            apiAvailable = true,
            serverStartedAt = AppMonitoringState.ServerStartedAt,
            checkedAt = now,
            uptimeSeconds = (int)(now - AppMonitoringState.ServerStartedAt).TotalSeconds,
            criticalErrorsCount = critical.Count,
            lastCriticalErrorAt = critical.Count > 0 ? critical[^1].Timestamp : (DateTime?)null,
            affectedUsersCount = errors.Where(e => e.Status >= 500 && e.UserId.HasValue)
                                       .Select(e => e.UserId).Distinct().Count(),
            recentErrorsCount = errors.Count(e => (now - e.Timestamp).TotalSeconds < 60)
        });
    }

    [HttpGet("errors")]
    [Authorize]
    public IActionResult GetErrors([FromQuery] int limit = 500)
    {
        var errors = AppMonitoringState.GetErrors()
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();
        return Ok(errors);
    }

    [HttpPost("error")]
    [Authorize]
    public IActionResult ReportError([FromBody] ClientErrorEntry entry)
    {
        entry.Timestamp = DateTime.UtcNow;

        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var uid))
            entry.UserId = uid;

        AppMonitoringState.AddError(entry);
        return Ok();
    }

    [HttpDelete("errors")]
    [Authorize]
    public IActionResult ClearErrors()
    {
        AppMonitoringState.ClearErrors();
        return Ok();
    }
}
