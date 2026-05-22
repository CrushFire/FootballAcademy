using Application.Utils;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel;
using Core.Models.MetricModel.Graph;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.MetricAnalytic
{
    public class GraphService : IGraphService
    {
        private readonly ApplicationDbContext _context;

        public GraphService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerGraphPoint>> GetGraphMetricsAsync(long sportsmanId, Filter? filter)
        {
            var query = _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .ApplyFilter(filter);

            var metrics = await query.OrderBy(m => m.Training.Date).ToListAsync();

            var allLoads = await _context.TrainingMetrics
                .Include(m => m.Training)
                .Where(m => m.SportsmanId == sportsmanId)
                .OrderBy(m => m.Training!.Date)
                .Select(m => new { m.TrainingId, m.PlayerLoad })
                .ToListAsync();

            // Загружаем информацию о матчах для тренировок: связь теперь Training.MatchId → Match
            var trainingIds = metrics.Select(m => m.TrainingId).ToList();
            var trainingMatchLinks = await _context.Trainings
                .Where(t => trainingIds.Contains(t.Id) && t.MatchId.HasValue)
                .Include(t => t.Match)
                .Select(t => new { TrainingId = t.Id, MatchType = t.Match!.Type })
                .ToListAsync();
            var matchDict = trainingMatchLinks.ToDictionary(x => x.TrainingId, x => x.MatchType.ToString());

            return metrics
                .Select((m, i) => new PlayerGraphPoint
                {
                    Date = m.Training!.Date,
                    Type = m.Training.Type,
                    MatchType = matchDict.ContainsKey(m.TrainingId) ? matchDict[m.TrainingId] : null,
                    Metrics = BaseMetricFactory.CreateMetrics(m, allLoads
                        .TakeWhile(x => x.TrainingId != m.TrainingId)
                        .Append(allLoads.First(x => x.TrainingId == m.TrainingId))
                        .TakeLast(5)
                        .Select(x => x.PlayerLoad)
                        .ToList())
                })
                .ToList();
        }
    }
}
