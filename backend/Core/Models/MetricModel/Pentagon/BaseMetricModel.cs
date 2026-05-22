namespace Core.Models.MetricModel
{
    public class BaseMetricModel
    {
        public double DistancePerMinute { get; init; }
        public double SprintRatio { get; init; }
        public double HighSpeedRatio { get; init; }
        public double AccelPerSecond { get; init; }
        public double DecelPerSecond { get; init; }
        public double HighIntensityRatio { get; init; }
        public double LowIntensityRatio { get; init; }
        public double AerobicLoad { get; init; }
        public double MaxSpeedIndex { get; init; }
        public double NeuroLoad { get; init; }
        public double ExplosiveIndex { get; init; }
        public double HRRedPercent { get; init; }
        public double CardioEfficiency { get; init; }
        public double HRStability { get; init; }
    }
}
