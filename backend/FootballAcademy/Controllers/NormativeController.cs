using AutoMapper;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.NormativeModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("normative")]
    public class NormativeController : BaseController
    {
        private readonly INormativeService _normativeService;

        public NormativeController(INormativeService normativeService)
        {
            _normativeService = normativeService;
        }

        // Normative
        [HttpGet("gto/{id}")]
        public async Task<IActionResult> GetNormative(long id)
        {
            var result = await _normativeService.GetNormativeAsync(id);
            return result.ToActionResult();
        }

        [HttpGet("gto")]
        public async Task<IActionResult> GetNormatives([FromQuery] Filter? filter)
        {
            var result = await _normativeService.GetNormativesAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost("gto")]
        public async Task<IActionResult> CreateNormative([FromBody] NormativeCreateRequest req)
        {
            var result = await _normativeService.CreateNormativeAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("gto/{id}")]
        public async Task<IActionResult> UpdateNormative([FromBody] NormativeUpdateRequest req, long id)
        {
            var result = await _normativeService.UpdateNormativeAsync(req, id);
            return result.ToActionResult();
        }

        [HttpDelete("gto/{id}")]
        public async Task<IActionResult> DeleteNormative(long id)
        {
            var result = await _normativeService.DeleteNormativeAsync(id);
            return result.ToActionResult();
        }

        // LocalNormative
        [HttpGet("local/{id}")]
        public async Task<IActionResult> GetLocalNormative(long id)
        {
            var result = await _normativeService.GetLocalNormativeAsync(id);
            return result.ToActionResult();
        }

        [HttpGet("local")]
        public async Task<IActionResult> GetLocalNormatives([FromQuery] Filter? filter)
        {
            var result = await _normativeService.GetLocalNormativesAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost("local")]
        public async Task<IActionResult> CreateLocalNormative([FromBody] LocalNormativeCreateRequest req)
        {
            var result = await _normativeService.CreateLocalNormativeAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("local/{id}")]
        public async Task<IActionResult> UpdateLocalNormative([FromBody] LocalNormativeUpdateRequest req, long id)
        {
            var result = await _normativeService.UpdateLocalNormativeAsync(req, id);
            return result.ToActionResult();
        }

        [HttpDelete("local/{id}")]
        public async Task<IActionResult> DeleteLocalNormative(long id)
        {
            var result = await _normativeService.DeleteLocalNormativeAsync(id);
            return result.ToActionResult();
        }

        // Results
        [HttpGet("gto/results/sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetNormativeResults(long sportsmanId)
        {
            var result = await _normativeService.GetNormativeResultsAsync(sportsmanId);
            return result.ToActionResult();
        }

        [HttpPost("gto/results")]
        public async Task<IActionResult> AddNormativeResult([FromBody] NormativeSportsmanCreateRequest req)
        {
            var result = await _normativeService.AddNormativeResultAsync(req);
            return result.ToActionResult();
        }

        [HttpGet("local/results/sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetLocalNormativeResults(long sportsmanId)
        {
            var result = await _normativeService.GetLocalNormativeResultsAsync(sportsmanId);
            return result.ToActionResult();
        }

        [HttpPost("local/results")]
        public async Task<IActionResult> AddLocalNormativeResult([FromBody] LocalNormativeSportsmanCreateRequest req)
        {
            var result = await _normativeService.AddLocalNormativeResultAsync(req);
            return result.ToActionResult();
        }
    }
}
