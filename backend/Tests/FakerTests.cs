using Application.Services;
using Application.Services.MetricAnalytic;
using Bogus;
using Core.Entities;
using Core.Enums;
using Core.Enums.Match;
using Core.Models.AttendanceModel;
using Core.Models.BroadcastModel;
using Core.Models.ClassModel;
using Core.Models.GroupModel;
using Core.Models.MatchModel;
using Core.Models.MessageModel;
using Core.Models.NormativeModel;
using Core.Models.TeamModel;
using Core.Models.TrainingModel;
using Core.Models.User;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Tests
{
    // Тесты с автогенерацией входных данных через Bogus
    // Ожидание: данные валидны → сервис возвращает Success
    public class FakerTests
    {
        private static Faker F(int seed) => new Faker("ru") { Random = new Randomizer(seed) };

        private static Personal MakePersonal(int seed) => new()
        {
            Id = 1, UserId = 1,
            FIO = F(seed).Name.FullName(),
            Position = F(seed).Name.JobTitle(),
            Description = F(seed).Lorem.Sentence()
        };

        private static Sportsman MakeSportsman(int seed, long id = 1) => new()
        {
            Id = id, UserId = id,
            FIO = F(seed).Name.FullName(),
            BirthDate = F(seed).Date.Past(18, DateTime.Now.AddYears(-8)),
            Height = F(seed).Random.Int(140, 200),
            Weight = F(seed).Random.Int(40, 100),
            Gender = F(seed).PickRandom('M', 'F'),
            Specialization = F(seed).PickRandom<Specialization>()
        };

        private static Training MakeTraining(int seed, long id = 1) => new()
        {
            Id = id, TrainerId = 1, GroupId = 1,
            Date = F(seed).Date.Recent(30),
            Type = F(seed).PickRandom("Общая", "Тактическая", "Физическая", "Техническая")
        };

        private static TrainingMetrics MakeMetrics(int seed, long id = 1) => new()
        {
            Id = id, SportsmanId = 1, TrainingId = 1,
            PlayerLoad = F(seed).Random.Double(50, 450),
            MaximumSpeed = F(seed).Random.Double(10, 35),
            TotalDistance = F(seed).Random.Int(2000, 12000),
            SprintDistance = F(seed).Random.Int(100, 1500),
            LowSpeedDistance = F(seed).Random.Int(500, 4000),
            ModerateSpeedDistance = F(seed).Random.Int(500, 3000),
            HighSpeedDistance = F(seed).Random.Int(100, 1000),
            AccelerationCount = F(seed).Random.Int(10, 150),
            DecelerationCount = F(seed).Random.Int(10, 150),
            SprintEfforts = F(seed).Random.Int(1, 20),
            ExplosiveEfforts = F(seed).Random.Int(1, 15),
            MaxHeartRate = F(seed).Random.Int(140, 195),
            AverageHeartRate = F(seed).Random.Int(110, 170),
            TimeInSpeedZone1 = F(seed).Random.Int(300, 1200),
            TimeInHRRedZone = F(seed).Random.Int(0, 500),
            Duration = F(seed).Random.Int(1800, 7200)
        };

        // --- UserService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task User_СоздатьСлучайногоПользователя_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_User_{seed}");
            var service = new UserService(context, TestHelper.CreateMapper());

            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Login = f.Internet.UserName(),
                Email = f.Internet.Email(),
                Password = f.Internet.Password(10),
                Role = f.PickRandom("admin", "trainer", "medical")
            });

            Assert.True(result.IsSuccess);
        }

        // --- GroupService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Group_СоздатьСлучайнуюГруппу_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Group_{seed}");
            context.Personal.Add(MakePersonal(seed));
            await context.SaveChangesAsync();

            var service = new GroupService(context, TestHelper.CreateMapper());
            var name = f.PickRandom("Юниоры", "Дошкольники", "Подростки", "Молодёжь") + seed;

            var result = await service.CreateGroupAsync(new GroupCreateRequest
            {
                Name = name,
                TrainerId = 1,
                Description = f.Lorem.Sentence()
            });

            Assert.True(result.IsSuccess);
        }

        // --- TeamService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Team_СоздатьСлучайнуюКоманду_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Team_{seed}");
            context.Personal.Add(MakePersonal(seed));
            await context.SaveChangesAsync();

            var service = new TeamService(context, TestHelper.CreateMapper(), new ImageService());

            var result = await service.CreateTeamAsync(new TeamCreateRequest
            {
                Name = f.Company.CompanyName() + seed,
                TrainerId = 1,
                AgeGroup = f.PickRandom<AgeGroup>()
            });

            Assert.True(result.IsSuccess);
        }

        // --- TrainingService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Training_СоздатьСлучайнуюТренировку_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Training_{seed}");
            var service = new TrainingService(context, TestHelper.CreateMapper());

            var result = await service.CreateTrainingAsync(new TrainingCreateRequest
            {
                TrainerId = f.Random.Long(1, 100),
                GroupId = f.Random.Long(1, 100),
                Date = f.Date.Recent(60),
                Type = f.PickRandom("Общая", "Тактическая", "Физическая", "Техническая"),
                OtherInformation = f.Lorem.Sentence()
            });

            Assert.True(result.IsSuccess);
        }

        // --- ClassService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Class_СоздатьСлучайноеЗанятие_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Class_{seed}");
            context.Groups.Add(new Group { Id = 1, Name = "Группа1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());

            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 1,
                DayOfWeek = f.PickRandom<DayOfWeek>(),
                BeginTime = new TimeOnly(f.Random.Int(7, 20), f.PickRandom(0, 30)),
                SportHall = f.Address.StreetName()
            });

            Assert.True(result.IsSuccess);
        }

        // --- NormativeService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Normative_СоздатьСлучайныйНорматив_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Normative_{seed}");
            var service = new NormativeService(context, TestHelper.CreateMapper());

            var excellent = f.Random.Double(10, 100);
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest
            {
                Type = f.PickRandom("Бег 60м", "Прыжок", "Отжимания", "Подтягивания", "Бег 1000м"),
                Unit = f.PickRandom("сек", "см", "раз", "мин"),
                Gender = f.PickRandom('М', 'Ж'),
                AgeGroup = f.Random.Int(8, 21),
                GradeExcellent = excellent,
                GradeGood = excellent * 0.85,
                GradeSatisfactory = excellent * 0.7
            });

            Assert.True(result.IsSuccess);
        }

        // --- MatchService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Match_СоздатьСлучайныйМатч_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Match_{seed}");
            context.Teams.Add(new Team { Id = 1, Name = "Команда", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());

            var result = await service.CreateMatchAsync(new MatchCreateRequest
            {
                HomeTeamId = 1,
                OpponentTeamName = f.Company.CompanyName(),
                Date = f.Date.Future(30),
                Type = f.PickRandom<GameType>()
            });

            Assert.True(result.IsSuccess);
        }

        // --- MessageService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Message_ОтправитьСлучайноеСообщение_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Message_{seed}");
            context.Users.Add(new User { Id = 1, Login = $"s{seed}", Email = $"s{seed}@s.com", Role = "trainer", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = $"r{seed}", Email = $"r{seed}@r.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());

            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest
            {
                ReceiverId = 2,
                Text = f.Lorem.Paragraph()
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.ReceiverId);
        }

        // --- SchedualeService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Attendance_ОтметитьСлучайнуюПосещаемость_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Attendance_{seed}");
            context.Trainings.Add(MakeTraining(seed));

            var count = f.Random.Int(1, 5);
            for (int i = 1; i <= count; i++)
                context.Sportsmen.Add(MakeSportsman(seed * 100 + i, i));

            await context.SaveChangesAsync();

            var service = new SchedualeService(context, TestHelper.CreateMapper());

            var requests = Enumerable.Range(1, count).Select(i => new AttendanceCreateRequest
            {
                SportsmanId = i,
                Status = f.PickRandom<AttendanceStatus>()
            }).ToList();

            var result = await service.MarkAttendanceAsync(1, requests);

            Assert.True(result.IsSuccess);
            Assert.Equal(count, result.Data!.Count);
        }

        // --- MainMetricService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task MainMetric_СлучайныеМетрики_Success(int seed)
        {
            var context = TestHelper.CreateContext($"Faker_MainMetric_{seed}");
            var sportsman = MakeSportsman(seed);
            context.Sportsmen.Add(sportsman);
            context.Trainings.Add(MakeTraining(seed));
            context.TrainingMetrics.Add(MakeMetrics(seed));
            await context.SaveChangesAsync();

            var service = new MainMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanAverageMetricsAsync(1, null);

            Assert.True(result.IsSuccess);
            Assert.Equal(sportsman.FIO, result.Data!.SportsmanName);
        }

        // --- PentagonService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Pentagon_СлучайныеМетрики_ВсеОсиВДиапазоне(int seed)
        {
            var context = TestHelper.CreateContext($"Faker_Pentagon_{seed}");
            var sportsman = MakeSportsman(seed);
            sportsman.Position = F(seed).PickRandom<Position>();
            context.Sportsmen.Add(sportsman);
            context.Trainings.Add(MakeTraining(seed));
            context.TrainingMetrics.Add(MakeMetrics(seed));
            await context.SaveChangesAsync();

            var service = new PentagonService(context, TestHelper.CreateMapper(),
                new MemoryCache(new MemoryCacheOptions()));
            var result = await service.GetSportsmanPentagonAsync(1);

            Assert.True(result.IsSuccess);
            var p = result.Data!.Pentagon;
            Assert.InRange(p.Speed, 0.0, 1.0);
            Assert.InRange(p.Power, 0.0, 1.0);
            Assert.InRange(p.Sprints, 0.0, 1.0);
            Assert.InRange(p.Endurance, 0.0, 1.0);
            Assert.InRange(p.Explosive, 0.0, 1.0);
        }

        // --- MedicalMetricService ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Medical_СлучайныеМетрики_НеПадает(int seed)
        {
            var context = TestHelper.CreateContext($"Faker_Medical_{seed}");
            context.Sportsmen.Add(MakeSportsman(seed));
            context.Trainings.Add(MakeTraining(seed));
            context.TrainingMetrics.Add(MakeMetrics(seed));
            await context.SaveChangesAsync();

            var service = new MedicalMetricService(context, TestHelper.CreateMapper());
            var result = await service.GetSportsmanMedicalCheckAsync(1);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.CheckResult);
        }

        // --- Broadcast ---

        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task Broadcast_СоздатьСлучайнуюРассылку_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"Faker_Broadcast_{seed}");
            var service = new MessageService(context, TestHelper.CreateMapper());

            var result = await service.SendBroadcastAsync(seed, SenderRole.Trainer, new BroadcastCreateRequest
            {
                Title = f.Lorem.Sentence(3),
                Text = f.Lorem.Paragraph(),
                TargetType = BroadcastTargetType.All,
                ExpireAt = f.Date.Future(7)
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Data!.RecipientsCount);
        }
    }
}
