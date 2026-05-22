using Core.Enums;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.BroadcastModel;
using Core.Models.MessageModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Authorize]
    [Route("message")]
    public class MessageController : BaseController
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // Личные сообщения

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] MessageSendRequest req)
        {
            if (UserId == null) return Unauthorized();
            if (!TryGetRole(out var role)) return Forbid();

            var result = await _messageService.SendAsync(UserId.Value, role, req);
            return result.ToActionResult();
        }

        [HttpGet("dialogs")]
        public async Task<IActionResult> GetDialogs()
        {
            if (UserId == null) return Unauthorized();
            var result = await _messageService.GetDialogListAsync(UserId.Value);
            return result.ToActionResult();
        }

        [HttpGet("dialog/{withUserId}")]
        public async Task<IActionResult> GetDialog([FromRoute] long withUserId, [FromQuery] Filter? filter)
        {
            if (UserId == null) return Unauthorized();

            var result = await _messageService.GetDialogAsync(UserId.Value, withUserId, filter);
            return result.ToActionResult();
        }

        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] long messageId)
        {
            if (UserId == null) return Unauthorized();

            var result = await _messageService.MarkReadAsync(messageId, UserId.Value);
            return result.ToActionResult();
        }

        // Рассылки

        [HttpPost("broadcast")]
        public async Task<IActionResult> SendBroadcast([FromBody] BroadcastCreateRequest req)
        {
            if (UserId == null) return Unauthorized();
            if (!TryGetRole(out var role)) return Forbid();

            var result = await _messageService.SendBroadcastAsync(UserId.Value, role, req);
            return result.ToActionResult();
        }

        [HttpGet("broadcast")]
        public async Task<IActionResult> GetBroadcasts([FromQuery] Filter? filter)
        {
            var result = await _messageService.GetBroadcastsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("broadcast/{broadcastId}")]
        public async Task<IActionResult> GetBroadcastDetails([FromRoute] long broadcastId, [FromQuery] Filter? filter)
        {
            var result = await _messageService.GetBroadcastDetailsAsync(broadcastId, filter);
            return result.ToActionResult();
        }

        [HttpDelete("broadcast/{broadcastId}")]
        public async Task<IActionResult> DeleteBroadcast([FromRoute] long broadcastId)
        {
            var result = await _messageService.DeleteBroadcastAsync(broadcastId);
            return result.ToActionResult();
        }

        // Подсчёт всех сообщений + рассылок (для админского дашборда)
        [HttpGet("count")]
        public async Task<IActionResult> GetTotalCount()
        {
            var result = await _messageService.GetTotalCountAsync();
            return result.ToActionResult();
        }

        // Вспомогательные методы

        private bool TryGetRole(out SenderRole role)
        {
            var raw = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (raw.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                role = SenderRole.Admin;
                return true;
            }

            if (raw.Equals("personal", StringComparison.OrdinalIgnoreCase))
            {
                var personalType = User.FindFirstValue("personalType") ?? string.Empty;
                if (personalType.Equals("Trainer", StringComparison.OrdinalIgnoreCase))
                {
                    role = SenderRole.Trainer;
                    return true;
                }
                if (personalType.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    role = SenderRole.Medical;
                    return true;
                }
            }

            role = default;
            return false;
        }
    }
}
