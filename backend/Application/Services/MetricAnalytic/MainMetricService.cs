using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel;
using Core.Models.MetricModel.Calculated;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.MetricAnalytic
{
    public class MainMetricService : IMainMetricService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MainMetricService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PlayerCalculatedMetricsResponse>> GetSportsmanAverageMetricsAsync(long sportsmanId, Filter? filter = null)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<PlayerCalculatedMetricsResponse>.Failure("Спортсмен не найден", 404);

            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter);

            var metrics = await query.ToListAsync();

            if (!metrics.Any())
                return Result<PlayerCalculatedMetricsResponse>.Failure("У данного игрока пока нет данных о метриках", 404);

            var avgMetric = _mapper.Map<TrainingMetrics>(metrics);
            var playerLoads = metrics.Select(m => m.PlayerLoad).ToList();

            return Result<PlayerCalculatedMetricsResponse>.Success(new PlayerCalculatedMetricsResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                Metrics = BaseMetricFactory.CreateMetrics(avgMetric, playerLoads)
            });
        }

        public async Task<Result<PlayerCalculatedMetricsResponse>> GetSportsmanTrainingMetricsAsync(long sportsmanId, long trainingId)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<PlayerCalculatedMetricsResponse>.Failure("Спортсмен не найден", 404);

            var metric = await _context.TrainingMetrics
                .Include(m => m.Training)
                .FirstOrDefaultAsync(m => m.SportsmanId == sportsmanId && m.TrainingId == trainingId);

            if (metric == null)
                return Result<PlayerCalculatedMetricsResponse>.Failure("Метрики для данной тренировки не найдены", 404);

            var recentLoads = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId && m.Training!.Date <= metric.Training!.Date)
                .OrderByDescending(m => m.Training!.Date)
                .Take(5)
                .Select(m => m.PlayerLoad)
                .ToListAsync();

            return Result<PlayerCalculatedMetricsResponse>.Success(new PlayerCalculatedMetricsResponse
            {
                SportsmanId = sportsmanId,
                SportsmanName = sportsman.FIO,
                Metrics = BaseMetricFactory.CreateMetrics(metric, recentLoads)
            });
        }
    }
}
