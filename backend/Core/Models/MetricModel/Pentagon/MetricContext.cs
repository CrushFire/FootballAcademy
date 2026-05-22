using Core.Models.MetricModel.Coefficientes;

namespace Core.Models.MetricModel.Pentagon
{
    public class MetricContext
    {
        public BaseMetricModel Base { get; init; }
        public AgeCoefficientData Age { get; init; }
        public PositionCoefficient Position { get; init; }
        public AgeGroupNormalization AgeGroup { get; init; }
    }
}
