using Core.Models;
using Core.Models.User;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result<bool>> CreateUserAsync(UserCreateRequest req);
        Task<Result<bool>> DeleteUserAsync(long id);
        Task<Result<UserResponse>> GetUserAsync(long id);
        Task<Result<List<UserResponse>>> GetUsersAsync(Filter? filter);
        Task<Result<List<UserAdminResponse>>> GetUsersAdminAsync(Filter? filter);
        Task<Result<UserResponse>> UpdateUserAsync(UserUpdateRequest req, long Id);
    }
}