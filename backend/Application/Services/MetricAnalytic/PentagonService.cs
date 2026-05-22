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

            var aggregated = await GetAggregatedMetricsAsync(sportsmanId, filter);
            if (aggregated == null)
                return Result<MetricPentagonResponse>.Failure("Нет данных о тренировках", 404);

            var baseMetrics = BaseMetricFactory.CreateBaseMetrics(aggregated);
            var context = await BuildContextAsync(baseMetrics, sportsman, filter);

            var raw = CalculateRawPentagon(context, aggregated);
            var normalized = NormalizePentagon(context, raw);

            return Result<MetricPentagonResponse>.Success(new MetricPentagonResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                Pentagon = normalized
            });
        }

        // Считаем реальные показатели спортсмена без нормализации
        private PentagonRawScores CalculateRawPentagon(MetricContext ctx, TrainingMetrics m)
        {
            var hrRedPercent = NormalizePercent(ctx.Base.HRRedPercent);
            var penalty = CoefficientsConfigProvider.Current.PentagonNormalization.EndurancePenaltyPerRedZonePercent;

            return new PentagonRawScores
            {
                Speed = m.MaximumSpeed,
                Power = m.PlayerLoad,
                Sprints = Math.Clamp(ctx.Base.SprintRatio, 0.0, 1.0),
                Endurance = ctx.Base.DistancePerMinute * (1.0 - penalty * hrRedPercent),
                Explosive = GetExplosiveContribution(m)
            };
        }

        // Гибридная нормализация: actual / standard, где стандарт учитывает возраст и позицию
        private PentagonScores NormalizePentagon(MetricContext ctx, PentagonRawScores raw)
        {
            var groupCount = ctx.AgeGroup.SportsmanCountInGroup;

            return new PentagonScores
            {
                Speed = NormalizeHybrid(
                    actual: raw.Speed,
                    groupStandard: ctx.AgeGroup.MaxSpeed,
                    absoluteStandard: ctx.AgeGroup.AbsoluteMaxSpeed,
                    positionWeight: ctx.Position.SpeedWeight,
                    groupCount: groupCount),

                Power = NormalizeHybrid(
                    actual: raw.Power,
                    groupStandard: ctx.AgeGroup.MaxPlayerLoad,
                    absoluteStandard: ctx.AgeGroup.AbsoluteMaxPlayerLoad,
                    positionWeight: ctx.Position.PowerWeight,
                    groupCount: groupCount),

                Sprints = NormalizeHybrid(
                    actual: raw.Sprints,
                    groupStandard: ctx.AgeGroup.MaxSprintRatio,
                    absoluteStandard: ctx.AgeGroup.AbsoluteMaxSprintRatio,
                    positionWeight: ctx.Position.SprintWeight,
                    groupCount: groupCount),

                Endurance = NormalizeHybrid(
                    actual: raw.Endurance,
                    groupStandard: ctx.AgeGroup.MaxDistancePerMinute,
                    absoluteStandard: ctx.AgeGroup.AbsoluteMaxDistancePerMinute,
                    positionWeight: ctx.Position.EnduranceWeight,
                    groupCount: groupCount),

                Explosive = NormalizeHybrid(
                    actual: raw.Explosive,
                    groupStandard: ctx.AgeGroup.MaxExplosiveContribution,
                    absoluteStandard: ctx.AgeGroup.AbsoluteMaxExplosiveContribution,
                    positionWeight: ctx.Position.ExplosiveWeight,
                    groupCount: groupCount)
            };
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

        private static double GetExplosiveContribution(TrainingMetrics m) =>
            m.SprintEfforts > 0
                ? Math.Clamp(m.ExplosiveEfforts / (double)m.SprintEfforts, 0.0, 1.0)
                : 0.0;

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
