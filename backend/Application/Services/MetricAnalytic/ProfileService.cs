using Application.Utils;
using Application.Services.Coefficientes;
using AutoMapper;
using Core.Entities;
using Core.Enums;
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
            // Пороги профилей читаются из appsettings.Coefficients.json (секция ProfileThresholds).
            // Soft-пороги хранятся отдельно от Hard — можно тонко настроить любой профиль.
            var t = CoefficientsConfigProvider.Current.ProfileThresholds;

            var active = new List<PlayerProfile>();
            var potential = new List<PlayerProfile>();

            // Двойные пороги: active = все условия > hard, potential = все условия > soft, но не все > hard.
            // Принимает массив троек (value, hard, soft).
            void EvalNum(PlayerProfile profile, params (double value, double hard, double soft)[] conditions)
            {
                if (conditions.All(c => c.value > c.hard))
                {
                    active.Add(profile);
                    return;
                }
                if (conditions.All(c => c.value > c.soft))
                {
                    potential.Add(profile);
                }
            }

            // Вариант со смешанными условиями: булево + числовые с hard/soft порогами.
            void EvalMixed(PlayerProfile profile, bool extraHard, bool extraSoft, params (double value, double hard, double soft)[] numeric)
            {
                if (extraHard && numeric.All(c => c.value > c.hard))
                {
                    active.Add(profile);
                    return;
                }
                if (extraSoft && numeric.All(c => c.value > c.soft))
                {
                    potential.Add(profile);
                }
            }

            // Простые числовые правила (одно условие)
            EvalNum(PlayerProfile.EnduranceRunner, (base_.AerobicLoad, t.EnduranceRunner_AerobicLoad, t.EnduranceRunner_AerobicLoad_Soft));
            EvalNum(PlayerProfile.PowerPlayer,     (m.PlayerLoad,      t.PowerPlayer_PlayerLoad,      t.PowerPlayer_PlayerLoad_Soft));
            EvalNum(PlayerProfile.ExplosivePlayer, (base_.ExplosiveIndex, t.ExplosivePlayer_ExplosiveIndex, t.ExplosivePlayer_ExplosiveIndex_Soft));
            EvalNum(PlayerProfile.DynamicPlayer,   (base_.AccelPerSecond, t.DynamicPlayer_AccelPerSecond, t.DynamicPlayer_AccelPerSecond_Soft));
            EvalNum(PlayerProfile.StaticPlayer,    (SafeDivide(m.PlayerLoad, m.TotalDistance), t.StaticPlayer_PlayerLoadPerDistance, t.StaticPlayer_PlayerLoadPerDistance_Soft));

            // Goalkeeper — по позиции, без soft.
            if (sportsman.Position == Position.GK) active.Add(PlayerProfile.Goalkeeper);

            // Sprinter
            EvalNum(PlayerProfile.Sprinter,
                (base_.SprintRatio,    t.Sprinter_SprintRatio,    t.Sprinter_SprintRatio_Soft),
                (base_.AccelPerSecond, t.Sprinter_AccelPerSecond, t.Sprinter_AccelPerSecond_Soft));

            // FlankPlayer — последнее условие позиционное (SpeedWeight), идёт как extra-bool без soft.
            EvalMixed(PlayerProfile.FlankPlayer,
                extraHard: posCoeff.SpeedWeight >= t.FlankPlayer_SpeedWeightMin,
                extraSoft: posCoeff.SpeedWeight >= t.FlankPlayer_SpeedWeightMin,
                (base_.SprintRatio,        t.FlankPlayer_SprintRatio,        t.FlankPlayer_SprintRatio_Soft),
                (base_.AccelPerSecond,     t.FlankPlayer_AccelPerSecond,     t.FlankPlayer_AccelPerSecond_Soft),
                (base_.DistancePerMinute,  t.FlankPlayer_DistancePerMinute,  t.FlankPlayer_DistancePerMinute_Soft));

            // DefenderType — позиционное условие на PowerWeight.
            double defenderIndex = SafeDivide(m.PlayerLoad, m.TotalDistance) + base_.AccelPerSecond + base_.DecelPerSecond;
            EvalMixed(PlayerProfile.DefenderType,
                extraHard: posCoeff.PowerWeight >= t.DefenderType_PowerWeightMin,
                extraSoft: posCoeff.PowerWeight >= t.DefenderType_PowerWeightMin,
                (defenderIndex, t.DefenderType_Index, t.DefenderType_Index_Soft));

            // CentralMidfielder — позиционное условие на EnduranceWeight.
            double centralMidIndex = base_.DistancePerMinute / 100.0 * 0.5 + base_.HRStability;
            EvalMixed(PlayerProfile.CentralMidfielder,
                extraHard: posCoeff.EnduranceWeight >= t.CentralMidfielder_EnduranceWeightMin,
                extraSoft: posCoeff.EnduranceWeight >= t.CentralMidfielder_EnduranceWeightMin,
                (centralMidIndex, t.CentralMidfielder_Index, t.CentralMidfielder_Index_Soft));

            // DefensiveMidfielder (1 условие)
            double defensiveMidIndex = SafeDivide(m.PlayerLoad, m.Duration / 60.0) / 10.0 + base_.LowIntensityRatio;
            EvalNum(PlayerProfile.DefensiveMidfielder, (defensiveMidIndex, t.DefensiveMidfielder_Index, t.DefensiveMidfielder_Index_Soft));

            // AttackingMidfielder (окно — без soft, окно само мягкое).
            if (base_.SprintRatio > t.AttackingMidfielder_SprintRatioMin && base_.SprintRatio <= t.AttackingMidfielder_SprintRatioMax
                && base_.HighSpeedRatio > base_.SprintRatio)
                active.Add(PlayerProfile.AttackingMidfielder);

            // Forward — позиционное условие на SprintWeight.
            EvalMixed(PlayerProfile.Forward,
                extraHard: posCoeff.SprintWeight >= t.Forward_SprintWeightMin,
                extraSoft: posCoeff.SprintWeight >= t.Forward_SprintWeightMin,
                (base_.SprintRatio, t.Forward_SprintRatio, t.Forward_SprintRatio_Soft));

            // Offensive
            EvalNum(PlayerProfile.Offensive,
                (base_.HighIntensityRatio, t.Offensive_HighIntensityRatio, t.Offensive_HighIntensityRatio_Soft),
                (m.SprintEfforts,          t.Offensive_SprintEfforts,      t.Offensive_SprintEfforts_Soft));

            // Defensive (1 условие)
            EvalNum(PlayerProfile.Defensive, (defenderIndex, t.Defensive_Index, t.Defensive_Index_Soft));

            // Universal — если набралось ≥ N активных профилей
            if (active.Count >= t.Universal_MinActiveCount) active.Add(PlayerProfile.Universal);

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
