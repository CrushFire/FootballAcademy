using Core.Models;
using Core.Models.MetricModel.Profile;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IProfileService
    {
        Task<Result<MetricPlayerResultResponse>> GetSportsmanProfileAsync(long sportsmanId, Filter? filter = null);
    }
}