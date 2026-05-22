using Application.Services.MetricAnalytic;
using Core.Entities;
using Xunit;

namespace Tests
{
    // Тесты MainMetricService — средние метрики и метрики по конкретной тренировке
    public class MainMetricServiceTests
    {
        private static Sportsman MakeSportsman(long id, string fio = "Игрок") =>
            new() { Id = id, FIO = fio, UserId = id, BirthDate = DateTime.Now.AddYears(-16) };

        private static Training MakeTraining(long id, DateTime? date = null) =>
            new() { Id = id, TrainerId = 1, GroupId = 1, Date = date ?? DateTime.UtcNow };

        private static TrainingMetrics MakeMetrics(long id, long sportsmanId, long trainingId,
            double playerLoad = 200, double maxSpeed = 25, int sprintDist = 300,
            int totalDist = 5000, int accel = 50, int decel = 50, int duration = 3600) =>
            new()
            {
                Id = id, SportsmanId = sportsmanId, TrainingId = trainingId,
                PlayerLoad = playerLoad, MaximumSpeed = maxSpeed,
                SprintDistance = sprintDist, TotalDistance = totalDist,
                AccelerationCount = accel, DecelerationCount = decel,
                Duration = duration, MaxHeartRate = 170, AverageHeartRate = 140,
                TimeInSpeedZone1 = 400, TimeInHRRedZone = 100, SprintEfforts = 5
            };

        // --- несуществующий спортсмен ---

        [Fact]
        public async Task GetAverage_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetAverage_НесуществующийСпортсмен_404));
            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(999, null);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetTraining_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetTraining_НесуществующийСпортсмен_404));
            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanTrainingMetricsAsync(999, 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нет метрик ---

        [Fact]
        public async Task GetAverage_НетМетрик_404()
        {
            var context = TestHelper.CreateContext(nameof(GetAverage_НетМетрик_404));
            context.Sportsmen.Add(MakeSportsman(1));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetTraining_МетрикиЕстьНоНеТаТренировка_404()
        {
            var context = TestHelper.CreateContext(nameof(GetTraining_МетрикиЕстьНоНеТаТренировка_404));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            // тренировка 999 не существует
            var result = await service.GetSportsmanTrainingMetricsAsync(1, 999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нормальные данные ---

        [Fact]
        public async Task GetAverage_ОднаТренировка_ВозвращаетМетрики()
        {
            var context = TestHelper.CreateContext(nameof(GetAverage_ОднаТренировка_ВозвращаетМетрики));
            context.Sportsmen.Add(MakeSportsman(1, "Иванов"));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 250, maxSpeed: 28));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.Equal("Иванов", result.Data!.SportsmanName);
            Assert.Equal(1, result.Data.SportsmanId);
            Assert.True(result.Data.Metrics.MaxSpeed > 0);
        }

        [Fact]
        public async Task GetTraining_КонкретнаяТренировка_ВозвращаетМетрики()
        {
            var context = TestHelper.CreateContext(nameof(GetTraining_КонкретнаяТренировка_ВозвращаетМетрики));
            context.Sportsmen.Add(MakeSportsman(1, "Петров"));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 300, maxSpeed: 32));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanTrainingMetricsAsync(1, 1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Петров", result.Data!.SportsmanName);
            Assert.True(result.Data.Metrics.MaxSpeed > 0);
        }

        // --- нестандартные / граничные данные ---

        [Fact]
        public async Task GetAverage_НулевыеМетрики_НеПадает()
        {
            // все поля = 0 — деление на ноль не должно ронять сервис
            var context = TestHelper.CreateContext(nameof(GetAverage_НулевыеМетрики_НеПадает));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 0, maxSpeed: 0, sprintDist: 0, totalDist: 0,
                accel: 0, decel: 0, duration: 0));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Data!.Metrics.MaxSpeed);
        }

        [Fact]
        public async Task GetAverage_МаксимальныеМетрики_НеПадает()
        {
            // экстремально высокие значения
            var context = TestHelper.CreateContext(nameof(GetAverage_МаксимальныеМетрики_НеПадает));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 9999, maxSpeed: 99, sprintDist: 50000,
                totalDist: 100000, accel: 9999, decel: 9999, duration: 86400));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data!.Metrics.MaxSpeed > 0);
        }

        [Fact]
        public async Task GetAverage_МногоТренировок_АгрегируетКорректно()
        {
            var context = TestHelper.CreateContext(nameof(GetAverage_МногоТренировок_АгрегируетКорректно));
            context.Sportsmen.Add(MakeSportsman(1));
            for (int i = 1; i <= 10; i++)
            {
                context.Trainings.Add(MakeTraining(i, DateTime.UtcNow.AddDays(-i)));
                context.TrainingMetrics.Add(MakeMetrics(i, 1, i, playerLoad: i * 50, maxSpeed: 20 + i));
            }
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data!.Metrics.MaxSpeed > 0);
        }

        [Fact]
        public async Task GetTraining_ДваСпортсменаОднаТренировка_ВозвращаетТолькоСвоего()
        {
            var context = TestHelper.CreateContext(nameof(GetTraining_ДваСпортсменаОднаТренировка_ВозвращаетТолькоСвоего));
            context.Sportsmen.Add(MakeSportsman(1, "Первый"));
            context.Sportsmen.Add(MakeSportsman(2, "Второй"));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, maxSpeed: 30));
            context.TrainingMetrics.Add(MakeMetrics(2, 2, 1, maxSpeed: 20));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanTrainingMetricsAsync(1, 1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Первый", result.Data!.SportsmanName);
        }
    }
}
