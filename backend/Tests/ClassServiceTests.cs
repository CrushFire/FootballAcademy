using Application.Services;
using Core.Entities;
using Core.Models.ClassModel;
using Xunit;

namespace Tests
{
    public class ClassServiceTests
    {
        private ClassService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return new ClassService(context, TestHelper.CreateMapper());
        }

        // --- базовые ---

        [Fact]
        public async Task GetClass_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetClass_НесуществующийId_404));
            var result = await service.GetClassAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateClass_ГруппаНеНайдена_404()
        {
            var service = CreateService(nameof(CreateClass_ГруппаНеНайдена_404));
            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 999, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(10, 0)
            });
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateClass_Дубль_409()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_Дубль_409));
            context.Groups.Add(new Group { Id = 1, Name = "G1", Description = "-", TrainerId = 1 });
            context.Classes.Add(new Class { Id = 1, GroupId = 1, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(10, 0), SportHall = "-" });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 1, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(10, 0)
            });
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task DeleteClass_НесуществующийId_404()
        {
            var service = CreateService(nameof(DeleteClass_НесуществующийId_404));
            var result = await service.DeleteClassAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task CreateClass_РазныеДниНедели_Success()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_РазныеДниНедели_Success));
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            int id = 1;
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
            {
                var result = await service.CreateClassAsync(new ClassCreateRequest
                {
                    GroupId = 1, DayOfWeek = day, BeginTime = new TimeOnly(10, 0), SportHall = "Зал А"
                });
                Assert.True(result.IsSuccess);
                id++;
            }
        }

        [Fact]
        public async Task CreateClass_ОдинДеньРазноеВремя_Success()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_ОдинДеньРазноеВремя_Success));
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());

            await service.CreateClassAsync(new ClassCreateRequest { GroupId = 1, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(9, 0), SportHall = "Зал А" });
            var result = await service.CreateClassAsync(new ClassCreateRequest { GroupId = 1, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(11, 0), SportHall = "Зал Б" });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateClass_РазныеГруппыОдноВремя_Success()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_РазныеГруппыОдноВремя_Success));
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            context.Groups.Add(new Group { Id = 2, Name = "Г2", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            await service.CreateClassAsync(new ClassCreateRequest { GroupId = 1, DayOfWeek = DayOfWeek.Tuesday, BeginTime = new TimeOnly(10, 0), SportHall = "Зал А" });
            var result = await service.CreateClassAsync(new ClassCreateRequest { GroupId = 2, DayOfWeek = DayOfWeek.Tuesday, BeginTime = new TimeOnly(10, 0), SportHall = "Зал Б" });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetClasses_ПустаяБД_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetClasses_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetClassesAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task DeleteClass_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(DeleteClass_Успешно));
            context.Classes.Add(new Class { Id = 1, GroupId = 1, DayOfWeek = DayOfWeek.Monday, BeginTime = new TimeOnly(10, 0), SportHall = "Зал" });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            var result = await service.DeleteClassAsync(1);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateClass_НесуществующийId_404()
        {
            var service = CreateService(nameof(UpdateClass_НесуществующийId_404));
            var result = await service.UpdateClassAsync(new ClassUpdateRequest { DayOfWeek = DayOfWeek.Friday, BeginTime = new TimeOnly(12, 0), SportHall = "Зал" }, 999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateClass_РаннееУтро_Success()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_РаннееУтро_Success));
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 1, DayOfWeek = DayOfWeek.Saturday, BeginTime = new TimeOnly(7, 0), SportHall = "Стадион"
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateClass_ВечернееВремя_Success()
        {
            var context = TestHelper.CreateContext(nameof(CreateClass_ВечернееВремя_Success));
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 1, DayOfWeek = DayOfWeek.Thursday, BeginTime = new TimeOnly(20, 30), SportHall = "Манеж"
            });
            Assert.True(result.IsSuccess);
        }
    }
}
