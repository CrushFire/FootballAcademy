namespace Core.Models.MetricModel.Coefficientes
{
    public class PositionCoefficientData
    {
        public long Id { get; set; }
        public string Position { get; set; } = string.Empty;
        public double SpeedWeight { get; set; }
        public double SprintWeight { get; set; }
        public double PowerWeight { get; set; }
        public double EnduranceWeight { get; set; }
        public double ExplosiveWeight { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
