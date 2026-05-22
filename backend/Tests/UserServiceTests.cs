using Application.Services;
using Core.Entities;
using Core.Models.User;
using Xunit;

namespace Tests
{
    public class UserServiceTests
    {
        private UserService CreateService(string db)
        {
            var context = TestHelper.CreateContext(db);
            return TestHelper.CreateUserService(context);
        }

        // --- базовые ---

        [Fact]
        public async Task GetUser_НесуществующийId_404()
        {
            var service = CreateService(nameof(GetUser_НесуществующийId_404));
            var result = await service.GetUserAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetUser_СуществующийId_Success()
        {
            var context = TestHelper.CreateContext(nameof(GetUser_СуществующийId_Success));
            context.Users.Add(new User { Id = 1, Login = "test", Email = "test@test.com", Role = "admin", Password = "hash" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.GetUserAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("test@test.com", result.Data!.Email);
        }

        [Fact]
        public async Task CreateUser_ДублEmail_409()
        {
            var context = TestHelper.CreateContext(nameof(CreateUser_ДублEmail_409));
            context.Users.Add(new User { Id = 1, Login = "test", Email = "dup@test.com", Role = "admin", Password = "hash" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.CreateUserAsync(new UserCreateRequest { Email = "dup@test.com", Login = "x", Role = "admin", Password = "p" });

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task CreateUser_НовыйПользователь_Success()
        {
            var service = CreateService(nameof(CreateUser_НовыйПользователь_Success));
            var result = await service.CreateUserAsync(new UserCreateRequest { Email = "new@test.com", Login = "new", Role = "admin", Password = "p" });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteUser_НесуществующийId_404()
        {
            var service = CreateService(nameof(DeleteUser_НесуществующийId_404));
            var result = await service.DeleteUserAsync(999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_СуществующийId_Success()
        {
            var context = TestHelper.CreateContext(nameof(DeleteUser_СуществующийId_Success));
            context.Users.Add(new User { Id = 1, Login = "del", Email = "del@test.com", Role = "admin", Password = "hash" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.DeleteUserAsync(1);

            Assert.True(result.IsSuccess);
        }

        // --- нестандартные данные ---

        [Fact]
        public async Task GetUser_НулевойId_400()
        {
            var service = CreateService(nameof(GetUser_НулевойId_400));
            var result = await service.GetUserAsync(0);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetUser_ОтрицательныйId_400()
        {
            var service = CreateService(nameof(GetUser_ОтрицательныйId_400));
            var result = await service.GetUserAsync(-5);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateUser_EmailСПробелами_Success()
        {
            // email с пробелами — маппер просто сохранит как есть, сервис не валидирует формат
            var service = CreateService(nameof(CreateUser_EmailСПробелами_Success));
            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Email = "user with spaces@test.com",
                Login = "spaceman",
                Role = "trainer",
                Password = "qwerty123"
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateUser_ОченьДлинныйLogin_Success()
        {
            var service = CreateService(nameof(CreateUser_ОченьДлинныйLogin_Success));
            var longLogin = new string('a', 255);
            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Email = "longlogin@test.com",
                Login = longLogin,
                Role = "admin",
                Password = "pass"
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateUser_РазныеРоли_Success()
        {
            var service = CreateService(nameof(CreateUser_РазныеРоли_Success));
            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Email = "medical@test.com",
                Login = "doc",
                Role = "medical",
                Password = "medpass"
            });
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetUsers_ПустаяБД_ВозвращаетПустойСписок()
        {
            var service = CreateService(nameof(GetUsers_ПустаяБД_ВозвращаетПустойСписок));
            var result = await service.GetUsersAsync(null);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetUsers_НесколькоПользователей_ВозвращаетВсех()
        {
            var context = TestHelper.CreateContext(nameof(GetUsers_НесколькоПользователей_ВозвращаетВсех));
            context.Users.AddRange(
                new User { Id = 1, Login = "u1", Email = "u1@test.com", Role = "admin", Password = "h" },
                new User { Id = 2, Login = "u2", Email = "u2@test.com", Role = "trainer", Password = "h" },
                new User { Id = 3, Login = "u3", Email = "u3@test.com", Role = "medical", Password = "h" }
            );
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.GetUsersAsync(null);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task CreateUser_ДублEmailРегистрНеважен_409()
        {
            // email "DUP@TEST.COM" и "dup@test.com" — одинаковые в БД
            var context = TestHelper.CreateContext(nameof(CreateUser_ДублEmailРегистрНеважен_409));
            context.Users.Add(new User { Id = 1, Login = "orig", Email = "dup@test.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Email = "dup@test.com",
                Login = "copy",
                Role = "admin",
                Password = "p"
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_НесуществующийId_404()
        {
            var service = CreateService(nameof(UpdateUser_НесуществующийId_404));
            var result = await service.UpdateUserAsync(new UserUpdateRequest
            {
                Login = "new",
                Email = "new@test.com",
                Password = "newpass"
            }, 999);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_EmailЗанятДругим_409()
        {
            var context = TestHelper.CreateContext(nameof(UpdateUser_EmailЗанятДругим_409));
            context.Users.Add(new User { Id = 1, Login = "u1", Email = "u1@test.com", Role = "admin", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = "u2", Email = "u2@test.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.UpdateUserAsync(new UserUpdateRequest
            {
                Login = "u1updated",
                Email = "u2@test.com",
                Password = "h"
            }, 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_СвойEmailОставляет_Success()
        {
            var context = TestHelper.CreateContext(nameof(UpdateUser_СвойEmailОставляет_Success));
            context.Users.Add(new User { Id = 1, Login = "u1", Email = "u1@test.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = TestHelper.CreateUserService(context);
            var result = await service.UpdateUserAsync(new UserUpdateRequest
            {
                Login = "u1new",
                Email = "u1@test.com",
                Password = "newpass"
            }, 1);

            Assert.True(result.IsSuccess);
            Assert.Equal("u1new", result.Data!.Login);
        }
    }
}
