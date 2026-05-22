using Core.Models.MetricModel;

namespace Core.Models.MetricModel.Calculated;

public class PlayerCalculatedMetricsResponse
{
    public long SportsmanId { get; set; }
    public string SportsmanName { get; set; } = string.Empty;
    public CommonMetricsAllFormulas Metrics { get; set; } = new();
}
