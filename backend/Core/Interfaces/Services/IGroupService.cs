using Core.Models;
using Core.Models.GroupModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IGroupService
    {
        Task<Result<GroupResponse>> GetGroupAsync(long groupId);
        Task<Result<List<GroupResponse>>> GetGroupsAsync(Filter? filter);
        Task<Result<List<GroupResponse>>> GetGroupsForSportsmanAsync(long sportsmanId);
        Task<Result<GroupResponse>> CreateGroupAsync(GroupCreateRequest req);
        Task<Result<GroupResponse>> UpdateGroupAsync(GroupUpdateRequest req, long groupId);
        Task<Result<bool>> DeleteGroupAsync(long groupId);
        Task<Result<bool>> AddSportsmanToGroupAsync(long groupId, long sportsmanId);
        Task<Result<bool>> RemoveSportsmanFromGroupAsync(long groupId, long sportsmanId);
    }
}
