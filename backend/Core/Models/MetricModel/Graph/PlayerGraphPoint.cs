using Core.Models.MetricModel;

namespace Core.Models.MetricModel.Graph;

public class PlayerGraphPoint
{
    public DateTime Date { get; set; }
    public string? Type { get; set; }
    public string? MatchType { get; set; }
    public CommonMetricsAllFormulas Metrics { get; set; } = new();
}
