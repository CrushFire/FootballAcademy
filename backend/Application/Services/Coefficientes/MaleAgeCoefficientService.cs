using System.Linq;
using Core.Models.MetricModel.Coefficientes;

namespace Application.Services.Coefficientes
{
    public static class MaleAgeCoefficientService
    {
        public static AgeCoefficientData? GetByAge(int age)
        {
            var entry = CoefficientsConfigProvider.Current.AgeCoefficients.Male
                .FirstOrDefault(e => age >= e.MinAge && age <= e.MaxAge);
            if (entry == null) return null;
            return new AgeCoefficientData
            {
                MinAge = entry.MinAge,
                MaxAge = entry.MaxAge,
                SpeedFactor = entry.SpeedFactor,
                PowerFactor = entry.PowerFactor,
                EnduranceFactor = entry.EnduranceFactor,
                ExplosiveFactor = entry.ExplosiveFactor,
                MaxHeartRateBase = entry.MaxHeartRateBase,
            };
        }

        public static AgeCoefficientData GetDefault()
        {
            var d = CoefficientsConfigProvider.Current.AgeCoefficients.MaleDefault;
            return new AgeCoefficientData
            {
                MinAge = 0,
                MaxAge = 100,
                SpeedFactor = d.SpeedFactor,
                PowerFactor = d.PowerFactor,
                EnduranceFactor = d.EnduranceFactor,
                ExplosiveFactor = d.ExplosiveFactor,
                MaxHeartRateBase = d.MaxHeartRateBase,
            };
        }
    }
}
