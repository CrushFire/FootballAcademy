namespace Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyUserAsync(long userId, string method, object payload);
    }
}
