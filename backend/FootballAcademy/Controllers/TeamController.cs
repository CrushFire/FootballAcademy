using Core.Interfaces.Services;
using Core.Models;
using Core.Models.TeamModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("team")]
    public class TeamController : BaseController
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(long teamId)
        {
            var result = await _teamService.GetTeamAsync(teamId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams([FromQuery] Filter? filter)
        {
            var result = await _teamService.GetTeamsAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromForm] TeamCreateRequest req)
        {
            var result = await _teamService.CreateTeamAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam([FromBody] TeamUpdateRequest req, long teamId)
        {
            var result = await _teamService.UpdateTeamAsync(req, teamId);
            return result.ToActionResult();
        }

        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(long teamId)
        {
            var result = await _teamService.DeleteTeamAsync(teamId);
            return result.ToActionResult();
        }

        [HttpPost("{teamId}/images")]
        public async Task<IActionResult> AddImages(long teamId, [FromForm] List<IFormFile> newImages)
        {
            var result = await _teamService.AddTeamImagesAsync(teamId, newImages);
            return result.ToActionResult();
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(long imageId)
        {
            var result = await _teamService.DeleteTeamImageAsync(imageId);
            return result.ToActionResult();
        }

        [HttpPost("{teamId}/sportsman/{sportsmanId}")]
        public async Task<IActionResult> AddSportsman(long teamId, long sportsmanId)
        {
            var result = await _teamService.AddSportsmanToTeamAsync(teamId, sportsmanId);
            return result.ToActionResult();
        }

        [HttpDelete("sportsman/{sportsmanId}")]
        public async Task<IActionResult> RemoveSportsman(long sportsmanId)
        {
            var result = await _teamService.RemoveSportsmanFromTeamAsync(sportsmanId);
            return result.ToActionResult();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetTeamsStats([FromQuery] Filter? filter)
        {
            var result = await _teamService.GetTeamsStatsAsync(filter);
            return result.ToActionResult();
        }
    }
}
