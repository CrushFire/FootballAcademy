using Application.Services.Coefficientes;
using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel.Medical;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.MetricAnalytic
{
    public class MedicalMetricService : IMedicalMetricService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MedicalMetricService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PlayerMedicalMetricsResponse>> GetSportsmanMedicalMetricsAsync(long sportsmanId, Filter? filter = null)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<PlayerMedicalMetricsResponse>.Failure("Спортсмен не найден", 404);

            var allLoads = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .OrderBy(m => m.Training.Date)
                .Select(m => m.PlayerLoad)
                .ToListAsync();

            var metrics = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter)
                .ToListAsync();

            if (!metrics.Any())
                return Result<PlayerMedicalMetricsResponse>.Failure("У этого игрока пока нет метрик", 404);

            var avgMetric = _mapper.Map<TrainingMetrics>(metrics);
            double chronicLoad = metrics.Average(m => m.PlayerLoad);
            var last5 = allLoads.TakeLast(5).ToList();

            return Result<PlayerMedicalMetricsResponse>.Success(new PlayerMedicalMetricsResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                Metrics = BaseMetricFactory.CreateMedicalMetrics(avgMetric, chronicLoad, last5)
            });
        }

        public async Task<Result<PlayerMedicalCheckResponse>> GetSportsmanMedicalCheckAsync(long sportsmanId, Filter? filter = null)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<PlayerMedicalCheckResponse>.Failure("Спортсмен не найден", 404);

            var allLoads = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .OrderBy(m => m.Training.Date)
                .Select(m => m.PlayerLoad)
                .ToListAsync();

            var metrics = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter)
                .ToListAsync();

            if (!metrics.Any())
                return Result<PlayerMedicalCheckResponse>.Failure("У этого игрока пока что нет метрик", 404);

            var avgMetric = _mapper.Map<TrainingMetrics>(metrics);
            double chronicLoad = metrics.Average(m => m.PlayerLoad);
            var last5 = allLoads.TakeLast(5).ToList();

            return Result<PlayerMedicalCheckResponse>.Success(new PlayerMedicalCheckResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                CheckResult = PerformMedicalCheck(avgMetric, chronicLoad, sportsman.Age, sportsman.Gender, last5)
            });
        }

        public async Task<Result<List<PlayerMedicalCheckResponse>>> GetAllSportsmenMedicalCheckAsync(Filter? filter = null)
        {
            var sportsmen = await _context.Sportsmen.ToListAsync();
            var results = new List<PlayerMedicalCheckResponse>();

            foreach (var sportsman in sportsmen)
            {
                var allLoads = await _context.TrainingMetrics
                    .Include(m => m.Training)
                    .Where(m => m.SportsmanId == sportsman.Id)
                    .OrderBy(m => m.Training.Date)
                    .Select(m => m.PlayerLoad)
                    .ToListAsync();

                var metrics = await _context.TrainingMetrics
                    .Include(m => m.Training)
                    .Where(m => m.SportsmanId == sportsman.Id)
                    .ApplyFilter(filter)
                    .ToListAsync();

                // Без метрик невозможно посчитать чек — пропускаем спортсмена,
                // иначе AutoMapper-агрегатор крашит на .First() пустого списка.
                if (!metrics.Any()) continue;

                var avgMetric = _mapper.Map<TrainingMetrics>(metrics);
                double chronicLoad = metrics.Average(m => m.PlayerLoad);
                var last5 = allLoads.TakeLast(5).ToList();

                var checkResult = PerformMedicalCheck(avgMetric, chronicLoad, sportsman.Age, sportsman.Gender, last5);

                if (!checkResult.IsHealthy)
                {
                    results.Add(new PlayerMedicalCheckResponse
                    {
                        SportsmanId = sportsman.Id,
                        SportsmanName = sportsman.FIO,
                        CheckResult = checkResult
                    });
                }
            }

            return Result<List<PlayerMedicalCheckResponse>>.Success(results);
        }

        private PlayerMedicalCheckResult PerformMedicalCheck(TrainingMetrics m, double chronicLoad, int age, char gender, List<double> last5)
        {
            var coeff = AgeCoefficientService.GetByAgeAndGender(age, gender);
            var t = CoefficientsConfigProvider.Current.MedicalThresholds;
            int maxHRThreshold = coeff.MaxHeartRateBase - age;
            double acuteChronicRatio = chronicLoad > 0 ? m.PlayerLoad / chronicLoad : 0;
            double mean5 = last5.Count >= 3 ? last5.Average() : 0;
            double fatigueIndex = mean5 > 0 ? m.PlayerLoad / mean5 : 0;
            double consistency = last5.Count >= 3
                ? Math.Sqrt(last5.Average(x => Math.Pow(x - mean5, 2)))
                : 0;

            // Возрастные пороги (пол уже учтён в Factor через GetByAgeAndGender)
            double loadCap        = t.LoadCapBase       * coeff.PowerFactor;
            double injuryLoadCap  = t.InjuryLoadCapBase * coeff.PowerFactor;
            int    accDecCap      = (int)(t.AccDecCapBase   * coeff.ExplosiveFactor);
            int    redZoneCap     = (int)(t.RedZoneCapBase  * coeff.EnduranceFactor);
            double consistencyCap = t.ConsistencyCapBase * coeff.PowerFactor;

            var result = new PlayerMedicalCheckResult
            {
                CardiovascularOk = m.MaxHeartRate < maxHRThreshold && m.AverageHeartRate < t.AverageHeartRateMax,
                LoadOk = m.PlayerLoad < loadCap && acuteChronicRatio < t.AcuteChronicDanger,
                RecoveryOk = m.TimeInSpeedZone1 > t.MinRecoveryTimeSeconds,
                InjuryRiskOk = m.PlayerLoad < injuryLoadCap && (m.AccelerationCount + m.DecelerationCount) < accDecCap && acuteChronicRatio < t.AcuteChronicWarn,
                FatigueOk = m.TimeInHRRedZone < redZoneCap && (fatigueIndex == 0 || fatigueIndex < t.FatigueIndexMax),
                ConsistencyOk = consistency == 0 || consistency <= consistencyCap
            };

            if (m.MaxHeartRate >= maxHRThreshold)
                result.Issues.Add($"Максимальный пульс слишком высокий: {m.MaxHeartRate} уд/мин (порог: {maxHRThreshold})");
            if (m.AverageHeartRate >= t.AverageHeartRateMax)
                result.Issues.Add($"Средний пульс слишком высокий: {m.AverageHeartRate} уд/мин (порог: {t.AverageHeartRateMax})");
            if (m.PlayerLoad >= loadCap)
                result.Issues.Add($"Нагрузка игрока критически высокая: {m.PlayerLoad:F1} (порог: {loadCap:F0})");
            if (acuteChronicRatio >= t.AcuteChronicDanger)
                result.Issues.Add($"Острая нагрузка превышает хроническую в {acuteChronicRatio:F2} раз (порог: {t.AcuteChronicDanger})");
            if (m.TimeInSpeedZone1 <= t.MinRecoveryTimeSeconds)
                result.Issues.Add($"Недостаточное время восстановления: {m.TimeInSpeedZone1}с (минимум: {t.MinRecoveryTimeSeconds}с)");
            if (m.PlayerLoad >= injuryLoadCap)
                result.Issues.Add($"Высокий риск травм - нагрузка: {m.PlayerLoad:F1} (порог: {injuryLoadCap:F0})");
            if (m.AccelerationCount + m.DecelerationCount >= accDecCap)
                result.Issues.Add($"Высокий риск травм - ускорения/замедления: {m.AccelerationCount + m.DecelerationCount} (порог: {accDecCap})");
            if (acuteChronicRatio >= t.AcuteChronicWarn && acuteChronicRatio < t.AcuteChronicDanger)
                result.Issues.Add($"Повышенный риск травм - острая/хроническая нагрузка: {acuteChronicRatio:F2} (порог: {t.AcuteChronicWarn})");
            if (m.TimeInHRRedZone >= redZoneCap)
            {
                var rzPercent = m.Duration > 0 ? m.TimeInHRRedZone * 100.0 / m.Duration : 0;
                var rzPercentCap = m.Duration > 0 ? redZoneCap * 100.0 / m.Duration : 0;
                result.Issues.Add($"Избыточное время в красной зоне ЧСС: {rzPercent:F1}% от тренировки (порог: {rzPercentCap:F1}%)");
            }
            if (fatigueIndex >= t.FatigueIndexMax)
                result.Issues.Add($"Перегруз: индекс усталости {fatigueIndex:F2} (норма < {t.FatigueIndexMax})");
            if (consistency > consistencyCap)
                result.Issues.Add($"Нестабильная нагрузка: разброс нагрузок {consistency:F1} у.е. (порог: {consistencyCap:F1})");

            result.IsHealthy = result.CardiovascularOk && result.LoadOk && result.RecoveryOk && result.InjuryRiskOk && result.FatigueOk && result.ConsistencyOk;
            return result;
        }
    }
}
