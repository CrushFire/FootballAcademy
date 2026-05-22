using Application.Services.MetricAnalytic;
using Core.Entities;
using Xunit;

namespace Tests
{
    // Тесты GraphService — временные точки метрик для графиков
    public class GraphServiceTests
    {
        private static Sportsman MakeSportsman(long id) =>
            new() { Id = id, FIO = "Игрок", UserId = id, BirthDate = DateTime.Now.AddYears(-16) };

        private static Training MakeTraining(long id, DateTime date) =>
            new() { Id = id, TrainerId = 1, GroupId = 1, Date = date };

        private static TrainingMetrics MakeMetrics(long id, long sportsmanId, long trainingId,
            double playerLoad = 200, double maxSpeed = 25,
            int sprintDist = 300, int totalDist = 5000) =>
            new()
            {
                Id = id, SportsmanId = sportsmanId, TrainingId = trainingId,
                PlayerLoad = playerLoad, MaximumSpeed = maxSpeed,
                SprintDistance = sprintDist, TotalDistance = totalDist,
                Duration = 3600, MaxHeartRate = 170, AverageHeartRate = 140,
                TimeInSpeedZone1 = 400, TimeInHRRedZone = 100,
                AccelerationCount = 50, DecelerationCount = 50, SprintEfforts = 5
            };

        // --- нет данных ---

        [Fact]
        public async Task GetGraph_НесуществующийСпортсмен_ПустойСписок()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_НесуществующийСпортсмен_ПустойСписок));
            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(999, null);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGraph_СпортсменЕстьНоНетМетрик_ПустойСписок()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_СпортсменЕстьНоНетМетрик_ПустойСписок));
            context.Sportsmen.Add(MakeSportsman(1));
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);
            Assert.Empty(result);
        }

        // --- одна тренировка ---

        [Fact]
        public async Task GetGraph_ОднаТренировка_ВозвращаетОднуТочку()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_ОднаТренировка_ВозвращаетОднуТочку));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1, DateTime.UtcNow));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 200, maxSpeed: 28));
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);

            Assert.Single(result);
            Assert.True(result[0].Metrics.MaxSpeed > 0);
        }

        // --- несколько тренировок ---

        [Fact]
        public async Task GetGraph_НесколькоТренировок_ОтсортированыПоДате()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_НесколькоТренировок_ОтсортированыПоДате));
            context.Sportsmen.Add(MakeSportsman(1));
            var base_ = DateTime.UtcNow;
            context.Trainings.Add(MakeTraining(1, base_.AddDays(-5)));
            context.Trainings.Add(MakeTraining(2, base_.AddDays(-3)));
            context.Trainings.Add(MakeTraining(3, base_.AddDays(-1)));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 100));
            context.TrainingMetrics.Add(MakeMetrics(2, 1, 2, playerLoad: 200));
            context.TrainingMetrics.Add(MakeMetrics(3, 1, 3, playerLoad: 300));
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);

            Assert.Equal(3, result.Count);
            Assert.True(result[0].Date <= result[1].Date);
            Assert.True(result[1].Date <= result[2].Date);
        }

        [Fact]
        public async Task GetGraph_ДваСпортсмена_ВозвращаетТолькоСвоего()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_ДваСпортсмена_ВозвращаетТолькоСвоего));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Sportsmen.Add(MakeSportsman(2));
            var base_ = DateTime.UtcNow;
            context.Trainings.Add(MakeTraining(1, base_.AddDays(-2)));
            context.Trainings.Add(MakeTraining(2, base_.AddDays(-1)));
            // спортсмен 1 — 2 тренировки, спортсмен 2 — 1 тренировка
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1));
            context.TrainingMetrics.Add(MakeMetrics(2, 1, 2));
            context.TrainingMetrics.Add(MakeMetrics(3, 2, 1));
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetGraph_НулевыеМетрики_НеПадает()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_НулевыеМетрики_НеПадает));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1, DateTime.UtcNow));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 0, maxSpeed: 0, sprintDist: 0, totalDist: 0));
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);

            Assert.Single(result);
            Assert.Equal(0, result[0].Metrics.MaxSpeed);
        }

        [Fact]
        public async Task GetGraph_10Тренировок_Возвращает10Точек()
        {
            var context = TestHelper.CreateContext(nameof(GetGraph_10Тренировок_Возвращает10Точек));
            context.Sportsmen.Add(MakeSportsman(1));
            for (int i = 1; i <= 10; i++)
            {
                context.Trainings.Add(MakeTraining(i, DateTime.UtcNow.AddDays(-i)));
                context.TrainingMetrics.Add(MakeMetrics(i, 1, i, playerLoad: i * 30));
            }
            await context.SaveChangesAsync();

            var service = new GraphService(context);
            var result = await service.GetGraphMetricsAsync(1, null);

            Assert.Equal(10, result.Count);
        }
    }
}
