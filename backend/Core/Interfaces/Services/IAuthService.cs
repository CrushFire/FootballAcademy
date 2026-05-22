using Core.Models.Auth;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> AuthorizationAsync(AuthRequest req);
        Task<Result<AuthResponse>> RegisterAdminAsync(RegistrationRequest req);
        Task<Result<AuthResponse>> RegisterSportsmanAsync(RegistrationRequest req);
        Task<Result<AuthResponse>> RegisterTrainerAsync(PersonalRegistrationRequest req);
        Task<Result<AuthResponse>> RegisterMedicalAsync(PersonalRegistrationRequest req);
    }
}
