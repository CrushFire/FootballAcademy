using Application.Services;
using Core.Entities;
using Core.Models.TeamModel;
using Xunit;

namespace Tests
{
    public class TeamServiceTests
    {
        private (TeamService service, DataAccess.ApplicationDbContext context) Create(string db)
        {
            var context = TestHelper.CreateContext(db);
            var service = new TeamService(context, TestHelper.CreateMapper(), new ImageService());
            return (service, context);
        }

        [Fact]
        public async Task GetTeam_НесуществующийId_404()
        {
            var (service, _) = Create(nameof(GetTeam_НесуществующийId_404));
            var result = await service.GetTeamAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateTeam_ТренерНеНайден_404()
        {
            var (service, _) = Create(nameof(CreateTeam_ТренерНеНайден_404));
            var result = await service.CreateTeamAsync(new TeamCreateRequest { Name = "T1", TrainerId = 999 });
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateTeam_ДублНазвания_409()
        {
            var (service, context) = Create(nameof(CreateTeam_ДублНазвания_409));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", Description = "-", UserId = 1 });
            context.Teams.Add(new Team { Id = 1, Name = "Дубль", TrainerId = 1 });
            await context.SaveChangesAsync();

            var result = await service.CreateTeamAsync(new TeamCreateRequest { Name = "Дубль", TrainerId = 1 });
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task AddSportsmanToTeam_УжеВКоманде_409()
        {
            var (service, context) = Create(nameof(AddSportsmanToTeam_УжеВКоманде_409));
            context.Teams.Add(new Team { Id = 1, Name = "T", TrainerId = 1 });
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", TeamId = 1, UserId = 1 });
            await context.SaveChangesAsync();

            var result = await service.AddSportsmanToTeamAsync(1, 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task RemoveSportsmanFromTeam_БезКоманды_400()
        {
            var (service, context) = Create(nameof(RemoveSportsmanFromTeam_БезКоманды_400));
            context.Sportsmen.Add(new Sportsman { Id = 1, FIO = "Игрок", TeamId = null, UserId = 1 });
            await context.SaveChangesAsync();

            var result = await service.RemoveSportsmanFromTeamAsync(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task DeleteTeam_НесуществующийId_404()
        {
            var (service, _) = Create(nameof(DeleteTeam_НесуществующийId_404));
            var result = await service.DeleteTeamAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
