using Core.Entities;
using Core.Models.MetricModel;
using Core.Models.MetricModel.Medical;

namespace Application.Utils
{
    public static class BaseMetricFactory
    {
        private static double SafeDivide(double numerator, double denominator)
             => denominator == 0 ? 0 : numerator / denominator;//���������� �������
        public static CommonMetricsAllFormulas CreateMetrics(TrainingMetrics m, List<double> last5)
        {
            double duration = m.Duration / 60.0;
            double mean = last5.Count >= 3 ? last5.Average() : 0;
            int lowZones = m.TimeInSpeedZone1 + m.TimeInSpeedZone2 + m.TimeInSpeedZone3;
            int highZones = m.TimeInSpeedZone5 + m.TimeInSpeedZone6 + m.TimeInSpeedZone7;

            return new CommonMetricsAllFormulas
            {
                MaxSpeed = m.MaximumSpeed,
                // AvgSpeed (км/ч) приходит из XLSX от GPS-трекера — доверяем его собственному расчёту,
                // не считаем заново через TotalDistance/Duration.
                AvgSpeed = m.AverageSpeed,
                SprintRatio = SafeDivide(m.SprintDistance, m.TotalDistance),
                HighSpeedRatio = SafeDivide(m.HighSpeedDistance, m.TotalDistance),
                PlayerLoad = m.PlayerLoad,
                HRRedPercent = SafeDivide(m.TimeInHRRedZone, m.Duration),
                AerobicLoad = SafeDivide(m.LowSpeedDistance + m.ModerateSpeedDistance, m.TotalDistance),
                ExplosiveIndex = SafeDivide(m.ExplosiveEfforts + m.AccelerationCount + m.DecelerationCount, m.Duration),
                RSA = m.TotalDistance > 0 ? m.SprintEfforts * SafeDivide(m.SprintDistance, m.TotalDistance) : 0,
                CardioEfficiency = SafeDivide(m.TotalDistance, m.AverageHeartRate),
                HRStability = SafeDivide(m.AverageHeartRate, m.MaxHeartRate),
                RecoveryIndex = SafeDivide(m.TimeInSpeedZone1, m.SprintEfforts),
                HI_LO_Ratio = SafeDivide(highZones, lowZones),
                AnaerobicRatio = SafeDivide(m.SprintDistance, m.HighSpeedDistance),
                MetabolicPower = m.MetabolicPower,
                WorkRatio = m.WorkRatio,
                EnergyEfficiency = SafeDivide(m.TotalDistance, m.Energy),
                SprintEffortsPerMin = SafeDivide(m.SprintEfforts, duration),
                FatigueIndex = mean > 0 ? m.PlayerLoad / mean : 0,
                Consistency = mean > 0 ? Math.Sqrt(last5.Average(v => Math.Pow(v - mean, 2))) : 0
            };
        }

        public static PlayerMedicalMetricsAllFormulas CreateMedicalMetrics(TrainingMetrics m, double chronicLoad, List<double> last5)
        {
            int timeInLow = m.TimeInSpeedZone1 + m.TimeInSpeedZone2;
            int timeInHigh = m.TimeInSpeedZone5 + m.TimeInSpeedZone6 + m.TimeInSpeedZone7;
            double mean5 = last5.Count >= 3 ? last5.Average() : 0;
            double fatigueIndex = mean5 > 0 ? m.PlayerLoad / mean5 : 0;
            double consistency = last5.Count >= 3
                ? Math.Sqrt(last5.Average(x => Math.Pow(x - mean5, 2)))
                : 0;

            return new PlayerMedicalMetricsAllFormulas
            {
                AverageHeartRate = m.AverageHeartRate,
                MaxHeartRate = m.MaxHeartRate,
                HRStability = SafeDivide(m.AverageHeartRate, m.MaxHeartRate),
                HRRedZonePercent = SafeDivide(m.TimeInHRRedZone, m.Duration),
                CardioEfficiency = SafeDivide(m.TotalDistance, m.AverageHeartRate),
                HeartRateExertion = m.HeartRateExertion,
                PlayerLoad = m.PlayerLoad,
                PlayerLoadPerMinute = SafeDivide(m.PlayerLoad, m.Duration / 60.0),
                FatigueIndex = fatigueIndex,
                WorkRatio = m.WorkRatio,
                Energy = m.Energy,
                EnergyEfficiency = SafeDivide(m.TotalDistance, m.Energy),
                RecoveryIndex = SafeDivide(m.TimeInSpeedZone1, m.SprintEfforts),
                TimeInLowIntensity = timeInLow,
                TimeInHighIntensity = timeInHigh,
                IntensityRatio = SafeDivide(timeInHigh, timeInLow),
                Impacts = m.Impacts,
                ImpactsPerMinute = SafeDivide(m.Impacts, m.Duration / 60.0),
                ExplosiveEfforts = m.ExplosiveEfforts,
                NeuroMuscularLoad = m.PlayerLoad + m.Impacts + m.ExplosiveEfforts,
                AcuteChronicRatio = SafeDivide(m.PlayerLoad, chronicLoad),
                HighSpeedExposure = SafeDivide(m.HighSpeedDistance + m.SprintDistance, m.TotalDistance),
                AccelDecelLoad = SafeDivide(m.AccelerationCount + m.DecelerationCount, m.Duration),
                Consistency = consistency
            };
        }

        public static BaseMetricModel CreateBaseMetrics(TrainingMetrics m)
        {
            var durationMinutes = m.Duration / 60.0;
            var highIntensity = m.TimeInSpeedZone5 + m.TimeInSpeedZone6 + m.TimeInSpeedZone7;
            var lowIntensity = m.TimeInSpeedZone1 + m.TimeInSpeedZone2;

            return new BaseMetricModel
            {
                DistancePerMinute = SafeDivide(m.TotalDistance, durationMinutes),
                SprintRatio = SafeDivide(m.SprintDistance, m.TotalDistance),
                HighSpeedRatio = SafeDivide(m.HighSpeedDistance, m.TotalDistance),
                AccelPerSecond = SafeDivide(m.AccelerationCount, m.Duration),
                DecelPerSecond = SafeDivide(m.DecelerationCount, m.Duration),
                HighIntensityRatio = SafeDivide(highIntensity, m.Duration),
                LowIntensityRatio = SafeDivide(lowIntensity, m.Duration),
                AerobicLoad = SafeDivide(m.LowSpeedDistance + m.ModerateSpeedDistance, m.TotalDistance),
                MaxSpeedIndex = m.MaximumSpeed,
                NeuroLoad = m.PlayerLoad + m.Impacts,
                ExplosiveIndex = SafeDivide(m.ExplosiveEfforts + m.AccelerationCount + m.DecelerationCount, m.Duration),
                HRRedPercent = SafeDivide(m.TimeInHRRedZone, m.Duration),
                CardioEfficiency = SafeDivide(m.TotalDistance, m.AverageHeartRate),
                HRStability = SafeDivide(m.AverageHeartRate, m.MaxHeartRate)
            };
        }
    }
}
