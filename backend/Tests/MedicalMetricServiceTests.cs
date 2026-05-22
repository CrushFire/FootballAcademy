using Application.Services.MetricAnalytic;
using Core.Entities;
using Xunit;

namespace Tests
{
    // Тесты MedicalMetricService — медицинские показатели и проверка здоровья
    public class MedicalMetricServiceTests
    {
        private static Sportsman MakeSportsman(long id, string fio = "Игрок", int ageYears = 16) =>
            new() { Id = id, FIO = fio, UserId = id, BirthDate = DateTime.Now.AddYears(-ageYears) };

        private static Training MakeTraining(long id, DateTime? date = null) =>
            new() { Id = id, TrainerId = 1, GroupId = 1, Date = date ?? DateTime.UtcNow };

        private static TrainingMetrics MakeMetrics(long id, long sportsmanId, long trainingId,
            double playerLoad = 200, int maxHR = 170, int avgHR = 140,
            int timeZone1 = 400, int accel = 50, int decel = 50, int hrRedZone = 100) =>
            new()
            {
                Id = id, SportsmanId = sportsmanId, TrainingId = trainingId,
                PlayerLoad = playerLoad, MaxHeartRate = maxHR, AverageHeartRate = avgHR,
                TimeInSpeedZone1 = timeZone1, AccelerationCount = accel, DecelerationCount = decel,
                TimeInHRRedZone = hrRedZone, Duration = 3600, TotalDistance = 5000,
                MaximumSpeed = 25, SprintDistance = 300, SprintEfforts = 5
            };

        // --- несуществующий спортсмен ---

        [Fact]
        public async Task GetMedicalMetrics_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalMetrics_НесуществующийСпортсмен_404));
            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalMetricsAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetMedicalCheck_НесуществующийСпортсмен_404()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_НесуществующийСпортсмен_404));
            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нет метрик ---

        [Fact]
        public async Task GetMedicalMetrics_НетМетрик_404()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalMetrics_НетМетрик_404));
            context.Sportsmen.Add(MakeSportsman(1));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalMetricsAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetMedicalCheck_НетМетрик_404()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_НетМетрик_404));
            context.Sportsmen.Add(MakeSportsman(1));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нормальные показатели ---

        [Fact]
        public async Task GetMedicalCheck_НормальнаяНагрузка_IsHealthyTrue()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_НормальнаяНагрузка_IsHealthyTrue));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 200, maxHR: 170, avgHR: 130,
                timeZone1: 500, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data!.CheckResult.IsHealthy);
            Assert.Empty(result.Data.CheckResult.Issues);
        }

        // --- критические / ошибочные данные ---

        [Fact]
        public async Task GetMedicalCheck_КритическийПульс_CardiovascularFalse()
        {
            // MaxHR = 250 — явно за пределами нормы для любого возраста
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_КритическийПульс_CardiovascularFalse));
            context.Sportsmen.Add(MakeSportsman(1, ageYears: 16));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 200, maxHR: 250, avgHR: 200,
                timeZone1: 500, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.CardiovascularOk);
            Assert.Contains(result.Data.CheckResult.Issues, i => i.Contains("пульс"));
        }

        [Fact]
        public async Task GetMedicalCheck_КритическаяНагрузка_LoadFalse()
        {
            // PlayerLoad = 700 — критически высокая
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_КритическаяНагрузка_LoadFalse));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 700, maxHR: 160, avgHR: 130,
                timeZone1: 500, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.LoadOk);
        }

        [Fact]
        public async Task GetMedicalCheck_НедостаточноеВосстановление_RecoveryFalse()
        {
            // TimeInSpeedZone1 = 50 — почти нет времени восстановления (порог 300)
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_НедостаточноеВосстановление_RecoveryFalse));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 200, maxHR: 160, avgHR: 130,
                timeZone1: 50, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.RecoveryOk);
            Assert.Contains(result.Data.CheckResult.Issues, i => i.Contains("восстановления"));
        }

        [Fact]
        public async Task GetMedicalCheck_ВысокийРискТравм_InjuryRiskFalse()
        {
            // accel + decel = 250 > 200 → высокий риск травм
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_ВысокийРискТравм_InjuryRiskFalse));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 200, maxHR: 160, avgHR: 130,
                timeZone1: 500, accel: 130, decel: 130, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.InjuryRiskOk);
        }

        [Fact]
        public async Task GetMedicalCheck_ПограничнаяНагрузка499_LoadOkTrue()
        {
            // PlayerLoad = 499 — ровно под порогом 500
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_ПограничнаяНагрузка499_LoadOkTrue));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 499, maxHR: 160, avgHR: 130,
                timeZone1: 500, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data!.CheckResult.LoadOk);
        }

        [Fact]
        public async Task GetMedicalCheck_ПограничнаяНагрузка500_LoadOkFalse()
        {
            // PlayerLoad = 500 — ровно на пороге → LoadOk = false
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_ПограничнаяНагрузка500_LoadOkFalse));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 500, maxHR: 160, avgHR: 130,
                timeZone1: 500, accel: 40, decel: 40, hrRedZone: 100));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.LoadOk);
        }

        [Fact]
        public async Task GetMedicalCheck_ВсеПоказателиКритические_МногоIssues()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalCheck_ВсеПоказателиКритические_МногоIssues));
            context.Sportsmen.Add(MakeSportsman(1));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1,
                playerLoad: 800, maxHR: 240, avgHR: 200,
                timeZone1: 50, accel: 200, decel: 200, hrRedZone: 900));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.CheckResult.IsHealthy);
            Assert.True(result.Data.CheckResult.Issues.Count >= 4);
        }

        [Fact]
        public async Task GetAllMedicalCheck_ПустаяБД_ВозвращаетПустой()
        {
            var context = TestHelper.CreateContext(nameof(GetAllMedicalCheck_ПустаяБД_ВозвращаетПустой));
            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetAllSportsmenMedicalCheckAsync();
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetMedicalMetrics_НормальныеДанные_ВозвращаетМетрики()
        {
            var context = TestHelper.CreateContext(nameof(GetMedicalMetrics_НормальныеДанные_ВозвращаетМетрики));
            context.Sportsmen.Add(MakeSportsman(1, "Козлов"));
            context.Trainings.Add(MakeTraining(1));
            context.TrainingMetrics.Add(MakeMetrics(1, 1, 1, playerLoad: 250, maxHR: 165, avgHR: 135));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalMetricsAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Козлов", result.Data!.SportsmanName);
            Assert.Equal(165, result.Data.Metrics.MaxHeartRate);
        }
    }
}
