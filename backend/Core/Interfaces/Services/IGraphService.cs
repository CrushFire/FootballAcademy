using Core.Models;
using Core.Models.MetricModel.Graph;

namespace Core.Interfaces.Services
{
    public interface IGraphService
    {
        Task<List<PlayerGraphPoint>> GetGraphMetricsAsync(long sportsmanId, Filter? filter);
    }
}