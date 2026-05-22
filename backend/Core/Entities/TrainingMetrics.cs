namespace Core.Entities
{
    public class TrainingMetrics
    {
        public long Id { get; set; }
        public long TrainingId { get; set; }
        public long SportsmanId { get; set; }
        public int Duration { get; set; }
        public int TotalDistance { get; set; }
        public int LowSpeedDistance { get; set; }
        public int ModerateSpeedDistance { get; set; }
        public int HighSpeedDistance { get; set; }
        public int SprintDistance { get; set; }
        public int TimeInSpeedZone1 { get; set; }
        public int TimeInSpeedZone2 { get; set; }
        public int TimeInSpeedZone3 { get; set; }
        public int TimeInSpeedZone4 { get; set; }
        public int TimeInSpeedZone5 { get; set; }
        public int TimeInSpeedZone6 { get; set; }
        public int TimeInSpeedZone7 { get; set; }
        public int DistanceInSpeedZone1 { get; set; }
        public int DistanceInSpeedZone2 { get; set; }
        public int DistanceInSpeedZone3 { get; set; }
        public int DistanceInSpeedZone4 { get; set; }
        public int DistanceInSpeedZone5 { get; set; }
        public int DistanceInSpeedZone6 { get; set; }
        public int DistanceInSpeedZone7 { get; set; }
        public double AverageSpeed { get; set; }
        public double MaximumSpeed { get; set; }
        public int AccelerationCount { get; set; }
        public int DecelerationCount { get; set; }
        public double MaxAcceleration { get; set; }
        public double MaxDeceleration { get; set; }
        public int SprintEfforts { get; set; }
        public int HighSpeedEfforts { get; set; }
        public double PlayerLoad { get; set; }
        public double Energy { get; set; }
        public double WorkRatio { get; set; }
        public double MetabolicPower { get; set; }
        public int Impacts { get; set; }
        public int AverageSatellites { get; set; }
        public double AverageHDOP { get; set; }
        public string? Position { get; set; }
        public string? PositionGroup { get; set; }
        public int ExplosiveEfforts { get; set; }
        public int AverageHeartRate { get; set; }
        public int MaxHeartRate { get; set; }
        public double HeartRateExertion { get; set; }
        public int TimeInHRZone1 { get; set; }
        public int TimeInHRZone2 { get; set; }
        public int TimeInHRZone3 { get; set; }
        public int TimeInHRZone4 { get; set; }
        public int TimeInHRZone5 { get; set; }
        public int TimeInHRZone6 { get; set; }
        public int TimeInHRRedZone { get; set; }
        public DateTime CreatedAt { get; set; }
        public Training Training { get; set; } = null!;
        public Sportsman Sportsman { get; set; } = null!;
    }
}
