using Application.Services;
using Bogus;
using Core.Entities;
using Core.Enums;
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
using Core.Enums.Match;
using Xunit;

namespace Tests
{
    // Расширенные Faker-тесты: валидные и невалидные данные
    public class FakerExtendedTests
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
            Type = F(seed).PickRandom("Общая", "Тактическая", "Физическая")
        };

        // ═══════════════════════════════════════════════════════════════
        // UserService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        // Валидные: 20 разных пользователей
        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task User_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_User_Valid_{seed}");
            var service = new UserService(context, TestHelper.CreateMapper());

            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Login = f.Internet.UserName() + seed,
                Email = $"user{seed}@test{seed}.com",
                Password = f.Internet.Password(12),
                Role = f.PickRandom("admin", "trainer", "medical")
            });

            Assert.True(result.IsSuccess);
        }

        // Невалидные: дубль email
        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task User_ДублEmail_409(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_User_Dup_{seed}");
            var email = $"dup{seed}@test.com";
            context.Users.Add(new User { Id = 1, Login = $"u{seed}", Email = email, Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = new UserService(context, TestHelper.CreateMapper());
            var result = await service.CreateUserAsync(new UserCreateRequest
            {
                Login = $"new{seed}", Email = email, Role = "trainer", Password = "p"
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // GroupService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Group_Валидный_Success(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Group_Valid_{seed}");
            context.Personal.Add(MakePersonal(seed));
            await context.SaveChangesAsync();

            var service = new GroupService(context, TestHelper.CreateMapper());
            var result = await service.CreateGroupAsync(new GroupCreateRequest
            {
                Name = F(seed).PickRandom("Юниоры", "Дошкольники", "Подростки") + seed,
                TrainerId = 1,
                Description = F(seed).Lorem.Sentence()
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Group_НесуществующийТренер_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Group_NoTrainer_{seed}");
            var service = new GroupService(context, TestHelper.CreateMapper());

            var result = await service.CreateGroupAsync(new GroupCreateRequest
            {
                Name = "Группа" + seed,
                TrainerId = seed + 1000
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // TeamService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Team_Валидный_Success(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Team_Valid_{seed}");
            context.Personal.Add(MakePersonal(seed));
            await context.SaveChangesAsync();

            var service = new TeamService(context, TestHelper.CreateMapper(), new ImageService());
            var result = await service.CreateTeamAsync(new TeamCreateRequest
            {
                Name = F(seed).Company.CompanyName() + seed,
                TrainerId = 1,
                AgeGroup = F(seed).PickRandom<AgeGroup>()
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Team_НесуществующийТренер_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Team_NoTrainer_{seed}");
            var service = new TeamService(context, TestHelper.CreateMapper(), new ImageService());

            var result = await service.CreateTeamAsync(new TeamCreateRequest
            {
                Name = "Команда" + seed,
                TrainerId = seed + 1000,
                AgeGroup = AgeGroup.U16
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // TrainingService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Training_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Training_Valid_{seed}");
            var service = new TrainingService(context, TestHelper.CreateMapper());

            var result = await service.CreateTrainingAsync(new TrainingCreateRequest
            {
                TrainerId = f.Random.Long(1, 50),
                GroupId = f.Random.Long(1, 50),
                Date = f.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(3)),
                Type = f.PickRandom("Общая", "Тактическая", "Физическая", "Техническая", "Игровая"),
                OtherInformation = f.Random.Bool() ? f.Lorem.Sentence() : null
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Training_НесуществующийId_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Training_NotFound_{seed}");
            var service = new TrainingService(context, TestHelper.CreateMapper());

            var result = await service.GetTrainingAsync(seed + 10000);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // ClassService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Class_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Class_Valid_{seed}");
            context.Groups.Add(new Group { Id = 1, Name = "Г1", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new ClassService(context, TestHelper.CreateMapper());
            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = 1,
                DayOfWeek = f.PickRandom<DayOfWeek>(),
                BeginTime = new TimeOnly(f.Random.Int(7, 21), f.PickRandom(0, 15, 30, 45)),
                SportHall = f.Address.StreetName() + " зал"
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Class_НесуществующаяГруппа_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Class_NoGroup_{seed}");
            var service = new ClassService(context, TestHelper.CreateMapper());

            var result = await service.CreateClassAsync(new ClassCreateRequest
            {
                GroupId = seed + 1000,
                DayOfWeek = DayOfWeek.Monday,
                BeginTime = new TimeOnly(10, 0)
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // NormativeService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Normative_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Normative_Valid_{seed}");
            var service = new NormativeService(context, TestHelper.CreateMapper());

            var top = f.Random.Double(5, 200);
            var result = await service.CreateNormativeAsync(new NormativeCreateRequest
            {
                Type = f.PickRandom("Бег 30м", "Бег 60м", "Бег 100м", "Прыжок в длину", "Прыжок в высоту",
                                    "Отжимания", "Подтягивания", "Пресс", "Бег 1000м", "Бег 3000м"),
                Unit = f.PickRandom("сек", "см", "раз", "мин", "м"),
                Gender = f.PickRandom('М', 'Ж'),
                AgeGroup = f.Random.Int(6, 21),
                GradeExcellent = top,
                GradeGood = top * 0.9,
                GradeSatisfactory = top * 0.75,
                IsAboveYearOfStudy = f.Random.Bool()
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Normative_НесуществующийId_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Normative_NotFound_{seed}");
            var service = new NormativeService(context, TestHelper.CreateMapper());

            var result = await service.GetNormativeAsync(seed + 10000);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // MatchService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Match_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Match_Valid_{seed}");
            context.Teams.Add(new Team { Id = 1, Name = "Хозяева", TrainerId = 1 });
            await context.SaveChangesAsync();

            var service = new MatchService(context, TestHelper.CreateMapper());
            var result = await service.CreateMatchAsync(new MatchCreateRequest
            {
                HomeTeamId = 1,
                OpponentTeamName = f.Company.CompanyName() + seed,
                Date = f.Date.Between(DateTime.UtcNow, DateTime.UtcNow.AddMonths(6)),
                Type = f.PickRandom<GameType>()
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Match_НесуществующийId_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Match_NotFound_{seed}");
            var service = new MatchService(context, TestHelper.CreateMapper());

            var result = await service.GetMatchAsync(seed + 10000);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // MessageService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Message_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Message_Valid_{seed}");
            context.Users.Add(new User { Id = 1, Login = $"s{seed}", Email = $"s{seed}@s.com", Role = "trainer", Password = "h" });
            context.Users.Add(new User { Id = 2, Login = $"r{seed}", Email = $"r{seed}@r.com", Role = "admin", Password = "h" });
            await context.SaveChangesAsync();

            var service = new MessageService(context, TestHelper.CreateMapper());
            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest
            {
                ReceiverId = 2,
                Text = f.Lorem.Sentences(f.Random.Int(1, 5))
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Message_НесуществующийПолучатель_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Message_NoReceiver_{seed}");
            var service = new MessageService(context, TestHelper.CreateMapper());

            var result = await service.SendAsync(1, SenderRole.Trainer, new MessageSendRequest
            {
                ReceiverId = seed + 10000,
                Text = "Привет"
            });

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // SchedualeService — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Attendance_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Attendance_Valid_{seed}");
            context.Trainings.Add(MakeTraining(seed));
            var count = f.Random.Int(1, 8);
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

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Attendance_НесуществующаяТренировка_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Attendance_NoTraining_{seed}");
            var service = new SchedualeService(context, TestHelper.CreateMapper());

            var result = await service.MarkAttendanceAsync(seed + 10000, new List<AttendanceCreateRequest>());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        // ═══════════════════════════════════════════════════════════════
        // Broadcast — 40 тестов
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
        [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
        [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
        [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
        public async Task Broadcast_Валидный_Success(int seed)
        {
            var f = F(seed);
            var context = TestHelper.CreateContext($"FakerExt_Broadcast_Valid_{seed}");
            var service = new MessageService(context, TestHelper.CreateMapper());

            var result = await service.SendBroadcastAsync(seed, SenderRole.Trainer, new BroadcastCreateRequest
            {
                Title = f.Lorem.Sentence(f.Random.Int(2, 6)),
                Text = f.Lorem.Paragraphs(f.Random.Int(1, 3)),
                TargetType = f.PickRandom<BroadcastTargetType>(),
                ExpireAt = f.Random.Bool() ? f.Date.Future(14) : null
            });

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
        [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
        [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)] [InlineData(45)]
        [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)] [InlineData(50)]
        public async Task Broadcast_НесуществующийId_404(int seed)
        {
            var context = TestHelper.CreateContext($"FakerExt_Broadcast_NotFound_{seed}");
            var service = new MessageService(context, TestHelper.CreateMapper());

            var result = await service.GetBroadcastDetailsAsync(seed + 10000, null);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
