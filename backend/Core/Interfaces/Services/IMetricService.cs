using Core.Models;
using Core.Models.MetricModel;
using Core.Results;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Services
{
    public interface IMetricsService
    {
        Task<Result<MetricResponse>> CreateMetricAsync(MetricCreateRequest req);
        Task<Result<bool>> DeleteMetricAsync(long metricId);
        Task<Result<MetricResponse>> GetMetricAsync(long metricId);
        Task<Result<List<MetricResponse>>> GetMetricsAsync(Filter? filter);
        Task<Result<MetricResponse>> GetMetricByTrainingSportsmanAsync(long trainingId, long sportsmanId);

        Task<Result<MetricResponse>> UpdateMetricAsync(MetricUpdateRequest req, long metricId);
        Task<Result<string>> ImportMetricsFromFilesAsync(List<IFormFile> files);
        Task<Result<string>> ImportMetricsFromFileAsync(string filePath);
    }
}