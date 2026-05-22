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
            public double MaxDistancePerMinuteStandard { get; set; }      // м/мин, средняя к тренировке
            public double MaxPlayerLoadStandard { get; set; }             // усл. ед., среднее за тренировку (device-specific)
            public double SprintRatioStandard { get; set; }               // доля высокоинтенсивной активности
            public double ExplosiveContributionStandard { get; set; }     // ExplosiveEfforts / SprintEfforts
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
                MaxDistancePerMinuteStandard = entry.MaxDistancePerMinuteStandard,
                MaxPlayerLoadStandard = entry.MaxPlayerLoadStandard,
                SprintRatioStandard = entry.SprintRatioStandard,
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
                MaxDistancePerMinuteStandard = baseData.MaxDistancePerMinuteStandard * m.MaxDistancePerMinute,
                MaxPlayerLoadStandard = baseData.MaxPlayerLoadStandard * m.MaxPlayerLoad,
                SprintRatioStandard = baseData.SprintRatioStandard * m.SprintRatio,
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
                MaxDistancePerMinuteStandard = d.MaxDistancePerMinuteStandard,
                MaxPlayerLoadStandard = d.MaxPlayerLoadStandard,
                SprintRatioStandard = d.SprintRatioStandard,
                ExplosiveContributionStandard = d.ExplosiveContributionStandard,
            };
        }
    }
}
