using Application.Services.Coefficientes;
using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Models;
using Core.Models.MetricModel;
using Core.Models.MetricModel.Calculated;
using Core.Models.MetricModel.Coefficientes;
using Core.Models.MetricModel.Pentagon;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.MetricAnalytic
{
    public class PentagonService : IPentagonService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public PentagonService(ApplicationDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        // Сырые показатели спортсмена в реальных единицах (до нормализации)
        private sealed class PentagonRawScores
        {
            public double Speed { get; set; }
            public double Power { get; set; }
            public double Sprints { get; set; }
            public double Endurance { get; set; }
            public double Explosive { get; set; }
        }

        public async Task<Result<MetricPentagonResponse>> GetSportsmanPentagonAsync(long sportsmanId, Filter? filter = null)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<MetricPentagonResponse>.Failure("Спортсмен не найден", 404);

            // Получаем СЫРОЙ список тренировок — нужен для AvgTop3 (стабильность пика скорости).
            var rawList = await GetRawMetricsListAsync(sportsmanId, filter);
            if (rawList.Count == 0)
                return Result<MetricPentagonResponse>.Failure("Нет данных о тренировках", 404);

            var aggregated = _mapper.Map<TrainingMetrics>(rawList);
            var baseMetrics = BaseMetricFactory.CreateBaseMetrics(aggregated);
            var context = await BuildContextAsync(baseMetrics, sportsman, filter);

            var raw = CalculateRawPentagon(context, aggregated, rawList);
            var normalized = NormalizePentagon(context, raw);

            return Result<MetricPentagonResponse>.Success(new MetricPentagonResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                Pentagon = normalized
            });
        }

        // ─── Композитные формулы 5 осей ─────────────────────────────────────────
        // Каждая ось возвращает уже нормированную (~0..1+) сумму взвешенных компонентов.
        // Стандарты берутся из AbsoluteStandards (по возрасту). Дальше идёт в NormalizeHybrid.
        // Обоснование весов: Обоснование_коэффициентов/ФормулыПятиугольника_Обоснование.txt

        // raw остаётся параметром (может пригодиться для других композитов в будущем).
        private double CalculateSpeedComposite(TrainingMetrics m, List<TrainingMetrics> raw, MetricContext ctx)
        {
            var c = CoefficientsConfigProvider.Current.SpeedComposition;
            var std = ctx.AgeGroup;

            return c.MaxSpeedWeight       * SafeDivide(m.MaximumSpeed,          std.AbsoluteMaxSpeed)
                 + c.AvgSpeedWeight       * SafeDivide(m.AverageSpeed,          std.AbsoluteAvgSpeed)
                 + c.HighSpeedRatioWeight * SafeDivide(ctx.Base.HighSpeedRatio, std.AbsoluteHighSpeedRatio);
        }

        private double CalculatePowerComposite(TrainingMetrics m, MetricContext ctx)
        {
            var c = CoefficientsConfigProvider.Current.PowerComposition;
            var std = ctx.AgeGroup;

            // Каждый компонент нормируется к своему стандарту (одинаковые единицы):
            // PlayerLoad / PlayerLoadStandard (у.е.), Energy / EnergyStandard (kJ),
            // MetabolicPower / MetabolicPowerStandard (W/kg).
            // Литература: Catapult (PlayerLoad), Osgnach 2010 (Energy/MetabolicPower).
            return c.PlayerLoadWeight     * SafeDivide(m.PlayerLoad,     std.AbsoluteMaxPlayerLoad)
                 + c.EnergyWeight         * SafeDivide(m.Energy,         std.AbsoluteEnergy)
                 + c.MetabolicPowerWeight * SafeDivide(m.MetabolicPower, std.AbsoluteMetabolicPower);
        }

        private double CalculateSprintsComposite(TrainingMetrics m, MetricContext ctx)
        {
            var c = CoefficientsConfigProvider.Current.SprintsComposition;
            var std = ctx.AgeGroup;

            double sprintsPerMin = m.Duration > 0 ? m.SprintEfforts / (m.Duration / 60.0) : 0.0;
            double sprintMaxSpeed = m.MaximumSpeed;

            // Каждый компонент нормируется к своему стандарту:
            // SprintRatio / SprintRatioStandard (доля), sprintsPerMin / SprintEffortsPerMinStandard (1/мин),
            // sprintMaxSpeed / MaxSpeedStandard (км/ч).
            return c.SprintRatioWeight         * SafeDivide(Math.Clamp(ctx.Base.SprintRatio, 0.0, 1.0), std.AbsoluteMaxSprintRatio)
                 + c.SprintEffortsPerMinWeight * SafeDivide(sprintsPerMin,                              std.AbsoluteSprintEffortsPerMin)
                 + c.SprintMaxSpeedWeight      * SafeDivide(sprintMaxSpeed,                             std.AbsoluteMaxSpeed);
        }

        private double CalculateEnduranceComposite(TrainingMetrics m, MetricContext ctx)
        {
            var c = CoefficientsConfigProvider.Current.EnduranceComposition;
            var std = ctx.AgeGroup;
            var penalty = CoefficientsConfigProvider.Current.PentagonNormalization.EndurancePenaltyPerRedZonePercent;
            var hrRedPercent = NormalizePercent(ctx.Base.HRRedPercent);

            // AerobicLoad, HRStability, LowIntensityRatio уже в диапазоне 0..1 → норма = 1.0 (целевое значение).
            // Чем ближе к 1.0, тем лучше выносливость.
            double composite = c.DistancePerMinuteWeight * SafeDivide(ctx.Base.DistancePerMinute, std.AbsoluteMaxDistancePerMinute)
                             + c.AerobicLoadWeight       * ctx.Base.AerobicLoad
                             + c.HRStabilityWeight       * ctx.Base.HRStability
                             + c.LowIntensityRatioWeight * ctx.Base.LowIntensityRatio;

            // Штраф за время в красной зоне ЧСС (как было раньше).
            return composite * (1.0 - penalty * hrRedPercent);
        }

        private double CalculateExplosiveComposite(TrainingMetrics m, MetricContext ctx)
        {
            var c = CoefficientsConfigProvider.Current.ExplosiveComposition;
            var std = ctx.AgeGroup;

            double explosivePerMin = m.Duration > 0 ? m.ExplosiveEfforts / (m.Duration / 60.0) : 0.0;
            double accDecPerSec    = m.Duration > 0 ? (m.AccelerationCount + m.DecelerationCount) / (double)m.Duration : 0.0;

            return c.ExplosiveEffortsPerMinWeight * SafeDivide(explosivePerMin, std.AbsoluteMaxExplosiveContribution)
                 + c.MaxAccelerationWeight        * SafeDivide(m.MaxAcceleration, c.MaxAccelerationStandard)
                 + c.AccelDecelPerSecWeight       * SafeDivide(accDecPerSec, c.AccelDecelPerSecStandard);
        }

        // Считаем реальные показатели спортсмена без нормализации
        private PentagonRawScores CalculateRawPentagon(MetricContext ctx, TrainingMetrics m, List<TrainingMetrics> raw)
        {
            return new PentagonRawScores
            {
                // Все 5 осей теперь композитные. Возвращают значение ~0..1+, дальше идёт в NormalizeHybrid.
                Speed     = CalculateSpeedComposite(m, raw, ctx),
                Power     = CalculatePowerComposite(m, ctx),
                Sprints   = CalculateSprintsComposite(m, ctx),
                Endurance = CalculateEnduranceComposite(m, ctx),
                Explosive = CalculateExplosiveComposite(m, ctx)
            };
        }

        // Финальная нормализация: raw уже композитная доля (~0..1+).
        // Делим на positionWeight (даёт «процент для своей позиции») и clamp 0..1.
        private PentagonScores NormalizePentagon(MetricContext ctx, PentagonRawScores raw)
        {
            return new PentagonScores
            {
                Speed     = ApplyPositionAndClamp(raw.Speed,     ctx.Position.SpeedWeight),
                Power     = ApplyPositionAndClamp(raw.Power,     ctx.Position.PowerWeight),
                Sprints   = ApplyPositionAndClamp(raw.Sprints,   ctx.Position.SprintWeight),
                Endurance = ApplyPositionAndClamp(raw.Endurance, ctx.Position.EnduranceWeight),
                Explosive = ApplyPositionAndClamp(raw.Explosive, ctx.Position.ExplosiveWeight)
            };
        }

        // Применяем позиционный вес как «важность качества для позиции»:
        // композит × weight. Низкий weight (GK SpeedWeight=0.65) → даже отличный показатель
        // снижается до 65% максимум. Высокий weight (Winger=1.0) → composite не меняется.
        // Это даёт реалистичную оценку: вратарь не должен «дотягивать» до полевых по скорости.
        private static double ApplyPositionAndClamp(double composite, double positionWeight)
        {
            return Math.Clamp(composite * positionWeight, 0.0, 1.0);
        }

        // Relative: actual / (groupStandard * positionWeight)          — группа уже возрастная
        // Absolute: actual / (absoluteStandard * positionWeight)         — стандарт уже возрастной (GetByAge)
        // ageFactor не применяется: оба стандарта уже привязаны к возрасту
        // Итог: weighted average relative/absolute, clamp 0..1
        private static double NormalizeHybrid(
            double actual,
            double groupStandard,
            double absoluteStandard,
            double positionWeight,
            int groupCount)
        {
            var (relW, absW) = GetNormalizationWeights(groupCount);

            var adjustedGroup    = groupStandard    * positionWeight;
            var adjustedAbsolute = absoluteStandard * positionWeight;

            var relScore = SafeDivide(actual, adjustedGroup);
            var absScore = SafeDivide(actual, adjustedAbsolute);

            return Math.Clamp(relW * relScore + absW * absScore, 0.0, 1.0);
        }

        // Чем больше спортсменов в группе — тем больше доверяем групповому стандарту
        private static (double Relative, double Absolute) GetNormalizationWeights(int groupCount)
        {
            var p = CoefficientsConfigProvider.Current.PentagonNormalization;
            if (groupCount >= p.HighGroupCount)   return (p.HighWeights.Relative,   p.HighWeights.Absolute);
            if (groupCount >= p.MediumGroupCount) return (p.MediumWeights.Relative, p.MediumWeights.Absolute);
            if (groupCount >= p.LowGroupCount)    return (p.LowWeights.Relative,    p.LowWeights.Absolute);
            return (p.FallbackWeights.Relative, p.FallbackWeights.Absolute);
        }

        private async Task<TrainingMetrics?> GetAggregatedMetricsAsync(long sportsmanId, Filter? filter)
        {
            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter);

            var metrics = await query.ToListAsync();
            return metrics.Any() ? _mapper.Map<TrainingMetrics>(metrics) : null;
        }

        // Сырой список тренировок (без агрегации) — нужен для AvgTop3 MaxSpeed.
        private async Task<List<TrainingMetrics>> GetRawMetricsListAsync(long sportsmanId, Filter? filter)
        {
            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter);

            return await query.ToListAsync();
        }

        private async Task<MetricContext> BuildContextAsync(BaseMetricModel baseMetrics, Sportsman sportsman, Filter? filter)
        {
            var ageCoeff = AgeCoefficientService.GetByAgeAndGender(sportsman.Age, sportsman.Gender);
            var posCoeff = PositionCoefficientService.GetByPosition(sportsman.Position) ?? PositionCoefficientService.GetDefault();
            var ageGroupNorm = await GetAgeGroupNormalizationAsync(filter, sportsman.Age, sportsman.Gender);

            return new MetricContext
            {
                Base = baseMetrics,
                Age = ageCoeff,
                Position = new PositionCoefficient
                {
                    SpeedWeight = posCoeff.SpeedWeight,
                    SprintWeight = posCoeff.SprintWeight,
                    PowerWeight = posCoeff.PowerWeight,
                    EnduranceWeight = posCoeff.EnduranceWeight,
                    ExplosiveWeight = posCoeff.ExplosiveWeight
                },
                AgeGroup = ageGroupNorm
            };
        }

        private async Task<AgeGroupNormalization> GetAgeGroupNormalizationAsync(Filter? filter, int sportsmanAge, char gender)
        {
            var cacheKey = BuildAgeGroupCacheKey(sportsmanAge, gender, filter);
            if (_cache.TryGetValue(cacheKey, out AgeGroupNormalization? cached) && cached != null)
                return cached;

            var absoluteStandard = AbsoluteStandardService.GetByAgeAndGender(sportsmanAge, gender) ?? AbsoluteStandardService.GetDefault();
            var today = DateTime.Today;

            var maxBirthDate = today.AddYears(-sportsmanAge);
            var minBirthDate = today.AddYears(-(sportsmanAge + 1)).AddDays(1);

            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Include(m => m.Sportsman)
                .Where(m => m.Sportsman.BirthDate >= minBirthDate && m.Sportsman.BirthDate <= maxBirthDate) // не в фильтр, так как это часть логики, а не фронта, не убирай
                .Where(m => m.Sportsman.Gender == gender) // группа нормализации — только одного пола
                .ApplyFilter(filter);

            var ageGroupMetrics = await query.ToListAsync();

            var sportsmanCount = ageGroupMetrics.Select(m => m.SportsmanId).Distinct().Count();

            // Агрегируем по каждому спортсмену — чтобы стандарт был в тех же единицах, что и actual
            // actual = агрегат за период, значит groupStandard тоже должен быть агрегатом за период
            var aggregatedPerSportsman = ageGroupMetrics
                .GroupBy(m => m.SportsmanId)
                .Select(g => _mapper.Map<TrainingMetrics>(g.ToList()))
                .ToList();

            AgeGroupNormalization result;

            if (aggregatedPerSportsman.Any())
            {
                result = new AgeGroupNormalization
                {
                    MaxSpeed = PositiveOrFallback(
                        aggregatedPerSportsman.Max(m => m.MaximumSpeed),
                        absoluteStandard.MaxSpeedStandard),

                    MaxDistancePerMinute = PositiveOrFallback(
                        aggregatedPerSportsman.Max(GetDistancePerMinute),
                        absoluteStandard.MaxDistancePerMinuteStandard),

                    MaxPlayerLoad = PositiveOrFallback(
                        aggregatedPerSportsman.Max(m => m.PlayerLoad),
                        absoluteStandard.MaxPlayerLoadStandard),

                    MaxSprintRatio = PositiveOrFallback(
                        aggregatedPerSportsman.Max(m => Math.Clamp(BaseMetricFactory.CreateBaseMetrics(m).SprintRatio, 0.0, 1.0)),
                        absoluteStandard.SprintRatioStandard),

                    MaxExplosiveContribution = PositiveOrFallback(
                        aggregatedPerSportsman.Max(GetExplosiveContribution),
                        absoluteStandard.ExplosiveContributionStandard),

                    AbsoluteMaxSpeed = absoluteStandard.MaxSpeedStandard,
                    AbsoluteEnergy = absoluteStandard.EnergyStandard,
                    AbsoluteMetabolicPower = absoluteStandard.MetabolicPowerStandard,
                    AbsoluteSprintEffortsPerMin = absoluteStandard.SprintEffortsPerMinStandard,
                    AbsoluteAvgSpeed = absoluteStandard.AvgSpeedStandard,
                    AbsoluteAvgTop3Speed = absoluteStandard.AvgTop3SpeedStandard,
                    AbsoluteHighSpeedRatio = absoluteStandard.HighSpeedRatioStandard,
                    AbsoluteMaxDistancePerMinute = absoluteStandard.MaxDistancePerMinuteStandard,
                    AbsoluteMaxPlayerLoad = absoluteStandard.MaxPlayerLoadStandard,
                    AbsoluteMaxSprintRatio = absoluteStandard.SprintRatioStandard,
                    AbsoluteMaxExplosiveContribution = absoluteStandard.ExplosiveContributionStandard,

                    SportsmanCountInGroup = sportsmanCount
                };
            }
            else
            {
                result = new AgeGroupNormalization
                {
                    MaxSpeed = absoluteStandard.MaxSpeedStandard,
                    MaxDistancePerMinute = absoluteStandard.MaxDistancePerMinuteStandard,
                    MaxPlayerLoad = absoluteStandard.MaxPlayerLoadStandard,
                    MaxSprintRatio = absoluteStandard.SprintRatioStandard,
                    MaxExplosiveContribution = absoluteStandard.ExplosiveContributionStandard,

                    AbsoluteMaxSpeed = absoluteStandard.MaxSpeedStandard,
                    AbsoluteEnergy = absoluteStandard.EnergyStandard,
                    AbsoluteMetabolicPower = absoluteStandard.MetabolicPowerStandard,
                    AbsoluteSprintEffortsPerMin = absoluteStandard.SprintEffortsPerMinStandard,
                    AbsoluteAvgSpeed = absoluteStandard.AvgSpeedStandard,
                    AbsoluteAvgTop3Speed = absoluteStandard.AvgTop3SpeedStandard,
                    AbsoluteHighSpeedRatio = absoluteStandard.HighSpeedRatioStandard,
                    AbsoluteMaxDistancePerMinute = absoluteStandard.MaxDistancePerMinuteStandard,
                    AbsoluteMaxPlayerLoad = absoluteStandard.MaxPlayerLoadStandard,
                    AbsoluteMaxSprintRatio = absoluteStandard.SprintRatioStandard,
                    AbsoluteMaxExplosiveContribution = absoluteStandard.ExplosiveContributionStandard,

                    SportsmanCountInGroup = 0
                };
            }

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        // Полный ключ кэша — все значимые поля Filter, чтобы не вернуть неправильную норму при разных фильтрах
        private static string BuildAgeGroupCacheKey(int sportsmanAge, char gender, Filter? filter)
        {
            if (filter?.Filters == null)
                return $"agegroup_norm_{sportsmanAge}_{gender}_nofilter";

            var f = filter.Filters;
            return string.Join("_",
                "agegroup_norm",
                sportsmanAge,
                gender,
                f.Date?.From?.ToString("yyyyMMdd") ?? "null",
                f.Date?.To?.ToString("yyyyMMdd") ?? "null",
                f.TrainerId.Any() ? string.Join(",", f.TrainerId) : "null",
                f.GroupId.Any() ? string.Join(",", f.GroupId) : "null",
                f.TeamId.Any() ? string.Join(",", f.TeamId) : "null",
                f.Type.Any() ? string.Join(",", f.Type) : "null"
            );
        }

        // Helpers

        private static double GetDistancePerMinute(TrainingMetrics m) =>
            m.Duration > 0 ? m.TotalDistance / (m.Duration / 60.0) : 0.0;

        // «Взрывная» = частота взрывных эпизодов в минуту.
        // Взрывные действия (резкие ускорения/торможения по производной скорости) — независимое множество
        // от спринтов (попадание в 7-ю зону скорости). Поэтому отношение Explosive/Sprint некорректно.
        // Берём абсолютную частоту: ExplosiveEfforts / минуты. Норматив см. ExplosiveContributionStandard (теперь /мин).
        private static double GetExplosiveContribution(TrainingMetrics m) =>
            m.Duration > 0 ? m.ExplosiveEfforts / (m.Duration / 60.0) : 0.0;

        private static double SafeDivide(double numerator, double denominator) =>
            denominator > 0 ? numerator / denominator : 0.0;

        private static double PositiveOrFallback(double value, double fallback) =>
            value > 0 ? value : fallback;

        private static double NormalizePercent(double value)
        {
            var normalized = value > 1.0 ? value / 100.0 : value;
            return Math.Clamp(normalized, 0.0, 1.0);
        }
    }
}
