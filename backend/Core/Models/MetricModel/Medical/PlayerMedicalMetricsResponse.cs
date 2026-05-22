namespace Core.Models.MetricModel.Medical;

public class PlayerMedicalMetricsResponse
{
    public long SportsmanId { get; set; }
    public string SportsmanName { get; set; } = string.Empty;
    public PlayerMedicalMetricsAllFormulas Metrics { get; set; } = new();
}
