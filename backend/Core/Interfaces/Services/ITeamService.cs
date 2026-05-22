using Core.Models;
using Core.Models.TeamModel;
using Core.Results;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Result<TeamResponse>> GetTeamAsync(long teamId);
        Task<Result<List<TeamResponse>>> GetTeamsAsync(Filter? filter);
        Task<Result<TeamResponse>> CreateTeamAsync(TeamCreateRequest req);
        Task<Result<TeamResponse>> UpdateTeamAsync(TeamUpdateRequest req, long teamId);
        Task<Result<bool>> DeleteTeamAsync(long teamId);
        Task<Result<List<string>>> AddTeamImagesAsync(long teamId, List<IFormFile> images);
        Task<Result<bool>> DeleteTeamImageAsync(long imageId);
        Task<Result<bool>> AddSportsmanToTeamAsync(long teamId, long sportsmanId);
        Task<Result<bool>> RemoveSportsmanFromTeamAsync(long sportsmanId);
        Task<Result<List<TeamStatsResponse>>> GetTeamsStatsAsync(Filter? filter);
    }
}
