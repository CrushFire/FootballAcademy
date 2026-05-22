using Core.Enums;
using Core.Models;
using Core.Models.BroadcastModel;
using Core.Models.MessageModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IMessageService
    {
        // Message
        Task<Result<MessageResponse>> SendAsync(long senderId, SenderRole senderRole, MessageSendRequest req);
        Task<Result<List<DialogPreviewResponse>>> GetDialogListAsync(long userId);
        Task<Result<DialogResponse>> GetDialogAsync(long userId, long withUserId, Filter? filter);
        Task<Result<bool>> MarkReadAsync(long messageId, long userId);
        Task<Result<int>> GetTotalCountAsync();

        // Broadcast
        Task<Result<BroadcastResponse>> SendBroadcastAsync(long createdById, SenderRole createdByRole, BroadcastCreateRequest req);
        Task<Result<List<BroadcastResponse>>> GetBroadcastsAsync(Filter? filter);
        Task<Result<BroadcastDetailsResponse>> GetBroadcastDetailsAsync(long broadcastId, Filter? filter);
        Task<Result<bool>> DeleteBroadcastAsync(long broadcastId);
    }
}
