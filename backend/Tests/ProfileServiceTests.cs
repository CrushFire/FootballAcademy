using Application.Services.MetricAnalytic;
using Core.Entities;
using Core.Models.MetricModel.Profile;
using Xunit;

namespace Tests
{
    // Тесты ProfileService — определение игрового профиля спортсмена
    public class ProfileServiceTests
    {
        private static Sportsman MakeSportsman(long id, string fio = "Игрок") =>
            new() { Id = id, FIO = fio, UserId = id, BirthDate = DateTime.Now.AddYears(-16) };

        private static Training MakeTraining(long id, DateTime? date = null) =>
            new() { Id = id, TrainerId = 1, GroupId = 1, Date = date ?? DateTime.UtcNow };

        private static TrainingMetrics MakeMetrics(long id, long sportsmanId, long trainingId,
            double playerLoad = 200, double maxSpeed = 25,
            int sprintDist = 300, int totalDist = 5000,
            int accel = 50, int decel = 50,
            int sprintEfforts = 5, int explosiveEfforts = 3,
            int highSpeedDist = 500, int lowSpeedDist = 2000, int moderateDist = 1500) =>
            new()
            {
                Id = id, SportsmanId = sportsmanId, TrainingId = trainingId,
                PlayerLoad = playerLoad, MaximumSpeed = maxSpeed,
                SprintDistance = sprintDist, TotalDistance = totalDist,
                AccelerationCount = accel, DecelerationCount = decel,
                SprintEfforts = sprintEfforts, ExplosiveEfforts = explosiveEfforts,
                HighSpeedDistance = highSpeedDist,
                LowSpeedDistance = lowSpeedDist, ModerateSpeedDistance = moderateDist,
                Duration = 3600, MaxHeartRate = 170, AverageHeartRate = 140,
                TimeInSpeedZone1 = 400, TimeInHRRedZone = 100
            };

        // --- несуществующий спортсмен ---

        [Fact]
        public async Task GetProfile_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetProfile_НесуществующийСпортсмен_404));
            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нет метрик ---

        [Fact]
        public async Task GetProfile_НетМетрик_ПустойПрофиль()
        {
            var context = TestHelper.CreateContext(nameof(GetProfile_НетМетрик_ПустойПрофиль));
            context.Sportsmen.Add(MakeSportsman(1, "Сидоров"));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Сидоров", result.Data!.SportsmanName);
            Assert.Null(result.Data.Profiles);
        }

        // --- определение профилей ---

        [Fact]
        public async Task GetProfile_ВысокийСпринт_ОпределяетСпринтера()
        {
            // SprintRatio = 3000/5000 = 0.6 > 0.45, AccelPerSecond = 600/3600 = 0.167 > 0.15
            var context = TestHelper.CreateContext(nameof(GetProfile_ВысокийСпринт_ОпределяетСпринтера));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                sprintDist: 3000, totalDist: 5000, accel: 600, decel: 100, sprintEfforts: 15));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Profiles);
            Assert.Contains(PlayerProfile.Sprinter, result.Data.Profiles!.Profiles!);
        }

        [Fact]
        public async Task GetProfile_ВысокаяВыносливость_ОпределяетБегунаНаВыносливость()
        {
            // AerobicLoad = (lowSpeed + moderate) / total = (3000+1500)/5000 = 0.9 > 0.35
            var context = TestHelper.CreateContext(nameof(GetProfile_ВысокаяВыносливость_ОпределяетБегунаНаВыносливость));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                sprintDist: 100, totalDist: 5000,
                lowSpeedDist: 3000, moderateDist: 1500,
                accel: 20, decel: 20));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Profiles);
            Assert.Contains(PlayerProfile.EnduranceRunner, result.Data.Profiles!.Profiles!);
        }

        [Fact]
        public async Task GetProfile_ВысокаяНагрузка_ОпределяетСиловика()
        {
            // PlayerLoad > 300 → PowerPlayer
            var context = TestHelper.CreateContext(nameof(GetProfile_ВысокаяНагрузка_ОпределяетСиловика));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 400));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Profiles);
            Assert.Contains(PlayerProfile.PowerPlayer, result.Data.Profiles!.Profiles!);
        }

        [Fact]
        public async Task GetProfile_НулевыеДанные_НеПадает()
        {
            // все нули — ни один профиль не должен определиться, но сервис не должен падать
            var context = TestHelper.CreateContext(nameof(GetProfile_НулевыеДанные_НеПадает));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 0, maxSpeed: 0, sprintDist: 0, totalDist: 0,
                accel: 0, decel: 0, sprintEfforts: 0, explosiveEfforts: 0,
                highSpeedDist: 0, lowSpeedDist: 0, moderateDist: 0));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetProfile_ВзрывнойИндекс_ОпределяетВзрывника()
        {
            // ExplosiveIndex = (explosiveEfforts + accel + decel) / duration > 1.2
            // 500 + 500 + 500 = 1500, duration = 1000 → 1.5 > 1.2
            var context = TestHelper.CreateContext(nameof(GetProfile_ВзрывнойИндекс_ОпределяетВзрывника));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(new Core.Entities.TrainingMetrics
            {
                Id = 1, SportsmanId = 1, TrainingId = 1,
                PlayerLoad = 200, MaximumSpeed = 25,
                SprintDistance = 100, TotalDistance = 5000,
                AccelerationCount = 500, DecelerationCount = 500,
                SprintEfforts = 5, ExplosiveEfforts = 500,
                HighSpeedDistance = 500, LowSpeedDistance = 2000, ModerateSpeedDistance = 1500,
                Duration = 1000, // маленький duration → высокий индекс
                MaxHeartRate = 170, AverageHeartRate = 140,
                TimeInSpeedZone1 = 400, TimeInHRRedZone = 100
            });
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Profiles);
            Assert.Contains(PlayerProfile.ExplosivePlayer, result.Data.Profiles!.Profiles!);
        }

        [Fact]
        public async Task GetProfile_МногоПрофилей_ОпределяетУниверсала()
        {
            // 4+ профиля → Universal
            var context = TestHelper.CreateContext(nameof(GetProfile_МногоПрофилей_ОпределяетУниверсала));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 400,           // PowerPlayer
                sprintDist: 3000, totalDist: 5000,  // Sprinter (SprintRatio > 0.45)
                accel: 600, decel: 100,    // Sprinter (AccelPerSecond > 0.15)
                explosiveEfforts: 500,     // ExplosivePlayer
                lowSpeedDist: 2000, moderateDist: 1500,  // EnduranceRunner
                sprintEfforts: 15));
            await context.SaveChangesAsync();

            var service = new ProfileService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanProfileAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Profiles);
            Assert.Contains(PlayerProfile.Universal, result.Data.Profiles!.Profiles!);
        }
    }
}
