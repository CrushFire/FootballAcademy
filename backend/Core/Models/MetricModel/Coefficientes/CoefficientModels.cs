namespace Core.Models.MetricModel.Coefficientes
{
    public class MaleAgeCoefficient
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double SpeedFactor { get; set; }
        public double PowerFactor { get; set; }
        public double EnduranceFactor { get; set; }
        public double ExplosiveFactor { get; set; }
        public int MaxHeartRateBase { get; set; }
    }

    public class FemaleAgeCoefficient
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double SpeedFactor { get; set; }
        public double PowerFactor { get; set; }
        public double EnduranceFactor { get; set; }
        public double ExplosiveFactor { get; set; }
        public int MaxHeartRateBase { get; set; }
    }

    public class PositionCoefficient
    {
        public double SpeedWeight { get; set; }
        public double SprintWeight { get; set; }
        public double PowerWeight { get; set; }
        public double EnduranceWeight { get; set; }
        public double ExplosiveWeight { get; set; }
    }
}
