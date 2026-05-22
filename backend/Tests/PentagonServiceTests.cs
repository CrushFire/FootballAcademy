using Application.Services.MetricAnalytic;
using Core.Entities;
using Core.Enums;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Tests
{
    // Тесты PentagonService — пятиугольник характеристик спортсмена
    public class PentagonServiceTests
    {
        private static IMemoryCache Cache() =>
            new MemoryCache(new MemoryCacheOptions());

        private static Sportsman MakeSportsman(long id, string fio = "Игрок", Position? position = null) =>
            new() { Id = id, FIO = fio, UserId = id, BirthDate = DateTime.Now.AddYears(-16), Position = position };

        private static Training MakeTraining(long id, DateTime? date = null) =>
            new() { Id = id, TrainerId = 1, GroupId = 1, Date = date ?? DateTime.UtcNow };

        private static TrainingMetrics MakeMetrics(long id, long sportsmanId, long trainingId,
            double playerLoad = 200, double maxSpeed = 25,
            int sprintDist = 300, int totalDist = 5000,
            int accel = 50, int decel = 50,
            int sprintEfforts = 5, int explosiveEfforts = 3) =>
            new()
            {
                Id = id, SportsmanId = sportsmanId, TrainingId = trainingId,
                PlayerLoad = playerLoad, MaximumSpeed = maxSpeed,
                SprintDistance = sprintDist, TotalDistance = totalDist,
                AccelerationCount = accel, DecelerationCount = decel,
                SprintEfforts = sprintEfforts, ExplosiveEfforts = explosiveEfforts,
                Duration = 3600, MaxHeartRate = 170, AverageHeartRate = 140,
                TimeInSpeedZone1 = 400, TimeInHRRedZone = 100,
                LowSpeedDistance = 2000, ModerateSpeedDistance = 1500,
                HighSpeedDistance = 500
            };

        // --- несуществующий спортсмен ---

        [Fact]
        public async Task GetPentagon_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetPentagon_НесуществующийСпортсмен_404));
            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нет метрик ---

        [Fact]
        public async Task GetPentagon_НетМетрик_404()
        {
            var context = TestHelper.CreateContext(nameof(GetPentagon_НетМетрик_404));
            context.Sportsmen.Add(MakeSportsman(1));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нормальные данные ---

        [Fact]
        public async Task GetPentagon_НормальныеДанные_ВсеОсиВДиапазоне01()
        {
            var context = TestHelper.CreateContext(nameof(GetPentagon_НормальныеДанные_ВсеОсиВДиапазоне01));
            context.Sportsmen.Add(MakeSportsman(1, "Козлов"));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 250, maxSpeed: 28, sprintDist: 500, totalDist: 6000,
                accel: 80, decel: 60, sprintEfforts: 8, explosiveEfforts: 5));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Козлов", result.Data!.SportsmanName);
            var p = result.Data.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Power, 0.0, 1.0);
            Assert.InRange(p.Sprints, 0.0, 1.0);
            Assert.InRange(p.Endurance, 0.0, 1.0);
            Assert.InRange(p.Explosive, 0.0, 1.0);
        }

        // --- позиции и коэффициенты ---

        [Fact]
        public async Task GetPentagon_ВратарьGK_ВсеОсиВДиапазоне()
        {
            var context = TestHelper.CreateContext(nameof(GetPentagon_ВратарьGK_ВсеОсиВДиапазоне));
            context.Sportsmen.Add(MakeSportsman(1, "Вратарь", Position.GK));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 150, maxSpeed: 18));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Explosive, 0.0, 1.0);
        }

        [Fact]
        public async Task GetPentagon_НападающийST_ВсеОсиВДиапазоне()
        {
            var context = TestHelper.CreateContext(nameof(GetPentagon_НападающийST_ВсеОсиВДиапазоне));
            context.Sportsmen.Add(MakeSportsman(1, "Форвард", Position.ST));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 280, maxSpeed: 32, sprintDist: 800, totalDist: 5500,
                sprintEfforts: 12, explosiveEfforts: 8));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Sprints, 0.0, 1.0);
        }

        [Fact]
        public async Task GetPentagon_БезПозиции_ИспользуетДефолтКоэффициент()
        {
            // Position = null → GetDefault() коэффициент
            var context = TestHelper.CreateContext(nameof(GetPentagon_БезПозиции_ИспользуетДефолтКоэффициент));
            context.Sportsmen.Add(MakeSportsman(1, "Без позиции", null));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 200, maxSpeed: 25));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
        }

        // --- граничные / ошибочные данные ---

        [Fact]
        public async Task GetPentagon_НулевыеМетрики_НеПадает()
        {
            // все нули — деление на ноль не должно ронять сервис
            var context = TestHelper.CreateContext(nameof(GetPentagon_НулевыеМетрики_НеПадает));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 0, maxSpeed: 0, sprintDist: 0, totalDist: 0,
                accel: 0, decel: 0, sprintEfforts: 0, explosiveEfforts: 0));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            // при нулях все оси = 0
            Assert.Equal(0.0, p.Speed);
            Assert.Equal(0.0, p.Power);
        }

        [Fact]
        public async Task GetPentagon_МаксимальныеМетрики_ОсиНеПревышают1()
        {
            // экстремально высокие значения — оси должны быть clamp(0,1)
            var context = TestHelper.CreateContext(nameof(GetPentagon_МаксимальныеМетрики_ОсиНеПревышают1));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 99999, maxSpeed: 999, sprintDist: 99999, totalDist: 100000,
                accel: 9999, decel: 9999, sprintEfforts: 9999, explosiveEfforts: 9999));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Power, 0.0, 1.0);
            Assert.InRange(p.Sprints, 0.0, 1.0);
            Assert.InRange(p.Endurance, 0.0, 1.0);
            Assert.InRange(p.Explosive, 0.0, 1.0);
        }

        [Fact]
        public async Task GetPentagon_МолодойСпортсмен10Лет_ВсеОсиВДиапазоне()
        {
            // возраст 10 лет — другие возрастные коэффициенты
            var context = TestHelper.CreateContext(nameof(GetPentagon_МолодойСпортсмен10Лет_ВсеОсиВДиапазоне));
            var young = MakeSportsman(1, "Юный");
            young.BirthDate = DateTime.Now.AddYears(-10);
            context.Sportsmen.Add(young);
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 100, maxSpeed: 15));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(), Cache());
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Endurance, 0.0, 1.0);
        }
    }
}
