using System.Linq;

namespace Application.Services.Coefficientes
{
    public static class AbsoluteStandardService
    {
        public class AbsoluteStandardData
        {
            public int MinAge { get; set; }
            public int MaxAge { get; set; }
            public double MaxSpeedStandard { get; set; }                  // км/ч, пиковая скорость
            public double AvgSpeedStandard { get; set; }                  // км/ч, средний темп за тренировку
            public double AvgTop3SpeedStandard { get; set; }              // км/ч, среднее 3 лучших пиков
            public double HighSpeedRatioStandard { get; set; }            // доля high-speed дист (>19.8 км/ч)
            public double MaxDistancePerMinuteStandard { get; set; }      // м/мин, средняя к тренировке
            public double MaxPlayerLoadStandard { get; set; }             // усл. ед., среднее за тренировку
            public double EnergyStandard { get; set; }                    // kJ, расход энергии за тренировку
            public double MetabolicPowerStandard { get; set; }            // W/kg, средняя метаб. мощность
            public double SprintRatioStandard { get; set; }               // доля спринт-дистанции от total
            public double SprintEffortsPerMinStandard { get; set; }       // спринт-эпизодов в минуту
            public double ExplosiveContributionStandard { get; set; }     // взрывных эпизодов в минуту
        }

        public static AbsoluteStandardData? GetByAge(int age)
        {
            var entry = CoefficientsConfigProvider.Current.AbsoluteStandards.ByAge
                .FirstOrDefault(e => age >= e.MinAge && age <= e.MaxAge);
            if (entry == null) return null;
            return new AbsoluteStandardData
            {
                MinAge = entry.MinAge,
                MaxAge = entry.MaxAge,
                MaxSpeedStandard = entry.MaxSpeedStandard,
                AvgSpeedStandard = entry.AvgSpeedStandard,
                AvgTop3SpeedStandard = entry.AvgTop3SpeedStandard,
                HighSpeedRatioStandard = entry.HighSpeedRatioStandard,
                MaxDistancePerMinuteStandard = entry.MaxDistancePerMinuteStandard,
                MaxPlayerLoadStandard = entry.MaxPlayerLoadStandard,
                EnergyStandard = entry.EnergyStandard,
                MetabolicPowerStandard = entry.MetabolicPowerStandard,
                SprintRatioStandard = entry.SprintRatioStandard,
                SprintEffortsPerMinStandard = entry.SprintEffortsPerMinStandard,
                ExplosiveContributionStandard = entry.ExplosiveContributionStandard,
            };
        }

        // Для женщин снижаем потолки по множителям из конфига.
        public static AbsoluteStandardData? GetByAgeAndGender(int age, char gender)
        {
            var baseData = GetByAge(age);
            if (baseData == null || gender != 'F') return baseData;

            var m = CoefficientsConfigProvider.Current.AbsoluteStandards.FemaleMultipliers;
            return new AbsoluteStandardData
            {
                MinAge = baseData.MinAge,
                MaxAge = baseData.MaxAge,
                MaxSpeedStandard = baseData.MaxSpeedStandard * m.MaxSpeed,
                AvgSpeedStandard = baseData.AvgSpeedStandard * m.AvgSpeed,
                AvgTop3SpeedStandard = baseData.AvgTop3SpeedStandard * m.AvgTop3Speed,
                HighSpeedRatioStandard = baseData.HighSpeedRatioStandard * m.HighSpeedRatio,
                MaxDistancePerMinuteStandard = baseData.MaxDistancePerMinuteStandard * m.MaxDistancePerMinute,
                MaxPlayerLoadStandard = baseData.MaxPlayerLoadStandard * m.MaxPlayerLoad,
                EnergyStandard = baseData.EnergyStandard * m.Energy,
                MetabolicPowerStandard = baseData.MetabolicPowerStandard * m.MetabolicPower,
                SprintRatioStandard = baseData.SprintRatioStandard * m.SprintRatio,
                SprintEffortsPerMinStandard = baseData.SprintEffortsPerMinStandard * m.SprintEffortsPerMin,
                ExplosiveContributionStandard = baseData.ExplosiveContributionStandard * m.ExplosiveContribution,
            };
        }

        public static AbsoluteStandardData GetDefault()
        {
            var d = CoefficientsConfigProvider.Current.AbsoluteStandards.Default;
            return new AbsoluteStandardData
            {
                MinAge = d.MinAge,
                MaxAge = d.MaxAge,
                MaxSpeedStandard = d.MaxSpeedStandard,
                AvgSpeedStandard = d.AvgSpeedStandard,
                AvgTop3SpeedStandard = d.AvgTop3SpeedStandard,
                HighSpeedRatioStandard = d.HighSpeedRatioStandard,
                MaxDistancePerMinuteStandard = d.MaxDistancePerMinuteStandard,
                MaxPlayerLoadStandard = d.MaxPlayerLoadStandard,
                EnergyStandard = d.EnergyStandard,
                MetabolicPowerStandard = d.MetabolicPowerStandard,
                SprintRatioStandard = d.SprintRatioStandard,
                SprintEffortsPerMinStandard = d.SprintEffortsPerMinStandard,
                ExplosiveContributionStandard = d.ExplosiveContributionStandard,
            };
        }
    }
}
