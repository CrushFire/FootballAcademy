using Core.Interfaces.Services;
using Core.Models.Auth;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("auth")]
        public async Task<IActionResult> Authorization([FromBody] AuthRequest req)
        {
            var result = await _authService.AuthorizationAsync(req);
            return result.ToActionResult();
        }

        [HttpPost("reg/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegistrationRequest req)
        {
            var result = await _authService.RegisterAdminAsync(req);
            return result.ToActionResult();
        }

[HttpPost("reg/sportsman")]
        public async Task<IActionResult> RegisterSportsman([FromBody] RegistrationRequest req)
        {
            var result = await _authService.RegisterSportsmanAsync(req);
            return result.ToActionResult();
        }
        [HttpPost("reg/trainer")]
        public async Task<IActionResult> RegisterTrainer([FromBody] PersonalRegistrationRequest req)
        {
            var result = await _authService.RegisterTrainerAsync(req);
            return result.ToActionResult();
        }

        [HttpPost("reg/medical")]
        public async Task<IActionResult> RegisterMedical([FromBody] PersonalRegistrationRequest req)
        {
            var result = await _authService.RegisterMedicalAsync(req);
            return result.ToActionResult();
        }
    }
}
