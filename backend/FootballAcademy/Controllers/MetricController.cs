using Application.Services;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("metric")]
    public class MetricController : BaseController
    {
        private readonly IMetricsService _metricsService;
        private readonly MetricAutoImportService _autoImportService;
        private readonly ISportsmanService _sportsmanService;

        public MetricController(IMetricsService metricsService, MetricAutoImportService autoImportService, ISportsmanService sportsmanService)
        {
            _metricsService = metricsService;
            _autoImportService = autoImportService;
            _sportsmanService = sportsmanService;
        }

        [HttpGet("{metricId}")]
        public async Task<IActionResult> GetMetric(long metricId)
        {
            var result = await _metricsService.GetMetricAsync(metricId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetMetrics([FromQuery] Filter? filter)
        {
            var result = await _metricsService.GetMetricsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("training/{trainingId}/sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetMetricByTrainingSportsman(long trainingId, long sportsmanId)
        {
            var result = await _metricsService.GetMetricByTrainingSportsmanAsync(trainingId, sportsmanId);
            return result.ToActionResult();
        }

        [HttpGet("training/{trainingId}/me")]
        public async Task<IActionResult> GetMyMetricByTraining(long trainingId)
        {
            if (UserId == null) return Unauthorized();
            var sportsmanRes = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            if (!sportsmanRes.IsSuccess || sportsmanRes.Data == null)
                return sportsmanRes.ToActionResult();
            var result = await _metricsService.GetMetricByTrainingSportsmanAsync(trainingId, sportsmanRes.Data.Id);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMetric([FromBody] MetricCreateRequest req)
        {
            var result = await _metricsService.CreateMetricAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{metricId}")]
        public async Task<IActionResult> UpdateMetric([FromBody] MetricUpdateRequest req, long metricId)
        {
            var result = await _metricsService.UpdateMetricAsync(req, metricId);
            return result.ToActionResult();
        }

        [HttpDelete("{metricId}")]
        public async Task<IActionResult> DeleteMetric(long metricId)
        {
            var result = await _metricsService.DeleteMetricAsync(metricId);
            return result.ToActionResult();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportMetrics([FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest("Файлы не предоставлены");

            var result = await _metricsService.ImportMetricsFromFilesAsync(files);
            return result.ToActionResult();
        }

        [HttpGet("auto-import/status")]
        public IActionResult GetAutoImportStatus()
        {
            var enabled = _autoImportService.AutoImportEnabled;
            return Ok(new { enabled });
        }

        [HttpPost("auto-import/toggle")]
        public IActionResult ToggleAutoImport([FromBody] bool enable)
        {
            _autoImportService.SetAutoImport(enable);
            return Ok(new { enabled = enable, message = enable ? "Автоимпорт включен" : "Автоимпорт выключен" });
        }

        [HttpPost("auto-import/run-now")]
        public async Task<IActionResult> RunAutoImportNow()
        {
            var result = await _autoImportService.RunAutoImportNowAsync();
            return result.ToActionResult();
        }
    }
}
