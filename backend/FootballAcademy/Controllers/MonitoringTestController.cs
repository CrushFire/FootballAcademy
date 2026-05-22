using FootballAcademy.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers;

/// <summary>
/// Только для тестирования мониторинга. Удалить в проде.
/// </summary>
[ApiController]
[Route("test/monitoring")]
public class MonitoringTestController : ControllerBase
{
    /// <summary>Симулирует ошибку 500 — попадёт в лог мониторинга</summary>
    [HttpPost("error500")]
    public IActionResult Error500()
    {
        AppMonitoringState.AddError(new ClientErrorEntry
        {
            Timestamp = DateTime.UtcNow,
            Status = 500,
            Method = "POST",
            Url = "/api/test/monitoring/error500",
            Message = "Internal Server Error (тест)",
            UserId = 1
        });
        return StatusCode(500, new { error = "Тестовая ошибка 500" });
    }

    /// <summary>Симулирует ошибку 503</summary>
    [HttpPost("error503")]
    public IActionResult Error503()
    {
        AppMonitoringState.AddError(new ClientErrorEntry
        {
            Timestamp = DateTime.UtcNow,
            Status = 503,
            Method = "GET",
            Url = "/api/test/monitoring/error503",
            Message = "Service Unavailable (тест)",
            UserId = 2
        });
        return StatusCode(503, new { error = "Тестовая ошибка 503" });
    }

    /// <summary>Добавляет N случайных ошибок от разных пользователей</summary>
    [HttpPost("flood")]
    public IActionResult Flood([FromQuery] int count = 5)
    {
        var statuses = new[] { 500, 500, 502, 503, 504 };
        var urls = new[] { "/api/sportsmen", "/api/groups", "/api/matches", "/api/trainings", "/api/users" };
        var methods = new[] { "GET", "POST", "PUT", "DELETE" };
        var rnd = new Random();

        for (var i = 0; i < count; i++)
        {
            AppMonitoringState.AddError(new ClientErrorEntry
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-rnd.Next(0, 300)),
                Status = statuses[rnd.Next(statuses.Length)],
                Method = methods[rnd.Next(methods.Length)],
                Url = urls[rnd.Next(urls.Length)],
                Message = $"Test error #{i + 1}",
                UserId = rnd.Next(1, 6)
            });
        }

        return Ok(new { added = count, message = $"Добавлено {count} тестовых ошибок" });
    }

    /// <summary>Очищает лог мониторинга</summary>
    [HttpDelete("clear")]
    public IActionResult Clear()
    {
        AppMonitoringState.ClearErrors();
        return Ok(new { message = "Лог очищен" });
    }
}
