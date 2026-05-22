namespace Core.Models.MetricModel.Medical;

public class PlayerMedicalCheckResponse
{
    public long SportsmanId { get; set; }
    public string SportsmanName { get; set; } = string.Empty;
    public PlayerMedicalCheckResult CheckResult { get; set; } = new();
}
