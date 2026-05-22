using Core.Models;
using Core.Models.MetricModel.Medical;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IMedicalMetricService
    {
        Task<Result<List<PlayerMedicalCheckResponse>>> GetAllSportsmenMedicalCheckAsync(Filter? filter = null);
        Task<Result<PlayerMedicalCheckResponse>> GetSportsmanMedicalCheckAsync(long sportsmanId, Filter? filter = null);
        Task<Result<PlayerMedicalMetricsResponse>> GetSportsmanMedicalMetricsAsync(long sportsmanId, Filter? filter = null);
    }
}