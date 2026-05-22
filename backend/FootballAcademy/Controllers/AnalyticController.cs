using Application.Services.MetricAnalytic;
using AutoMapper;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel.Graph;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("analytic")]
    [Authorize]
    public class AnalyticController : BaseController
    {
        private readonly IGraphService _graphService;
        private readonly IMainMetricService _mainMetricService;
        private readonly IMedicalMetricService _medicalMetricService;
        private readonly IPentagonService _pentagonService;
        private readonly IProfileService _profileService;
        private readonly IMapper _mapper;

        public AnalyticController(IGraphService graphService, IMainMetricService mainMetricService, IMedicalMetricService medicalMetricService, IPentagonService pentagonService, IProfileService profileService, IMapper mapper)
        {
            _graphService = graphService;
            _mainMetricService = mainMetricService;
            _medicalMetricService = medicalMetricService;
            _pentagonService = pentagonService;
            _profileService = profileService;
            _mapper = mapper;
        }

        [HttpGet("{sportsmanId}/graph")]
        public async Task<IActionResult> GetGraph(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _graphService.GetGraphMetricsAsync(sportsmanId, filter);
            return Ok(result);
        }

        [HttpGet("{sportsmanId}/metrics")]
        public async Task<IActionResult> GetAverageMetrics(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _mainMetricService.GetSportsmanAverageMetricsAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/metrics/{trainingId}")]
        public async Task<IActionResult> GetTrainingMetrics(long sportsmanId, long trainingId)
        {
            var result = await _mainMetricService.GetSportsmanTrainingMetricsAsync(sportsmanId, trainingId);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/medical")]
        public async Task<IActionResult> GetMedicalMetrics(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _medicalMetricService.GetSportsmanMedicalMetricsAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/medical/check")]
        public async Task<IActionResult> GetMedicalCheck(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _medicalMetricService.GetSportsmanMedicalCheckAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("medical/check")]
        public async Task<IActionResult> GetAllMedicalCheck([FromQuery] Filter? filter)
        {
            var result = await _medicalMetricService.GetAllSportsmenMedicalCheckAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/pentagon")]
        public async Task<IActionResult> GetPentagon(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _pentagonService.GetSportsmanPentagonAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/profile")]
        public async Task<IActionResult> GetProfile(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _profileService.GetSportsmanProfileAsync(sportsmanId, filter);
            return result.ToActionResult();
        }
    }
}
