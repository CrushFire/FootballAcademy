namespace Core.Models.MetricModel.Medical;

public class PlayerMedicalCheckResult
{
    public bool IsHealthy { get; set; }
    public bool CardiovascularOk { get; set; }
    public bool LoadOk { get; set; }
    public bool RecoveryOk { get; set; }
    public bool InjuryRiskOk { get; set; }
    public bool FatigueOk { get; set; }
    public bool ConsistencyOk { get; set; }
    public List<string> Issues { get; set; } = new();
}
