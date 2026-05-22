using Core.Interfaces.Services;
using FootballAcademy.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FootballAcademy.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<ChatHub> _hub;

        public SignalRNotificationService(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        public async Task NotifyUserAsync(long userId, string method, object payload)
        {
            await _hub.Clients.Group($"user_{userId}").SendAsync(method, payload);
        }
    }
}
