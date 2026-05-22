using Application.Services;
using Core.Entities;
using Core.Enums;
using Core.Models.AttendanceModel;
using Xunit;

namespace Tests
{
    public class SchedualeServiceTests
    {
        private SchedualeService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return new SchedualeService(context, TestHelper.CreateMapper());
        }

        // --- базовые ---

        [Fact]
        public async Task GetAttendance_ТренировкаНеНайдена_404()
        {
            var service = CreateService(nameof(GetAttendance_ТренировкаНеНайдена_404));
            var result = await service.GetAttendanceAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task MarkAttendance_ТренировкаНеНайдена_404()
        {
            var service = CreateService(nameof(MarkAttendance_ТренировкаНеНайдена_404));
            var result = await service.MarkAttendanceAsync(999, new List<AttendanceCreateRequest>());
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task MarkAttendance_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_Успешно));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>
            {
                new() { SportsmanId = 1, Status = AttendanceStatus.Present }
            });

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task MarkAttendance_ПустойСписок_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_ПустойСписок_Success));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>());

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task MarkAttendance_НесколькоСпортсменов_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_НесколькоСпортсменов_Success));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            context.Sportsmen.AddRange(
                new Sportsman { Id = 1, FIO = "Игрок1", UserId = 1 },
                new Sportsman { Id = 2, FIO = "Игрок2", UserId = 2 },
                new Sportsman { Id = 3, FIO = "Игрок3", UserId = 3 }
            );
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>
            {
                new() { SportsmanId = 1, Status = AttendanceStatus.Present },
                new() { SportsmanId = 2, Status = AttendanceStatus.Absent },
                new() { SportsmanId = 3, Status = AttendanceStatus.Late }
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task MarkAttendance_ПерезаписьСуществующей_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_ПерезаписьСуществующей_Success));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", UserId = 1 });
            context.Attendances.Add(new Attendance { Id = 1, TrainingId = 1, SportsmanId = 1, Status = AttendanceStatus.Absent });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            // перезаписываем — теперь Present
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>
            {
                new() { SportsmanId = 1, Status = AttendanceStatus.Present }
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(AttendanceStatus.Present, result.Data![0].Status);
        }

        [Fact]
        public async Task GetAttendance_ПустаяТренировка_ВозвращаетПустойСписок()
        {
            var context = TestHelper.CreateContext(nameof(GetAttendance_ПустаяТренировка_ВозвращаетПустойСписок));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.GetAttendanceAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task MarkAttendance_СтатусОтсутствует_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_СтатусОтсутствует_Success));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>
            {
                new() { SportsmanId = 1, Status = AttendanceStatus.Absent }
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(AttendanceStatus.Absent, result.Data![0].Status);
        }

        [Fact]
        public async Task MarkAttendance_СтатусОпоздал_Success()
        {
            var context = TestHelper.CreateContext(nameof(MarkAttendance_СтатусОпоздал_Success));
            context.Trainings.Add(new Training { Id = 1, TrainerId = 1, GroupId = 1, Type = "Общая", Date = DateTime.UtcNow });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());
            var result = await service.MarkAttendanceAsync(1, new List<AttendanceCreateRequest>
            {
                new() { SportsmanId = 1, Status = AttendanceStatus.Late }
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(AttendanceStatus.Late, result.Data![0].Status);
        }
    }
}
