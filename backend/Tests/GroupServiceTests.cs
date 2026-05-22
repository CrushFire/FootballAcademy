using Application.Services;
using Core.Entities;
using Core.Models.GroupModel;
using Xunit;

namespace Tests
{
    public class GroupServiceTests
    {
        private (GroupService service, DataAccess.ApplicationDbContext context) Create(string db)
        {
            var context = TestHelper.CreateContext(db);
            var service = new GroupService(context, TestHelper.CreateMapper());
            return (service, context);
        }

        // --- базовые ---

        [Fact]
        public async Task GetGroup_НесуществующийId_404()
        {
            var (service, _) = Create(nameof(GetGroup_НесуществующийId_404));
            var result = await service.GetGroupAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateGroup_ТренерНеНайден_404()
        {
            var (service, _) = Create(nameof(CreateGroup_ТренерНеНайден_404));
            var result = await service.CreateGroupAsync(new GroupCreateRequest { Name = "G1", TrainerId = 999 });
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CreateGroup_ДублНазвания_409()
        {
            var (service, context) = Create(nameof(CreateGroup_ДублНазвания_409));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", Description = "-", UserId = 1 });
            context.Groups.Add(new Group { Id = 1, Name = "Дубль1", Description = "-", TrainerId = 1 });
            await context.SaveChangesAsync();

            var result = await service.CreateGroupAsync(new GroupCreateRequest { Name = "Дубль1", TrainerId = 1, Description = "-" });
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task CreateGroup_Успешно()
        {
            var (service, context) = Create(nameof(CreateGroup_Успешно));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", Description = "-", UserId = 1 });
            await context.SaveChangesAsync();

            var result = await service.CreateGroupAsync(new GroupCreateRequest { Name = "Новая1", TrainerId = 1, Description = "-" });
            Assert.True(result.IsSuccess);
            Assert.Equal("Новая1", result.Data!.Name);
        }

        [Fact]
        public async Task DeleteGroup_НесуществующийId_404()
        {
            var (service, _) = Create(nameof(DeleteGroup_НесуществующийId_404));
            var result = await service.DeleteGroupAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGroup_НесуществующийId_404()
        {
            var (service, _) = Create(nameof(UpdateGroup_НесуществующийId_404));
            var result = await service.UpdateGroupAsync(new GroupUpdateRequest { Name = "X" }, 999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task GetGroups_ПустаяБД_ВозвращаетПустойСписок()
        {
            var (service, _) = Create(nameof(GetGroups_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetGroupsAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task CreateGroup_БезОписания_Success()
        {
            var (service, context) = Create(nameof(CreateGroup_БезОписания_Success));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", UserId = 1 });
            await context.SaveChangesAsync();

            var result = await service.CreateGroupAsync(new GroupCreateRequest
            {
                Name = "Юниоры16",
                TrainerId = 1,
                Description = "-"
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateGroup_НесколькоГрупп_УТогоЖеТренера_Success()
        {
            var (service, context) = Create(nameof(CreateGroup_НесколькоГрупп_УТогоЖеТренера_Success));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", UserId = 1 });
            await context.SaveChangesAsync();

            await service.CreateGroupAsync(new GroupCreateRequest { Name = "Группа1", TrainerId = 1 });
            var result = await service.CreateGroupAsync(new GroupCreateRequest { Name = "Группа2", TrainerId = 1 });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateGroup_ДублНазванияСДругойГруппой_409()
        {
            var (service, context) = Create(nameof(UpdateGroup_ДублНазванияСДругойГруппой_409));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", UserId = 1 });
            context.Groups.Add(new Group { Id = 1, Name = "Группа1", TrainerId = 1 });
            context.Groups.Add(new Group { Id = 2, Name = "Группа2", TrainerId = 1 });
            await context.SaveChangesAsync();

            var result = await service.UpdateGroupAsync(new GroupUpdateRequest { Name = "Группа2" }, 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task UpdateGroup_ТоЖеНазвание_Success()
        {
            var (service, context) = Create(nameof(UpdateGroup_ТоЖеНазвание_Success));
            context.Personal.Add(new Personal { Id = 1, FIO = "Тренер", Position = "Тренер", UserId = 1 });
            context.Groups.Add(new Group { Id = 1, Name = "Группа1", TrainerId = 1 });
            await context.SaveChangesAsync();

            // обновляем группу с тем же именем — не должно быть 409
            var result = await service.UpdateGroupAsync(new GroupUpdateRequest { Name = "Группа1" }, 1);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetGroups_НесколькоГрупп_ВозвращаетВсе()
        {
            var (service, context) = Create(nameof(GetGroups_НесколькоГрупп_ВозвращаетВсе));
            context.Groups.AddRange(
                new Group { Id = 1, Name = "А1", TrainerId = 1 },
                new Group { Id = 2, Name = "Б2", TrainerId = 1 },
                new Group { Id = 3, Name = "В3", TrainerId = 2 }
            );
            await context.SaveChangesAsync();

            var result = await service.GetGroupsAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task DeleteGroup_Успешно()
        {
            var (service, context) = Create(nameof(DeleteGroup_Успешно));
            context.Groups.Add(new Group { Id = 1, Name = "УдалитьМеня1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var result = await service.DeleteGroupAsync(1);
            Assert.True(result.IsSuccess);
        }
    }
}
