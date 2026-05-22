using Core.Models;
using Core.Models.MetricModel.Calculated;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IMainMetricService
    {
        Task<Result<PlayerCalculatedMetricsResponse>> GetSportsmanAverageMetricsAsync(long sportsmanId, Filter? filter = null);
        Task<Result<PlayerCalculatedMetricsResponse>> GetSportsmanTrainingMetricsAsync(long sportsmanId, long trainingId);
    }
}