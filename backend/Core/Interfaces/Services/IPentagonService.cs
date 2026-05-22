
using Core.Models;
using Core.Models.MetricModel.Pentagon;
using Core.Results;

namespace Application.Services.MetricAnalytic
{
    public interface IPentagonService
    {
        Task<Result<MetricPentagonResponse>> GetSportsmanPentagonAsync(long sportsmanId, Filter? filter = null);
    }
}