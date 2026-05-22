using Core.Interfaces.Services;
using Core.Models;
using Core.Models.SportsmanModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("sportsman")]
    public class SportsmanController : BaseController
    {
        private ISportsmanService _sportsmanService;
        
        public SportsmanController(ISportsmanService sportsmanService)
        {
            _sportsmanService = sportsmanService;
        }

        [HttpGet("{sportsmanId}")]
        public async Task<IActionResult> GetSportsman(long sportsmanId)
        {
            var result = await _sportsmanService.GetSportsmanAsync(sportsmanId);
            
            return result.ToActionResult();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            if(UserId == null) return Unauthorized();
            var result = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetSportsmen([FromQuery] Filter? filter)
        {
            var result = await _sportsmanService.GetSportsmenAsync(filter);

            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSportsman([FromForm] SportsmanCreateRequest req)
        {
            var result = await _sportsmanService.CreateSportsmanAsync(req);

            return result.ToActionResult();
        }

        [HttpPut("{sportsmanId}")]
        public async Task<IActionResult> UpdateSportsman([FromBody] SportsmanUpdateRequest req, long sportsmanId)
        {
            var result = await _sportsmanService.UpdateSportsmanAsync(req, sportsmanId);

            return result.ToActionResult();
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] SportsmanUpdateRequest req)
        {
            if(UserId == null) return Unauthorized();
            var sportsmanRes = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            if (!sportsmanRes.IsSuccess || sportsmanRes.Data == null)
                return sportsmanRes.ToActionResult();
            var result = await _sportsmanService.UpdateSportsmanAsync(req, sportsmanRes.Data.Id);

            return result.ToActionResult();
        }

        [HttpDelete("{sportsmanId}")]
        public async Task<IActionResult> DeleteSportsman(long sportsmanId)
        {
            var result = await _sportsmanService.DeleteSportsmanAsync(sportsmanId);

            return result.ToActionResult();
        }

        [HttpPost("{sportsmanId}/images")]
        public async Task<IActionResult> AddImages(long sportsmanId, [FromForm] List<IFormFile> newImages)
        {
            var result = await _sportsmanService.AddSportsmanImagesAsync(sportsmanId, newImages);

            return result.ToActionResult();
        }

        [HttpPost("me/images")]
        public async Task<IActionResult> AddMyImages([FromForm] List<IFormFile> newImages)
        {
            if (UserId == null) return Unauthorized();
            var sportsmanRes = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            if (!sportsmanRes.IsSuccess || sportsmanRes.Data == null)
                return sportsmanRes.ToActionResult();
            var result = await _sportsmanService.AddSportsmanImagesAsync(sportsmanRes.Data.Id, newImages);

            return result.ToActionResult();
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(long imageId)
        {
            var result = await _sportsmanService.DeleteSportsmanImageAsync(imageId);
            return result.ToActionResult();
        }

        [HttpGet("{sportsmanId}/workouts")]
        public async Task<IActionResult> GetWorkouts(long sportsmanId, [FromQuery] Filter? filter)
        {
            var result = await _sportsmanService.GetWorkoutsBySportsmanAsync(sportsmanId, filter);
            return result.ToActionResult();
        }

        [HttpGet("me/workouts")]
        public async Task<IActionResult> GetMyWorkouts([FromQuery] Filter? filter)
        {
            if (UserId == null) return Unauthorized();
            // Получаем спортсмена по UserId, затем его workouts по SportsmanId
            var sportsmanRes = await _sportsmanService.GetSportsmanByUserIdAsync(UserId.Value);
            if (!sportsmanRes.IsSuccess || sportsmanRes.Data == null)
                return sportsmanRes.ToActionResult();
            var result = await _sportsmanService.GetWorkoutsBySportsmanAsync(sportsmanRes.Data.Id, filter);
            return result.ToActionResult();
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetSportsmensForGroup(long groupId)
        {
            var result = await _sportsmanService.GetSportsmensForGroupAsync(groupId);
            return result.ToActionResult();
        }
    }
}
