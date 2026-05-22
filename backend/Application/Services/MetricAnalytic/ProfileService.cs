using Application.Utils;
using Application.Services.Coefficientes;
using AutoMapper;
using Core.Entities;
using Core.Models;
using Core.Models.MetricModel.Profile;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces.Services;

namespace Application.Services.MetricAnalytic
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProfileService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<MetricPlayerResultResponse>> GetSportsmanProfileAsync(long sportsmanId, Filter? filter = null)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<MetricPlayerResultResponse>.Failure("Спортсмен не найден", 404);

            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter);

            var metrics = await query.ToListAsync();

            if (!metrics.Any())
                return Result<MetricPlayerResultResponse>.Success(new MetricPlayerResultResponse
                {
                    SportsmanId = sportsmanId,
                    SportsmanName = sportsman.FIO,
                    Filter = filter,
                    Profiles = null
                });

            var avgMetric = _mapper.Map<TrainingMetrics>(metrics);
            var result = BuildProfile(avgMetric, sportsman);
            result.Filter = filter;
            return Result<MetricPlayerResultResponse>.Success(result);
        }

        private MetricPlayerResultResponse BuildProfile(TrainingMetrics m, Sportsman sportsman)
        {
            var base_ = BaseMetricFactory.CreateBaseMetrics(m);
            var posCoeff = PositionCoefficientService.GetByPosition(sportsman.Position);

            // Для каждого правила собираем флаги "выполнено / частично выполнено / не выполнено"
            // Активные  — где сработали ВСЕ условия (строгий критерий)
            // Потенциальные — где сработала ЧАСТЬ условий, но не все

            var active = new List<PlayerProfile>();
            var potential = new List<PlayerProfile>();

            void Eval(PlayerProfile profile, params bool[] conditions)
            {
                int hits = conditions.Count(c => c);
                if (hits == 0) return;
                if (hits == conditions.Length) active.Add(profile);
                else potential.Add(profile);
            }

            void EvalSimple(PlayerProfile profile, bool condition)
            {
                if (condition) active.Add(profile);
            }

            // Простые правила (одно условие)
            EvalSimple(PlayerProfile.EnduranceRunner, base_.AerobicLoad > 0.35);
            EvalSimple(PlayerProfile.PowerPlayer, m.PlayerLoad > 300);
            EvalSimple(PlayerProfile.ExplosivePlayer, base_.ExplosiveIndex > 1.2);
            EvalSimple(PlayerProfile.DynamicPlayer, base_.AccelPerSecond > 0.15);
            EvalSimple(PlayerProfile.StaticPlayer, SafeDivide(m.PlayerLoad, m.TotalDistance) > 1.0);
            EvalSimple(PlayerProfile.Goalkeeper, m.Position == "Goalkeeper");

            // Составные правила (несколько условий через OR — активный если все)
            Eval(PlayerProfile.Sprinter,
                base_.SprintRatio > 0.45,
                base_.AccelPerSecond > 0.15);

            Eval(PlayerProfile.FlankPlayer,
                base_.SprintRatio > 0.4,
                base_.AccelPerSecond > 0.12,
                base_.DistancePerMinute > 100,
                posCoeff.SpeedWeight >= 0.75);

            double defenderIndex = SafeDivide(m.PlayerLoad, m.TotalDistance) + base_.AccelPerSecond + base_.DecelPerSecond;
            Eval(PlayerProfile.DefenderType,
                defenderIndex > 2.0,
                posCoeff.PowerWeight >= 1.0);

            double centralMidIndex = base_.DistancePerMinute / 100.0 * 0.5 + base_.HRStability;
            Eval(PlayerProfile.CentralMidfielder,
                centralMidIndex > 1.2,
                posCoeff.EnduranceWeight >= 1.0);

            double defensiveMidIndex = SafeDivide(m.PlayerLoad, m.Duration / 60.0) / 10.0 + base_.LowIntensityRatio;
            EvalSimple(PlayerProfile.DefensiveMidfielder, defensiveMidIndex > 0.5);

            Eval(PlayerProfile.AttackingMidfielder,
                base_.SprintRatio > 0.35 && base_.SprintRatio <= 0.45,
                base_.HighSpeedRatio > base_.SprintRatio);

            Eval(PlayerProfile.Forward,
                base_.SprintRatio > 0.5,
                posCoeff.SprintWeight >= 0.55);

            Eval(PlayerProfile.Offensive,
                base_.HighIntensityRatio > 0.3,
                m.SprintEfforts > 10);

            EvalSimple(PlayerProfile.Defensive, defenderIndex > 2.5);

            if (active.Count >= 4) active.Add(PlayerProfile.Universal);

            return new MetricPlayerResultResponse
            {
                SportsmanId = sportsman.Id,
                SportsmanName = sportsman.FIO,
                Profiles = (active.Any() || potential.Any())
                    ? new PlayerProfileResult { Profiles = active, PotentialProfiles = potential }
                    : null
            };
        }

        private static double SafeDivide(double a, double b) => b == 0 ? 0 : a / b;
    }
}
