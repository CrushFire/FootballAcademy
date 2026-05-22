using Core.Interfaces.Services;
using Core.Models;
using Core.Models.GroupModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("group")]
    public class GroupController : BaseController
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroup(long groupId)
        {
            var result = await _groupService.GetGroupAsync(groupId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] Filter? filter)
        {
            var result = await _groupService.GetGroupsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("sportsman/{sportsmanId}")]
        public async Task<IActionResult> GetGroupsForSportsman(long sportsmanId)
        {
            var result = await _groupService.GetGroupsForSportsmanAsync(sportsmanId);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupCreateRequest req)
        {
            var result = await _groupService.CreateGroupAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroup([FromBody] GroupUpdateRequest req, long groupId)
        {
            var result = await _groupService.UpdateGroupAsync(req, groupId);
            return result.ToActionResult();
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(long groupId)
        {
            var result = await _groupService.DeleteGroupAsync(groupId);
            return result.ToActionResult();
        }

        [HttpPost("{groupId}/sportsman/{sportsmanId}")]
        public async Task<IActionResult> AddSportsmanToGroup(long groupId, long sportsmanId)
        {
            var result = await _groupService.AddSportsmanToGroupAsync(groupId, sportsmanId);
            return result.ToActionResult();
        }

        [HttpDelete("{groupId}/sportsman/{sportsmanId}")]
        public async Task<IActionResult> RemoveSportsmanFromGroup(long groupId, long sportsmanId)
        {
            var result = await _groupService.RemoveSportsmanFromGroupAsync(groupId, sportsmanId);
            return result.ToActionResult();
        }
    }
}
