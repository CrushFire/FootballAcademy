using Application.Services;
using Core.Entities;
using Core.Enums.Match;
using Core.Models.MatchModel;
using Xunit;

namespace Tests
{
    public class MatchServiceTests
    {
        private MatchService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return new MatchService(context, TestHelper.CreateMapper());
        }

        // --- базовые ---

        [Fact]
        public async Task GetMatch_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetMatch_НесуществующийId_404));
            var result = await service.GetMatchAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateMatch_БезСоперника_400()
        {
            var service = CreateService(nameof(CreateMatch_БезСоперника_400));
            var result = await service.CreateMatchAsync(new MatchCreateRequest { HomeTeamId = 1, Date = DateTime.UtcNow, Type = GameType.Friendly });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateMatch_ОбаСоперника_400()
        {
            var service = CreateService(nameof(CreateMatch_ОбаСоперника_400));
            var result = await service.CreateMatchAsync(new MatchCreateRequest
            {
                HomeTeamId = 1, OpponentTeamId = 2, OpponentTeamName = "Внешний",
                Date = DateTime.UtcNow, Type = GameType.Friendly
            });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task StartMatch_НесуществующийId_404()
        {
            var service = CreateService(nameof(StartMatch_НесуществующийId_404));
            var result = await service.StartMatchAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteMatch_НесуществующийId_404()
        {
            var service = CreateService(nameof(DeleteMatch_НесуществующийId_404));
            var result = await service.DeleteMatchAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task AddEvent_МинутаВнеДиапазона_400()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_МинутаВнеДиапазона_400));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = MatchEventType.Goal, IsHomeTeam = true, Minute = 200 });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task StartMatch_УжеНачатый_400()
        {
            var context = TestHelper.CreateContext(nameof(StartMatch_УжеНачатый_400));
            context.Teams.Add(new Team { Id = 1, Name = "T", TrainerId = 1 });
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.StartMatchAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task StartMatch_УжеЗавершённый_400()
        {
            var context = TestHelper.CreateContext(nameof(StartMatch_УжеЗавершённый_400));
            context.Teams.Add(new Team { Id = 1, Name = "T", TrainerId = 1 });
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.Finished, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.StartMatchAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddEvent_МинутаНоль_Success()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_МинутаНоль_Success));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = MatchEventType.Goal, IsHomeTeam = true, Minute = 0 });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddEvent_МинутаМаксимальная130_Success()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_МинутаМаксимальная130_Success));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = MatchEventType.Goal, IsHomeTeam = false, Minute = 130 });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddEvent_МинутаОтрицательная_400()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_МинутаОтрицательная_400));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = MatchEventType.Goal, IsHomeTeam = true, Minute = -1 });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddEvent_ВЗавершённыйМатч_400()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_ВЗавершённыйМатч_400));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.Finished, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = MatchEventType.Goal, IsHomeTeam = true, Minute = 45 });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddEvent_РазныеТипыСобытий_Success()
        {
            var context = TestHelper.CreateContext(nameof(AddEvent_РазныеТипыСобытий_Success));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.InProgress, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            int min = 1;
            foreach (var type in new[] { MatchEventType.Goal, MatchEventType.YellowCard, MatchEventType.RedCard, MatchEventType.Corner, MatchEventType.Foul })
            {
                var result = await service.AddEventAsync(1, new MatchEventCreateRequest { Type = type, IsHomeTeam = true, Minute = min++ });
                Assert.True(result.IsSuccess);
            }
        }

        [Fact]
        public async Task FinishMatch_НесуществующийId_404()
        {
            var service = CreateService(nameof(FinishMatch_НесуществующийId_404));
            var result = await service.FinishMatchAsync(999, new MatchFinishRequest { Result = MatchResult.Win });
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task FinishMatch_УжеЗавершённый_400()
        {
            var context = TestHelper.CreateContext(nameof(FinishMatch_УжеЗавершённый_400));
            context.Teams.Add(new Team { Id = 1, Name = "T", TrainerId = 1 });
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.Finished, Type = GameType.Friendly, Result = MatchResult.Win });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.FinishMatchAsync(1, new MatchFinishRequest { Result = MatchResult.Loss });
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetMatches_ПустаяБД_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetMatches_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetMatchesAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task DeleteEvent_НесуществующийId_404()
        {
            var service = CreateService(nameof(DeleteEvent_НесуществующийId_404));
            var result = await service.DeleteEventAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetEvents_НесуществующийМатч_404()
        {
            var service = CreateService(nameof(GetEvents_НесуществующийМатч_404));
            var result = await service.GetEventsAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetEvents_МатчБезСобытий_ПустойСписок()
        {
            var context = TestHelper.CreateContext(nameof(GetEvents_МатчБезСобытий_ПустойСписок));
            context.Matches.Add(new Match { Id = 1, HomeTeamId = 1, Status = MatchStatus.Scheduled, Type = GameType.Friendly });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.GetEventsAsync(1);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }
    }
}
