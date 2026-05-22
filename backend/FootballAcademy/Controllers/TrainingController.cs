using Core.Interfaces.Services;
using Core.Models;
using Core.Models.TrainingModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("training")]
    public class TrainingController : BaseController
    {
        private readonly ITrainingService _trainingService;
        private readonly ISportsmanService _sportsmanService;

        public TrainingController(ITrainingService trainingService, ISportsmanService sportsmanService)
        {
            _trainingService = trainingService;
            _sportsmanService = sportsmanService;
        }

        [HttpGet("{trainingId}")]
        public async Task<IActionResult> GetTraining(long trainingId)
        {
            var result = await _trainingService.GetTrainingAsync(trainingId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainings([FromQuery] Filter? filter)
        {
            var result = await _trainingService.GetTrainingsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetTrainingsForSportsman(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _trainingService.GetTrainingsForSportsmanAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyTrainings([FromQuery] Filter? filter)
        {
            if (UserId == null) return Unauthorized();
            // Резолвим SportsmanId по UserId, потом тянем тренировки
            var sportsmanRes = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            if (!sportsmanRes.IsSuccess || sportsmanRes.Data == null)
                return sportsmanRes.ToActionResult();
            var result = await _trainingService.GetTrainingsForSportsmanAsync(sportsmanRes.Data.Id, filter);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTraining([FromBody] TrainingCreateRequest req)
        {
            var result = await _trainingService.CreateTrainingAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{trainingId}")]
        public async Task<IActionResult> UpdateTraining([FromBody] TrainingUpdateRequest req, long trainingId)
        {
            var result = await _trainingService.UpdateTrainingAsync(req, trainingId);
            return result.ToActionResult();
        }

        [HttpDelete("{trainingId}")]
        public async Task<IActionResult> DeleteTraining(long trainingId)
        {
            var result = await _trainingService.DeleteTrainingAsync(trainingId);
            return result.ToActionResult();
        }
    }
}
