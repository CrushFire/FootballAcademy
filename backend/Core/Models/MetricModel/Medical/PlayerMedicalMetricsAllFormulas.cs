namespace Core.Models.MetricModel.Medical;

public class PlayerMedicalMetricsAllFormulas
{
    // Cardiovascular metrics
    public int AverageHeartRate { get; set; }
    public int MaxHeartRate { get; set; }
    public double HRStability { get; set; }
    public double HRRedZonePercent { get; set; }
    public double CardioEfficiency { get; set; }
    public double HeartRateExertion { get; set; }

    // Load and fatigue metrics
    public double PlayerLoad { get; set; }
    public double PlayerLoadPerMinute { get; set; }
    public double WorkRatio { get; set; }
    public double Energy { get; set; }
    public double EnergyEfficiency { get; set; }

    // Recovery metrics
    public double RecoveryIndex { get; set; }
    public int TimeInLowIntensity { get; set; }
    public int TimeInHighIntensity { get; set; }
    public double IntensityRatio { get; set; }

    // Physical stress metrics
    public int Impacts { get; set; }
    public double ImpactsPerMinute { get; set; }
    public int ExplosiveEfforts { get; set; }
    public double NeuroMuscularLoad { get; set; }

    // Injury risk indicators
    public double AcuteChronicRatio { get; set; }
    public double HighSpeedExposure { get; set; }
    public double AccelDecelLoad { get; set; }
    public double FatigueIndex { get; set; }
    public double Consistency { get; set; }
}
