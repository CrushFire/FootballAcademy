using Core.Enums.Match;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    /// <summary>
    /// Фоновый сервис очистки брошенных матчей. Запускается каждые 3 часа.
    ///
    /// Логика (матчи со статусом InProgress):
    ///   > 150 минут с начала И нет событий И нет результата → удаляем (пустой матч, создан случайно)
    ///   > 150 минут с начала И есть события ИЛИ есть результат → завершаем (матч вёлся, но не закрыт)
    /// </summary>
    public class MatchCleanupService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public MatchCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            // Первый запуск через 10 минут после старта сервера, затем каждые 3 часа
            _timer = new Timer(async _ => await CleanupAsync(), null,
                TimeSpan.FromMinutes(10),
                TimeSpan.FromHours(3));
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private async Task CleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var threshold = now.AddMinutes(-150);

            var stale = await context.Matches
                .Include(m => m.Events)
                .Where(m => m.Status == MatchStatus.InProgress && m.Date < threshold)
                .ToListAsync();

            if (!stale.Any()) return;

            var toDelete = new List<Core.Entities.Match>();
            var toFinish = new List<Core.Entities.Match>();

            foreach (var match in stale)
            {
                bool hasActivity = match.Events.Any() || match.Result.HasValue;
                if (hasActivity)
                    toFinish.Add(match);
                else
                    toDelete.Add(match);
            }

            if (toFinish.Any())
            {
                foreach (var m in toFinish)
                    m.Status = MatchStatus.Finished;
                context.Matches.UpdateRange(toFinish);
            }

            // При удалении пустых матчей чистим все автосозданные Training (Type="Матч")
            // с MatchId == match.Id вместе с метриками и посещаемостью.
            List<long> autoTrainingIds = new();
            if (toDelete.Any())
            {
                var matchIds = toDelete.Select(m => m.Id).ToList();
                autoTrainingIds = await context.Trainings
                    .Where(t => t.MatchId.HasValue && matchIds.Contains(t.MatchId.Value) && t.Type == "Матч")
                    .Select(t => t.Id)
                    .ToListAsync();

                if (autoTrainingIds.Any())
                {
                    context.TrainingMetrics.RemoveRange(
                        context.TrainingMetrics.Where(m => autoTrainingIds.Contains(m.TrainingId)));
                    context.Attendances.RemoveRange(
                        context.Attendances.Where(a => autoTrainingIds.Contains(a.TrainingId)));
                    context.Trainings.RemoveRange(
                        context.Trainings.Where(t => autoTrainingIds.Contains(t.Id)));
                }

                context.Matches.RemoveRange(toDelete);
            }

            if (toFinish.Any() || toDelete.Any())
                await context.SaveChangesAsync();
        }
    }
}
