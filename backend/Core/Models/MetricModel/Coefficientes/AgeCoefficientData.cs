namespace Core.Models.MetricModel.Coefficientes
{
    public class AgeCoefficientData
    {
        public long Id { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double SpeedFactor { get; set; }
        public double PowerFactor { get; set; }
        public double EnduranceFactor { get; set; }
        public double ExplosiveFactor { get; set; }
        public int MaxHeartRateBase { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
