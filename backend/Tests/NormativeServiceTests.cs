using Application.Services;
using Core.Entities;
using Core.Enums;
using Core.Models.NormativeModel;
using Xunit;

namespace Tests
{
    public class NormativeServiceTests
    {
        private NormativeService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return new NormativeService(context, TestHelper.CreateMapper());
        }

        // --- базовые ---

        [Fact]
        public async Task GetNormative_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetNormative_НесуществующийId_404));
            var result = await service.GetNormativeAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateNormative_Успешно()
        {
            var service = CreateService(nameof(CreateNormative_Успешно));
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest { Type = "Бег 100м", Unit = "сек", Gender = 'М', AgeGroup = 14 });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteNormative_НесуществующийId_404()
        {
            var service = CreateService(nameof(DeleteNormative_НесуществующийId_404));
            var result = await service.DeleteNormativeAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetLocalNormative_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetLocalNormative_НесуществующийId_404));
            var result = await service.GetLocalNormativeAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateLocalNormative_Успешно()
        {
            var service = CreateService(nameof(CreateLocalNormative_Успешно));
            var result = await service.CreateLocalNormativeAsync(new LocalNormativeCreateRequest
            {
                Type = "Прыжок", Unit = "см", Gender = 'М', Specialization = Specialization.Football
            });
            Assert.True(result.IsSuccess);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task CreateNormative_ЖенскийПол_Success()
        {
            var service = CreateService(nameof(CreateNormative_ЖенскийПол_Success));
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest
            {
                Type = "Прыжок в длину", Unit = "см", Gender = 'Ж', AgeGroup = 12,
                GradeExcellent = 180, GradeGood = 160, GradeSatisfactory = 140
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateNormative_МаксимальныйВозраст_Success()
        {
            var service = CreateService(nameof(CreateNormative_МаксимальныйВозраст_Success));
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest
            {
                Type = "Бег 3000м", Unit = "мин", Gender = 'М', AgeGroup = 21
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateNormative_НулевыеОценки_Success()
        {
            var service = CreateService(nameof(CreateNormative_НулевыеОценки_Success));
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest
            {
                Type = "Тест", Unit = "раз", Gender = 'М', AgeGroup = 10,
                GradeExcellent = 0, GradeGood = 0, GradeSatisfactory = 0
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetNormatives_ПустаяБД_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetNormatives_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetNormativesAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetNormatives_НесколькоЗаписей_ВозвращаетВсе()
        {
            var context = TestHelper.CreateContext(nameof(GetNormatives_НесколькоЗаписей_ВозвращаетВсе));
            context.Normatives.AddRange(
                new Normative { Id = 1, Type = "Бег", Unit = "сек", Gender = 'М', AgeGroup = 10 },
                new Normative { Id = 2, Type = "Прыжок", Unit = "см", Gender = 'Ж', AgeGroup = 12 },
                new Normative { Id = 3, Type = "Отжимания", Unit = "раз", Gender = 'М', AgeGroup = 14 }
            );
            await context.SaveChangesAsync();

            var service = new NormativeService(context, TestHelper.CreateMapper());
            var result = await service.GetNormativesAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task DeleteNormative_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(DeleteNormative_Успешно));
            context.Normatives.Add(new Normative { Id = 1, Type = "Бег", Unit = "сек", Gender = 'М', AgeGroup = 10 });
            await context.SaveChangesAsync();

            var service = new NormativeService(context, TestHelper.CreateMapper());
            var result = await service.DeleteNormativeAsync(1);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateLocalNormative_Minifootball_Success()
        {
            var service = CreateService(nameof(CreateLocalNormative_Minifootball_Success));
            var result = await service.CreateLocalNormativeAsync(new LocalNormativeCreateRequest
            {
                Type = "Удар по воротам", Unit = "раз", Gender = 'М',
                Specialization = Specialization.Minifootball, Value = 10, IsMoreBetter = true
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateLocalNormative_IsMoreBetterFalse_Success()
        {
            var service = CreateService(nameof(CreateLocalNormative_IsMoreBetterFalse_Success));
            var result = await service.CreateLocalNormativeAsync(new LocalNormativeCreateRequest
            {
                Type = "Бег 60м", Unit = "сек", Gender = 'М',
                Specialization = Specialization.Football, Value = 8.5, IsMoreBetter = false
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddNormativeResult_Успешно()
        {
            var context = TestHelper.CreateContext(nameof(AddNormativeResult_Успешно));
            context.Normatives.Add(new Normative { Id = 1, Type = "Бег", Unit = "сек", Gender = 'М', AgeGroup = 14 });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Иванов", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new NormativeService(context, TestHelper.CreateMapper());
            var result = await service.AddNormativeResultAsync(new NormativeSportsmanCreateRequest
            {
                SportsmanId = 1, NormativeId = 1, Result = 12.5
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetNormativeResults_ПустойСписок()
        {
            var service = CreateService(nameof(GetNormativeResults_ПустойСписок));
            var result = await service.GetNormativeResultsAsync(999);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task UpdateNormative_НесуществующийId_404()
        {
            var service = CreateService(nameof(UpdateNormative_НесуществующийId_404));
            var result = await service.UpdateNormativeAsync(new NormativeUpdateRequest { Type = "X", Unit = "y" }, 999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task AddNormativeResult_ОченьВысокийРезультат_Success()
        {
            var context = TestHelper.CreateContext(nameof(AddNormativeResult_ОченьВысокийРезультат_Success));
            context.Normatives.Add(new Normative { Id = 1, Type = "Прыжок", Unit = "см", Gender = 'М', AgeGroup = 16 });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Суперигрок", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new NormativeService(context, TestHelper.CreateMapper());
            var result = await service.AddNormativeResultAsync(new NormativeSportsmanCreateRequest
            {
                SportsmanId = 1, NormativeId = 1, Result = 9999.99
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddNormativeResult_НулевойРезультат_Success()
        {
            var context = TestHelper.CreateContext(nameof(AddNormativeResult_НулевойРезультат_Success));
            context.Normatives.Add(new Normative { Id = 1, Type = "Бег", Unit = "сек", Gender = 'М', AgeGroup = 10 });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", UserId = 1 });
            await context.SaveChangesAsync();

            var service = new NormativeService(context, TestHelper.CreateMapper());
            var result = await service.AddNormativeResultAsync(new NormativeSportsmanCreateRequest
            {
                SportsmanId = 1, NormativeId = 1, Result = 0
            });
            Assert.True(result.IsSuccess);
        }
    }
}
