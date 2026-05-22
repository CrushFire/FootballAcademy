using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MatchModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("match")]
    public class MatchController : BaseController
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatch(long matchId)
        {
            var result = await _matchService.GetMatchAsync(matchId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches([FromQuery] Filter? filter)
        {
            var result = await _matchService.GetMatchesAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] MatchCreateRequest req)
        {
            var result = await _matchService.CreateMatchAsync(req);
            return result.ToActionResult();
        }

        [HttpDelete("{matchId}")]
        public async Task<IActionResult> DeleteMatch(long matchId)
        {
            var result = await _matchService.DeleteMatchAsync(matchId);
            return result.ToActionResult();
        }

        [HttpPut("{matchId}")]
        public async Task<IActionResult> UpdateMatch(long matchId, [FromBody] MatchUpdateRequest req)
        {
            var result = await _matchService.UpdateMatchAsync(matchId, req);
            return result.ToActionResult();
        }

        [HttpPut("{matchId}/start")]
        public async Task<IActionResult> StartMatch(long matchId)
        {
            var result = await _matchService.StartMatchAsync(matchId);
            return result.ToActionResult();
        }

        [HttpPut("{matchId}/finish")]
        public async Task<IActionResult> FinishMatch(long matchId, [FromBody] MatchFinishRequest req)
        {
            var result = await _matchService.FinishMatchAsync(matchId, req);
            return result.ToActionResult();
        }

        // Real-time события
        [HttpGet("{matchId}/events")]
        public async Task<IActionResult> GetEvents(long matchId)
        {
            var result = await _matchService.GetEventsAsync(matchId);
            return result.ToActionResult();
        }

        [HttpPost("{matchId}/event")]
        public async Task<IActionResult> AddEvent(long matchId, [FromBody] MatchEventCreateRequest req)
        {
            var result = await _matchService.AddEventAsync(matchId, req);
            return result.ToActionResult();
        }

        [HttpPut("{matchId}/lineup")]
        public async Task<IActionResult> UpdateLineup(long matchId, [FromBody] List<LineupEntryDto> lineup)
        {
            var result = await _matchService.UpdateLineupAsync(matchId, lineup);
            return result.ToActionResult();
        }

        [HttpDelete("event/{eventId}")]
        public async Task<IActionResult> DeleteEvent(long eventId)
        {
            var result = await _matchService.DeleteEventAsync(eventId);
            return result.ToActionResult();
        }
    }
}
