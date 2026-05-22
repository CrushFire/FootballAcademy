using Application.Services;
using AutoMapper;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public static class TestHelper
    {
        public static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<FootballAcademy.Mappers.AutoMapper>());
            return config.CreateMapper();
        }

        public static UserService CreateUserService(ApplicationDbContext context)
        {
            var mapper = CreateMapper();
            var imageService = new ImageService();
            var personalService = new PersonalService(context, mapper, imageService);
            var sportsmanService = new SportsmanService(context, mapper, imageService);
            return new UserService(context, mapper, personalService, sportsmanService);
        }
    }
}
