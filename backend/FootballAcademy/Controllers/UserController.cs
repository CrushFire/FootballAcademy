using Core.Interfaces.Services;
using Core.Models;
using Core.Models.User;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest req)
        {
            var result = await _userService.CreateUserAsync(req);

            return result.ToActionResult();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] long userId)
        {
            var result = await _userService.GetUserAsync(userId);

            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] Filter? filter)
        {
            var result = await _userService.GetUsersAsync(filter);

            return result.ToActionResult();
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetUsersAdmin([FromQuery] Filter? filter)
        {
            var result = await _userService.GetUsersAdminAsync(filter);

            return result.ToActionResult();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            if (UserId == null) return Unauthorized();
            var result = await _userService.GetUserAsync(UserId.Value);

            return result.ToActionResult();
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest req, [FromRoute] long userId)
        {
            var result = await _userService.UpdateUserAsync(req, userId);

            return result.ToActionResult();
        }

        [HttpPut("update/me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateRequest req)
        {
            if (UserId == null) return Unauthorized();
            var result = await _userService.UpdateUserAsync(req, UserId.Value);

            return result.ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(long userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            return result.ToActionResult();
        }
    }
}
