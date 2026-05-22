using Core.Models;
using Core.Models.PersonalWorkoutModel;
using Core.Models.SportsmanModel;
using Core.Results;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Services
{
    public interface ISportsmanService
    {
        Task<Result<List<string>>> AddSportsmanImagesAsync(long sportsmanId, List<IFormFile> images);
        Task<Result<SportsmanResponse>> CreateSportsmanAsync(SportsmanCreateRequest req);
        Task<Result<bool>> DeleteSportsmanAsync(long id);
        Task<Result<bool>> DeleteSportsmanImageAsync(long imageId);
        Task<Result<SportsmanResponse>> GetSportsmanAsync(long id);
        Task<Result<SportsmanResponse>> GetSportsmanByUserIdAsync(long userId);
        Task<Result<List<SportsmanResponse>>> GetSportsmenAsync(Filter? filter);
        Task<Result<SportsmanResponse>> UpdateSportsmanAsync(SportsmanUpdateRequest req, long id);
        Task<Result<List<PersonalWorkoutResponse>>> GetWorkoutsBySportsmanAsync(long sportsmanId, Filter? filter);
        Task<Result<List<SportsmanResponse>>> GetSportsmensForGroupAsync(long groupId);
    }
}