using Application.Utils;
using Core.Entities;
using Core.Enums;
using Core.Enums.Match;
using Core.Interfaces.Services;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("seed")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _hasher;
        private readonly IRagService _rag;

        public SeedController(ApplicationDbContext context, PasswordHasher hasher, IRagService rag)
        {
            _context = context;
            _hasher = hasher;
            _rag = rag;
        }

        // ─── helpers ───────────────────────────────────────────────────────────

        private static string PosGroup(Position? pos) => pos switch
        {
            Position.GK => "Goalkeeper",
            Position.CB or Position.LB or Position.RB or Position.LWB or Position.RWB => "Defender",
            Position.CM or Position.CDM or Position.CAM => "Midfielder",
            Position.LW or Position.RW => "Winger",
            Position.ST or Position.CF or Position.SS => "Forward",
            _ => "Unknown"
        };

        // Реалистичные диапазоны MaxSpeed (км/ч) по возрасту (источник: Buchheit et al., youth academy benchmarks)
        private static (double min, double max) MaxSpeedRangeByAge(int age) => age switch
        {
            <= 10 => (16.0, 22.0),
            <= 12 => (19.0, 25.0),
            <= 14 => (22.0, 28.0),
            <= 16 => (25.0, 31.0),
            <= 18 => (27.0, 33.0),
            _     => (29.0, 36.0)
        };

        // AvgSpeed (км/ч) — средняя по тренировке с учётом пауз/ходьбы (типично 4-9 км/ч для футбольных сессий)
        private static (double min, double max) AvgSpeedRangeByAge(int age) => age switch
        {
            <= 10 => (3.5, 6.5),
            <= 12 => (4.0, 7.0),
            <= 14 => (4.5, 7.5),
            <= 16 => (5.0, 8.0),
            _     => (5.5, 8.5)
        };

        private static TrainingMetrics MakeWeakMetrics(Random rnd, long trainingId, long sportsmanId, Position? pos, DateTime date, int age)
        {
            // Слабые метрики: низкая дистанция, мало спринтов, высокий пульс покоя, плохая выносливость
            var total    = rnd.Next(3200, 6000);
            var sprint   = rnd.Next(50, 280);
            var high     = rnd.Next(200, 700);
            var moderate = rnd.Next(600, 1500);
            var low      = Math.Max(total - sprint - high - moderate, 300);
            // Слабый игрок: скорости берём из нижней половины возрастного диапазона
            var (avgMin, avgMax) = AvgSpeedRangeByAge(age);
            var (maxMin, maxMax) = MaxSpeedRangeByAge(age);
            double weakAvg = avgMin + rnd.NextDouble() * (avgMax - avgMin) * 0.6;
            double weakMax = maxMin + rnd.NextDouble() * (maxMax - maxMin) * 0.6;

            return new TrainingMetrics
            {
                TrainingId   = trainingId,
                SportsmanId  = sportsmanId,
                Duration     = rnd.Next(2400, 4200),
                TotalDistance         = total,
                SprintDistance        = sprint,
                HighSpeedDistance     = high,
                ModerateSpeedDistance = moderate,
                LowSpeedDistance      = low,
                TimeInSpeedZone1 = rnd.Next(600, 1400),
                TimeInSpeedZone2 = rnd.Next(400, 900),
                TimeInSpeedZone3 = rnd.Next(150, 400),
                TimeInSpeedZone4 = rnd.Next(60,  200),
                TimeInSpeedZone5 = rnd.Next(20,  80),
                TimeInSpeedZone6 = rnd.Next(5,   40),
                TimeInSpeedZone7 = rnd.Next(0,   15),
                DistanceInSpeedZone1 = rnd.Next(500, 1200),
                DistanceInSpeedZone2 = rnd.Next(400, 900),
                DistanceInSpeedZone3 = rnd.Next(200, 600),
                DistanceInSpeedZone4 = rnd.Next(100, 400),
                DistanceInSpeedZone5 = rnd.Next(50,  200),
                DistanceInSpeedZone6 = rnd.Next(20,  100),
                DistanceInSpeedZone7 = rnd.Next(5,   50),
                AverageSpeed      = Math.Round(weakAvg, 2),
                MaximumSpeed      = Math.Round(weakMax, 2),
                AccelerationCount = rnd.Next(6,  22),
                DecelerationCount = rnd.Next(6,  22),
                MaxAcceleration   = Math.Round(rnd.NextDouble() * 1.2 + 1.5, 2),
                MaxDeceleration   = Math.Round(rnd.NextDouble() * 1.2 + 1.5, 2),
                SprintEfforts     = rnd.Next(1,  7),
                HighSpeedEfforts  = rnd.Next(3,  14),
                ExplosiveEfforts  = rnd.Next(1,  5),
                PlayerLoad        = Math.Round(rnd.NextDouble() * 120 + 80,  1),
                Energy            = Math.Round(rnd.NextDouble() * 200 + 120, 1),
                WorkRatio         = Math.Round(rnd.NextDouble() * 1.0 + 0.5, 2),
                MetabolicPower    = Math.Round(rnd.NextDouble() * 3   + 3,   2),
                Impacts           = rnd.Next(2,  12),
                AverageSatellites = rnd.Next(7,  12),
                AverageHDOP       = Math.Round(rnd.NextDouble() * 0.6 + 1.0, 2),
                Position          = pos?.ToString(),
                PositionGroup     = PosGroup(pos),
                AverageHeartRate  = rnd.Next(160, 185),
                MaxHeartRate      = rnd.Next(188, 204),
                HeartRateExertion = Math.Round(rnd.NextDouble() * 150 + 200, 1),
                TimeInHRZone1     = rnd.Next(30,  150),
                TimeInHRZone2     = rnd.Next(80,  250),
                TimeInHRZone3     = rnd.Next(150, 450),
                TimeInHRZone4     = rnd.Next(400, 900),
                TimeInHRZone5     = rnd.Next(300, 800),
                CreatedAt         = date,
            };
        }

        // Детерминированный «талант»-множитель по sportsmanId — даёт стабильный разброс между игроками,
        // чтобы профили (Sprinter/PowerPlayer/...) имели смысл. Диапазон 0.80-1.25 (около 80%-125% от среднего).
        private static double TalentMultiplier(long sportsmanId) =>
            0.80 + ((sportsmanId * 2654435761L) % 1000) / 1000.0 * 0.45;

        private static TrainingMetrics MakeMetrics(Random rnd, long trainingId, long sportsmanId, Position? pos, DateTime date, int age)
        {
            var talent   = TalentMultiplier(sportsmanId);   // 0.80..1.25
            var total    = rnd.Next(5500, 11000);
            // SprintDistance: 1-3% от total с поправкой на «талант» (литература U14-U16: ~2% baseline,
            // Football Observatory: 200м спринта при 10км total ≈ 2%).
            var sprint   = (int)(total * (0.01 + rnd.NextDouble() * 0.02) * talent);
            // HighSpeedDistance: 5-10% от total с поправкой (литература: 600-1000м high-speed при 10-13км)
            var high     = (int)(total * (0.05 + rnd.NextDouble() * 0.05) * talent);
            var moderate = rnd.Next(1200, 3200);
            var low      = Math.Max(total - sprint - high - moderate, 300);
            // Возрастные диапазоны скорости (нормальные/сильные игроки)
            var (avgMin, avgMax) = AvgSpeedRangeByAge(age);
            var (maxMin, maxMax) = MaxSpeedRangeByAge(age);

            return new TrainingMetrics
            {
                TrainingId   = trainingId,
                SportsmanId  = sportsmanId,
                Duration     = rnd.Next(3600, 6300),
                TotalDistance         = total,
                SprintDistance        = sprint,
                HighSpeedDistance     = high,
                ModerateSpeedDistance = moderate,
                LowSpeedDistance      = low,
                TimeInSpeedZone1 = rnd.Next(400, 1000),
                TimeInSpeedZone2 = rnd.Next(300, 800),
                TimeInSpeedZone3 = rnd.Next(250, 700),
                TimeInSpeedZone4 = rnd.Next(200, 600),
                // Подростковый уровень (литература STATSports/FIFA WC: ~5% времени в high-intensity у юношей)
                TimeInSpeedZone5 = (int)(rnd.Next(80, 300) * talent),
                TimeInSpeedZone6 = (int)(rnd.Next(40, 165) * talent),
                TimeInSpeedZone7 = (int)(rnd.Next(15, 75)  * talent),
                DistanceInSpeedZone1 = rnd.Next(300, 800),
                DistanceInSpeedZone2 = rnd.Next(400, 1000),
                DistanceInSpeedZone3 = rnd.Next(500, 1300),
                DistanceInSpeedZone4 = rnd.Next(400, 1100),
                DistanceInSpeedZone5 = rnd.Next(300, 900),
                DistanceInSpeedZone6 = rnd.Next(150, 600),
                DistanceInSpeedZone7 = rnd.Next(80,  350),
                AverageSpeed      = Math.Round((avgMin + rnd.NextDouble() * (avgMax - avgMin)) * talent, 2),
                MaximumSpeed      = Math.Round((maxMin + rnd.NextDouble() * (maxMax - maxMin)) * talent, 2),
                AccelerationCount = (int)(rnd.Next(28, 70) * talent),    // Литература U14-U18: 30-50/трен
                DecelerationCount = (int)(rnd.Next(28, 70) * talent),
                MaxAcceleration   = Math.Round(rnd.NextDouble() * 2.5 + 2.0, 2),
                MaxDeceleration   = Math.Round(rnd.NextDouble() * 2.5 + 2.0, 2),
                SprintEfforts     = (int)(rnd.Next(9, 21) * talent),     // Юноши: 9-12 за матч, у нас 9-21
                HighSpeedEfforts  = (int)(rnd.Next(15, 45) * talent),
                ExplosiveEfforts  = (int)(rnd.Next(20, 45) * talent),
                PlayerLoad        = Math.Round((rnd.NextDouble() * 200 + 350) * talent, 1),
                Energy            = Math.Round(rnd.NextDouble() * 550 + 250, 1),
                WorkRatio         = Math.Round(rnd.NextDouble() * 2.2 + 1.0, 2),
                MetabolicPower    = Math.Round(rnd.NextDouble() * 6   + 5,   2),
                Impacts           = rnd.Next(6,  40),
                AverageSatellites = rnd.Next(9,  15),
                AverageHDOP       = Math.Round(rnd.NextDouble() * 0.4 + 0.7, 2),
                Position          = pos?.ToString(),
                PositionGroup     = PosGroup(pos),
                AverageHeartRate  = rnd.Next(130, 168),
                MaxHeartRate      = rnd.Next(170, 197),
                HeartRateExertion = Math.Round(rnd.NextDouble() * 280 + 120, 1),
                TimeInHRZone1     = rnd.Next(120, 500),
                TimeInHRZone2     = rnd.Next(200, 700),
                TimeInHRZone3     = rnd.Next(300, 900),
                TimeInHRZone4     = rnd.Next(200, 700),
                TimeInHRZone5     = rnd.Next(100, 450),
                TimeInHRZone6     = rnd.Next(50,  250),
                TimeInHRRedZone   = rnd.Next(0,   200),
                CreatedAt         = date
            };
        }

        private static TrainingMetrics MakeOverloadMetrics(Random rnd, long trainingId, long sportsmanId, Position? pos, DateTime date, int idx, int age)
        {
            // Чередуем очень высокую и низкую нагрузку → std > 50, FatigueIndex > 1.2
            var playerLoad = idx % 2 == 0
                ? Math.Round(rnd.NextDouble() * 50 + 480, 1)   // высокая: 480–530
                : Math.Round(rnd.NextDouble() * 50 + 150, 1);  // низкая: 150–200

            var total    = rnd.Next(5500, 11000);
            // SprintDistance: 1-3% от total (по литературе для baseline)
            var sprint   = (int)(total * (0.01 + rnd.NextDouble() * 0.02));
            // HighSpeedDistance: 5-10% от total (Bush 2015)
            var high     = (int)(total * (0.05 + rnd.NextDouble() * 0.05));
            var moderate = rnd.Next(1200, 3200);
            var low      = Math.Max(total - sprint - high - moderate, 300);
            // Возрастные диапазоны скорости — как у обычных метрик
            var (avgMin, avgMax) = AvgSpeedRangeByAge(age);
            var (maxMin, maxMax) = MaxSpeedRangeByAge(age);

            return new TrainingMetrics
            {
                TrainingId   = trainingId,
                SportsmanId  = sportsmanId,
                Duration     = rnd.Next(3600, 6300),
                TotalDistance         = total,
                SprintDistance        = sprint,
                HighSpeedDistance     = high,
                ModerateSpeedDistance = moderate,
                LowSpeedDistance      = low,
                TimeInSpeedZone1 = rnd.Next(400, 1000),
                TimeInSpeedZone2 = rnd.Next(300, 800),
                TimeInSpeedZone3 = rnd.Next(250, 700),
                TimeInSpeedZone4 = rnd.Next(200, 600),
                TimeInSpeedZone5 = rnd.Next(150, 500),
                TimeInSpeedZone6 = rnd.Next(80,  300),
                TimeInSpeedZone7 = rnd.Next(30,  150),
                DistanceInSpeedZone1 = rnd.Next(300, 800),
                DistanceInSpeedZone2 = rnd.Next(400, 1000),
                DistanceInSpeedZone3 = rnd.Next(500, 1300),
                DistanceInSpeedZone4 = rnd.Next(400, 1100),
                DistanceInSpeedZone5 = rnd.Next(300, 900),
                DistanceInSpeedZone6 = rnd.Next(150, 600),
                DistanceInSpeedZone7 = rnd.Next(80,  350),
                AverageSpeed      = Math.Round(avgMin + rnd.NextDouble() * (avgMax - avgMin), 2),
                MaximumSpeed      = Math.Round(maxMin + rnd.NextDouble() * (maxMax - maxMin), 2),
                AccelerationCount = rnd.Next(28, 70),
                DecelerationCount = rnd.Next(28, 70),
                MaxAcceleration   = Math.Round(rnd.NextDouble() * 2.5 + 2.0, 2),
                MaxDeceleration   = Math.Round(rnd.NextDouble() * 2.5 + 2.0, 2),
                SprintEfforts     = rnd.Next(9, 21),
                HighSpeedEfforts  = rnd.Next(15, 45),
                ExplosiveEfforts  = rnd.Next(20, 45),
                PlayerLoad        = playerLoad,
                Energy            = Math.Round(rnd.NextDouble() * 550 + 250, 1),
                WorkRatio         = Math.Round(rnd.NextDouble() * 2.2 + 1.0, 2),
                MetabolicPower    = Math.Round(rnd.NextDouble() * 6   + 5,   2),
                Impacts           = rnd.Next(6,  40),
                AverageSatellites = rnd.Next(9,  15),
                AverageHDOP       = Math.Round(rnd.NextDouble() * 0.4 + 0.7, 2),
                Position          = pos?.ToString(),
                PositionGroup     = PosGroup(pos),
                AverageHeartRate  = rnd.Next(130, 168),
                MaxHeartRate      = rnd.Next(170, 197),
                HeartRateExertion = Math.Round(rnd.NextDouble() * 280 + 120, 1),
                TimeInHRZone1     = rnd.Next(120, 500),
                TimeInHRZone2     = rnd.Next(200, 700),
                TimeInHRZone3     = rnd.Next(300, 900),
                TimeInHRZone4     = rnd.Next(200, 700),
                TimeInHRZone5     = rnd.Next(100, 450),
                TimeInHRZone6     = rnd.Next(50,  250),
                TimeInHRRedZone   = rnd.Next(0,   200),
                CreatedAt         = date
            };
        }

        // ─── POST /seed ────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            var rnd = new Random(42);

            // ── Пользователи ──────────────────────────────────────────────────
            var uAdmin    = new User { Login = "admin",   Email = "admin@academy.ru",    Password = _hasher.HashPassword("admin123"),    Role = "admin" };
            var uIvanov   = new User { Login = "ivanov",  Email = "ivanov@academy.ru",   Password = _hasher.HashPassword("trainer123"),  Role = "personal" };
            var uPetrov   = new User { Login = "petrov",  Email = "petrov@academy.ru",   Password = _hasher.HashPassword("trainer123"),  Role = "personal" };
            var uTrainer  = new User { Login = "trainer", Email = "trainer@example.com", Password = _hasher.HashPassword("string"),      Role = "personal" };
            var uMedical  = new User { Login = "doctor",  Email = "doctor@academy.ru",   Password = _hasher.HashPassword("medical123"), Role = "personal" };
            await _context.Users.AddRangeAsync(uAdmin, uIvanov, uPetrov, uTrainer, uMedical);
            await _context.SaveChangesAsync();

            // ── Персонал ──────────────────────────────────────────────────────
            var pIvanov  = new Personal { UserId = uIvanov.Id,  FIO = "Иванов Алексей Петрович",   Position = "Главный тренер",  Type = PersonalType.Trainer, Description = "Тренер U14/U16, стаж 12 лет" };
            var pPetrov  = new Personal { UserId = uPetrov.Id,  FIO = "Петров Сергей Иванович",    Position = "Тренер",          Type = PersonalType.Trainer, Description = "Тренер младших групп, стаж 7 лет" };
            var pTrainer = new Personal { UserId = uTrainer.Id, FIO = "Сергеев Иван Сергеевич",    Position = "Тренер",          Type = PersonalType.Trainer, Description = "Тренер групп U14/U16" };
            var pMedical = new Personal { UserId = uMedical.Id, FIO = "Сидорова Мария Николаевна", Position = "Спортивный врач", Type = PersonalType.Medical, Description = "Врач сборных команд академии" };
            await _context.Personal.AddRangeAsync(pIvanov, pPetrov, pTrainer, pMedical);
            await _context.SaveChangesAsync();

            // ── Команды ───────────────────────────────────────────────────────
            // Академические команды (основные для матчей)
            // Год рождения = текущий год − U (U14 → 2012, U16 → 2010), формат как у оппонентов: "Синие (2012) - 1"
            var nowYear = DateTime.UtcNow.Year;
            var yearU14 = nowYear - 14;
            var yearU16 = nowYear - 16;
            var teamU14Ivanov  = new Team { Name = $"Синие ({yearU14}) - 1",   TrainerId = pIvanov.Id,  AgeGroup = AgeGroup.U14 };
            var teamU16Ivanov  = new Team { Name = $"Синие ({yearU16}) - 1",   TrainerId = pIvanov.Id,  AgeGroup = AgeGroup.U16 };
            var teamU14Trainer = new Team { Name = $"Красные ({yearU14}) - 1", TrainerId = pTrainer.Id, AgeGroup = AgeGroup.U14 };
            var teamU16Trainer = new Team { Name = $"Красные ({yearU16}) - 1", TrainerId = pTrainer.Id, AgeGroup = AgeGroup.U16 };
            var teamU14B       = new Team { Name = $"Синие ({yearU14}) - 2",   TrainerId = pIvanov.Id,  AgeGroup = AgeGroup.U14 };
            var teamU16B       = new Team { Name = $"Красные ({yearU16}) - 2", TrainerId = pTrainer.Id, AgeGroup = AgeGroup.U16 };
            await _context.Teams.AddRangeAsync(teamU14Ivanov, teamU16Ivanov, teamU14Trainer, teamU16Trainer, teamU14B, teamU16B);
            await _context.SaveChangesAsync();

            // ── Группы ────────────────────────────────────────────────────────
            var gU14Ivanov  = new Group { Name = "U14-А",  TrainerId = pIvanov.Id,  Description = "Основная группа U14" };
            var gU16Ivanov  = new Group { Name = "U16-А",  TrainerId = pIvanov.Id,  Description = "Основная группа U16" };
            var gU14Trainer = new Group { Name = "U14-Б",  TrainerId = pTrainer.Id, Description = "Группа U14" };
            var gU16Trainer = new Group { Name = "U16-Б",  TrainerId = pTrainer.Id, Description = "Группа U16" };
            var gU12Petrov  = new Group { Name = "U12",    TrainerId = pPetrov.Id,  Description = "Младшая группа U12" };
            await _context.Groups.AddRangeAsync(gU14Ivanov, gU16Ivanov, gU14Trainer, gU16Trainer, gU12Petrov);
            await _context.SaveChangesAsync();

            // ── Расписание занятий ────────────────────────────────────────────
            await _context.Classes.AddRangeAsync(
                new Class { GroupId = gU14Ivanov.Id,  DayOfWeek = DayOfWeek.Monday,    BeginTime = new TimeOnly(10, 0), SportHall = "Стадион А",  WeekType = WeekType.Any },
                new Class { GroupId = gU14Ivanov.Id,  DayOfWeek = DayOfWeek.Wednesday, BeginTime = new TimeOnly(10, 0), SportHall = "Стадион А",  WeekType = WeekType.Any },
                new Class { GroupId = gU14Ivanov.Id,  DayOfWeek = DayOfWeek.Friday,    BeginTime = new TimeOnly(10, 0), SportHall = "Зал А",      WeekType = WeekType.Any },
                new Class { GroupId = gU16Ivanov.Id,  DayOfWeek = DayOfWeek.Tuesday,   BeginTime = new TimeOnly(15, 0), SportHall = "Стадион А",  WeekType = WeekType.Any },
                new Class { GroupId = gU16Ivanov.Id,  DayOfWeek = DayOfWeek.Thursday,  BeginTime = new TimeOnly(15, 0), SportHall = "Стадион А",  WeekType = WeekType.Any },
                new Class { GroupId = gU16Ivanov.Id,  DayOfWeek = DayOfWeek.Saturday,  BeginTime = new TimeOnly(11, 0), SportHall = "Стадион Б",  WeekType = WeekType.Any },
                new Class { GroupId = gU14Trainer.Id, DayOfWeek = DayOfWeek.Monday,    BeginTime = new TimeOnly(14, 0), SportHall = "Зал В",      WeekType = WeekType.Any },
                new Class { GroupId = gU14Trainer.Id, DayOfWeek = DayOfWeek.Thursday,  BeginTime = new TimeOnly(14, 0), SportHall = "Зал В",      WeekType = WeekType.Any },
                new Class { GroupId = gU16Trainer.Id, DayOfWeek = DayOfWeek.Tuesday,   BeginTime = new TimeOnly(16, 0), SportHall = "Стадион Б",  WeekType = WeekType.Any },
                new Class { GroupId = gU16Trainer.Id, DayOfWeek = DayOfWeek.Friday,    BeginTime = new TimeOnly(16, 0), SportHall = "Стадион Б",  WeekType = WeekType.Any },
                new Class { GroupId = gU12Petrov.Id,  DayOfWeek = DayOfWeek.Wednesday, BeginTime = new TimeOnly(12, 0), SportHall = "Зал Б",      WeekType = WeekType.Any },
                new Class { GroupId = gU12Petrov.Id,  DayOfWeek = DayOfWeek.Saturday,  BeginTime = new TimeOnly(10, 0), SportHall = "Зал Б",      WeekType = WeekType.Any }
            );
            await _context.SaveChangesAsync();

            // ── Нормативы ─────────────────────────────────────────────────────
            var norm14Run60   = new Normative { AgeGroup = 14, Gender = 'M', Type = "Бег 60м",           Unit = "сек", GradeExcellent = 7.9,  GradeGood = 8.4,  GradeSatisfactory = 9.0 };
            var norm14Jump    = new Normative { AgeGroup = 14, Gender = 'M', Type = "Прыжок в длину",     Unit = "см",  GradeExcellent = 185,  GradeGood = 168,  GradeSatisfactory = 152 };
            var norm14Pullup  = new Normative { AgeGroup = 14, Gender = 'M', Type = "Подтягивания",       Unit = "раз", GradeExcellent = 10,   GradeGood = 7,    GradeSatisfactory = 5 };
            var norm14Shuttle = new Normative { AgeGroup = 14, Gender = 'M', Type = "Челночный бег 4×9м", Unit = "сек", GradeExcellent = 9.1,  GradeGood = 9.6,  GradeSatisfactory = 10.2 };
            var norm16Run100  = new Normative { AgeGroup = 16, Gender = 'M', Type = "Бег 100м",           Unit = "сек", GradeExcellent = 12.3, GradeGood = 12.9, GradeSatisfactory = 13.8 };
            var norm16Jump    = new Normative { AgeGroup = 16, Gender = 'M', Type = "Прыжок в длину",     Unit = "см",  GradeExcellent = 200,  GradeGood = 185,  GradeSatisfactory = 170 };
            var norm16Pullup  = new Normative { AgeGroup = 16, Gender = 'M', Type = "Подтягивания",       Unit = "раз", GradeExcellent = 14,   GradeGood = 11,   GradeSatisfactory = 8 };
            var norm16Shuttle = new Normative { AgeGroup = 16, Gender = 'M', Type = "Челночный бег 4×9м", Unit = "сек", GradeExcellent = 8.7,  GradeGood = 9.2,  GradeSatisfactory = 9.8 };
            var norm12Run30   = new Normative { AgeGroup = 12, Gender = 'M', Type = "Бег 30м",            Unit = "сек", GradeExcellent = 5.1,  GradeGood = 5.5,  GradeSatisfactory = 5.9 };
            var norm12Jump    = new Normative { AgeGroup = 12, Gender = 'M', Type = "Прыжок в длину",     Unit = "см",  GradeExcellent = 165,  GradeGood = 150,  GradeSatisfactory = 135 };
            await _context.Normatives.AddRangeAsync(
                norm14Run60, norm14Jump, norm14Pullup, norm14Shuttle,
                norm16Run100, norm16Jump, norm16Pullup, norm16Shuttle,
                norm12Run30, norm12Jump
            );

            // Локальные нормативы академии
            var lnSprintDist = new LocalNormative { Specialization = Specialization.Football, Type = "Дистанция спринта за тренировку", Unit = "м",   Gender = 'M', Value = 400,  IsMoreBetter = true };
            var lnMaxSpeed   = new LocalNormative { Specialization = Specialization.Football, Type = "Максимальная скорость",           Unit = "км/ч", Gender = 'M', Value = 25.0, IsMoreBetter = true };
            var lnPlayerLoad = new LocalNormative { Specialization = Specialization.Football, Type = "Нагрузка (PlayerLoad)",           Unit = "у.е.", Gender = 'M', Value = 150,  IsMoreBetter = true };
            await _context.LocalNormatives.AddRangeAsync(lnSprintDist, lnMaxSpeed, lnPlayerLoad);
            await _context.SaveChangesAsync();

            // ── Спортсмены ────────────────────────────────────────────────────
            // (fio, birth, height, weight, pos, teamId, groupId)
            var sData = new (string fio, DateTime birth, int h, int w, Position pos, long? teamId, long groupId)[]
            {
                // U14 Иванов (10 человек, teamU14Ivanov)
                ("Волков Никита Сергеевич",       new DateTime(2011, 1, 10), 162, 55, Position.GK,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Козлов Дмитрий Андреевич",      new DateTime(2010, 3, 15), 165, 58, Position.CM,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Морозов Артём Викторович",       new DateTime(2010, 7, 22), 168, 62, Position.ST,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Лебедев Иван Олегович",          new DateTime(2010, 5, 30), 170, 65, Position.CB,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Новиков Павел Дмитриевич",       new DateTime(2010, 9, 18), 166, 60, Position.LW,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Орлов Виктор Романович",         new DateTime(2011, 2, 5),  163, 57, Position.RB,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Беляев Кирилл Максимович",       new DateTime(2010,11, 20), 167, 61, Position.CDM, teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Тихонов Сергей Алексеевич",      new DateTime(2011, 4, 8),  164, 59, Position.CAM, teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Громов Илья Владимирович",       new DateTime(2010, 6, 25), 169, 63, Position.LB,  teamU14Ivanov.Id, gU14Ivanov.Id),
                ("Кудряшов Алексей Николаевич",    new DateTime(2011, 8, 12), 161, 56, Position.RW,  teamU14Ivanov.Id, gU14Ivanov.Id),

                // U16 Иванов (10 человек, teamU16Ivanov)
                ("Соколов Егор Александрович",     new DateTime(2008, 4, 12), 175, 68, Position.CDM, teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Попов Максим Игоревич",          new DateTime(2008, 8, 25), 178, 72, Position.RB,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Зайцев Кирилл Романович",        new DateTime(2009, 2, 7),  172, 66, Position.CAM, teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Фёдоров Арсений Николаевич",     new DateTime(2009, 6, 19), 173, 67, Position.LB,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Кравцов Данила Игоревич",        new DateTime(2008, 10, 3), 177, 71, Position.ST,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Мельников Роман Сергеевич",      new DateTime(2009, 1, 14), 176, 69, Position.CM,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Лазарев Антон Дмитриевич",       new DateTime(2008, 7, 28), 174, 70, Position.CB,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Воронов Никита Павлович",        new DateTime(2009, 3, 22), 179, 73, Position.CF,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Климов Андрей Юрьевич",          new DateTime(2008, 12, 9), 180, 75, Position.GK,  teamU16Ivanov.Id, gU16Ivanov.Id),
                ("Туров Евгений Константинович",   new DateTime(2009, 5, 1),  175, 68, Position.LW,  teamU16Ivanov.Id, gU16Ivanov.Id),

                // U14 Сергеев (8 человек, teamU14Trainer)
                ("Смирнов Алексей Петрович",       new DateTime(2010, 4, 20), 164, 57, Position.GK,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Кузнецов Иван Дмитриевич",       new DateTime(2010, 6, 15), 167, 61, Position.CB,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Васильев Максим Андреевич",      new DateTime(2010, 8, 10), 169, 63, Position.CM,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Петров Артём Сергеевич",         new DateTime(2010,10, 5),  166, 59, Position.ST,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Михайлов Егор Олегович",         new DateTime(2010,12, 1),  165, 58, Position.LW,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Романов Денис Викторович",       new DateTime(2011, 3, 18), 163, 56, Position.RB,  teamU14Trainer.Id, gU14Trainer.Id),
                ("Борисов Никита Андреевич",       new DateTime(2010, 7, 7),  168, 62, Position.CDM, teamU14Trainer.Id, gU14Trainer.Id),
                ("Зубков Тимофей Иванович",        new DateTime(2011, 5, 25), 162, 55, Position.CAM, teamU14Trainer.Id, gU14Trainer.Id),

                // U16 Сергеев (8 человек, teamU16Trainer)
                ("Фёдоров Дмитрий Иванович",       new DateTime(2008, 3, 25), 174, 67, Position.CDM, teamU16Trainer.Id, gU16Trainer.Id),
                ("Александров Павел Романович",    new DateTime(2008, 5, 18), 177, 71, Position.RB,  teamU16Trainer.Id, gU16Trainer.Id),
                ("Николаев Кирилл Викторович",     new DateTime(2008, 7, 12), 175, 69, Position.CAM, teamU16Trainer.Id, gU16Trainer.Id),
                ("Сергеев Тимур Александрович",    new DateTime(2008, 9, 8),  176, 70, Position.CF,  teamU16Trainer.Id, gU16Trainer.Id),
                ("Андреев Арсений Дмитриевич",     new DateTime(2008,11, 22), 173, 68, Position.LB,  teamU16Trainer.Id, gU16Trainer.Id),
                ("Матвеев Игорь Петрович",         new DateTime(2009, 2, 14), 178, 72, Position.CM,  teamU16Trainer.Id, gU16Trainer.Id),
                ("Трифонов Захар Сергеевич",       new DateTime(2009, 4, 3),  174, 67, Position.GK,  teamU16Trainer.Id, gU16Trainer.Id),
                ("Карпов Максим Геннадьевич",      new DateTime(2008, 6, 19), 176, 70, Position.ST,  teamU16Trainer.Id, gU16Trainer.Id),

                // U12 Петров (6 человек, без команды)
                ("Егоров Степан Артёмович",        new DateTime(2013, 1, 8),  152, 44, Position.GK,  null, gU12Petrov.Id),
                ("Ильин Захар Дмитриевич",         new DateTime(2012,10, 22), 155, 47, Position.CB,  null, gU12Petrov.Id),
                ("Жуков Никита Игоревич",          new DateTime(2013, 3, 15), 150, 43, Position.CM,  null, gU12Petrov.Id),
                ("Крылов Артём Владимирович",      new DateTime(2012, 8, 30), 153, 45, Position.ST,  null, gU12Petrov.Id),
                ("Никитин Антон Павлович",         new DateTime(2013, 5, 12), 151, 44, Position.LW,  null, gU12Petrov.Id),
                ("Харитонов Дмитрий Сергеевич",   new DateTime(2012,12, 7),  154, 46, Position.RB,  null, gU12Petrov.Id),
            };

            var logins = new[]
            {
                // U14 Ivanov
                "nikitavol","kozlov","morozov","lebedev","novikov","orlov","belyaev","tihonov","gromov","kudryashov",
                // U16 Ivanov
                "sokolov","popov","zaycev","fedorov","kravcov","melnikov","lazarev","voronov","klimov","turov",
                // U14 Trainer
                "smirnov","kuznecov","vasiliev","petrov2","mihajlov","romanov","borisov","zubkov",
                // U16 Trainer
                "fedorov2","alexandrov","nikolaev","sergeev2","andreev","matveev","trifonov","karpov",
                // U12 Petrov
                "egorov","ilin","zhukov","krylov","nikitin","haritonov",
            };

            var sUsers = new List<User>();
            for (int i = 0; i < sData.Length; i++)
                sUsers.Add(new User { Login = logins[i], Email = $"{logins[i]}@academy.ru", Password = _hasher.HashPassword("sportsman123"), Role = "sportsman" });
            await _context.Users.AddRangeAsync(sUsers);
            await _context.SaveChangesAsync();

            var sportsmen = new List<Sportsman>();
            for (int i = 0; i < sData.Length; i++)
            {
                var d = sData[i];
                sportsmen.Add(new Sportsman
                {
                    UserId = sUsers[i].Id, FIO = d.fio, BirthDate = d.birth,
                    Height = d.h, Weight = d.w, Position = d.pos,
                    Gender = 'M', TeamId = d.teamId, Specialization = Specialization.Football
                });
            }
            await _context.Sportsmen.AddRangeAsync(sportsmen);
            await _context.SaveChangesAsync();

            for (int i = 0; i < sData.Length; i++)
                _context.SportsmanGroups.Add(new SportsmanGroup { SportsmanId = sportsmen[i].Id, GroupId = sData[i].groupId });
            await _context.SaveChangesAsync();

            // ── Тренировки (2 года) ───────────────────────────────────────────
            // ~3 раза в неделю × 52 недели × 2 = 312 тренировок на группу, делаем ~80 на группу
            var trainingTypes = new[] { "Общая", "Физическая", "Тактическая", "Техническая", "Силовая", "Вратарская", "Игровая", "Восстановительная" };

            List<Training> MakeTrainings(long trainerId, long groupId, int count, int startDaysAgo) =>
                Enumerable.Range(0, count).Select(i => new Training
                {
                    TrainerId = trainerId,
                    GroupId   = groupId,
                    Type      = trainingTypes[i % trainingTypes.Length],
                    Date      = DateTime.UtcNow.AddDays(-(startDaysAgo - i * (startDaysAgo / count)))
                }).ToList();

            var trU14Ivanov  = MakeTrainings(pIvanov.Id,  gU14Ivanov.Id,  80, 730);
            var trU16Ivanov  = MakeTrainings(pIvanov.Id,  gU16Ivanov.Id,  80, 730);
            var trU14Trainer = MakeTrainings(pTrainer.Id, gU14Trainer.Id, 50, 730);
            var trU16Trainer = MakeTrainings(pTrainer.Id, gU16Trainer.Id, 50, 730);
            var trU12Petrov  = MakeTrainings(pPetrov.Id,  gU12Petrov.Id,  40, 365);

            var allTrainings = trU14Ivanov.Concat(trU16Ivanov).Concat(trU14Trainer).Concat(trU16Trainer).Concat(trU12Petrov).ToList();
            await _context.Trainings.AddRangeAsync(allTrainings);
            await _context.SaveChangesAsync();

            // ── Метрики ───────────────────────────────────────────────────────
            var allMetrics = new List<TrainingMetrics>();
            var allAttend  = new List<Attendance>();
            var attStatuses = new[] { AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Late, AttendanceStatus.Absent, AttendanceStatus.ExcusedAbsent };

            void AddMetricsForGroup(IEnumerable<Training> trainings, IEnumerable<int> sportsmenIdxs)
            {
                var idxList = sportsmenIdxs.ToList();
                foreach (var tr in trainings)
                {
                    for (int si = 0; si < idxList.Count; si++)
                    {
                        var s = sportsmen[idxList[si]];
                        var absent = rnd.Next(100) < 10; // 10% пропускают
                        var status = absent
                            ? (rnd.Next(2) == 0 ? AttendanceStatus.Absent : AttendanceStatus.ExcusedAbsent)
                            : (rnd.Next(100) < 12 ? AttendanceStatus.Late : AttendanceStatus.Present);

                        allAttend.Add(new Attendance { SportsmanId = s.Id, TrainingId = tr.Id, Status = status });

                        if (status != AttendanceStatus.Absent && status != AttendanceStatus.ExcusedAbsent)
                            allMetrics.Add(MakeMetrics(rnd, tr.Id, s.Id, s.Position, tr.Date, s.Age));
                    }
                }
            }

            // Все кроме Новикова Павла (индекс 4) — нормальные метрики
            AddMetricsForGroup(trU14Ivanov,  Enumerable.Range(0, 4).Concat(Enumerable.Range(5, 5)));
            AddMetricsForGroup(trU16Ivanov,  Enumerable.Range(10, 10));
            AddMetricsForGroup(trU14Trainer, Enumerable.Range(20, 8));
            AddMetricsForGroup(trU16Trainer, Enumerable.Range(28, 8));
            AddMetricsForGroup(trU12Petrov,  Enumerable.Range(36, 6));

            // Новиков Павел — перегруз: нестабильный PlayerLoad (std > 20)
            var novikov = sportsmen[4];
            for (int i = 0; i < trU14Ivanov.Count; i++)
            {
                var tr = trU14Ivanov[i];
                allAttend.Add(new Attendance { SportsmanId = novikov.Id, TrainingId = tr.Id, Status = AttendanceStatus.Present });
                allMetrics.Add(MakeOverloadMetrics(rnd, tr.Id, novikov.Id, novikov.Position, tr.Date, i, novikov.Age));
            }

            await _context.TrainingMetrics.AddRangeAsync(allMetrics);
            await _context.Attendances.AddRangeAsync(allAttend);
            await _context.SaveChangesAsync();

            // Устанавливаем дату регистрации = дата первой тренировки
            var metricsBySportsman = allMetrics.GroupBy(m => m.SportsmanId);
            var trainingDateById = allTrainings.ToDictionary(t => t.Id, t => t.Date);
            foreach (var group in metricsBySportsman)
            {
                var sportsman = sportsmen.FirstOrDefault(s => s.Id == group.Key);
                if (sportsman == null) continue;
                var oldestDate = group.Min(m => trainingDateById.TryGetValue(m.TrainingId, out var d) ? d : DateTime.UtcNow);
                sportsman.CreatedAt = oldestDate;
            }
            await _context.SaveChangesAsync();

            // ── Нормативы спортсменов ─────────────────────────────────────────
            // Тесты проводятся каждые 6 месяцев (~4 замера за 2 года)
            var normResults = new List<NormativeSportsman>();
            var lnResults   = new List<LocalNormativeSportsman>();

            // U14: norm14Run60, norm14Jump, norm14Pullup, norm14Shuttle
            var norm14list = new[] { norm14Run60, norm14Jump, norm14Pullup, norm14Shuttle };
            var norm16list = new[] { norm16Run100, norm16Jump, norm16Pullup, norm16Shuttle };
            var norm12list = new[] { norm12Run30, norm12Jump };

            // Базовые значения и прогрессия для каждого норматива
            // U14: Run60 — 9.2→8.5, Jump — 158→176, Pullup — 5→9, Shuttle — 10.5→9.4
            double[][] u14Progress = {
                new[] { 9.2, 9.0, 8.8, 8.5 },   // Run60 (меньше = лучше)
                new[] { 158, 163, 169, 176.0 },  // Jump
                new[] { 5, 6, 7, 9.0 },           // Pullup
                new[] { 10.5, 10.1, 9.7, 9.4 },  // Shuttle (меньше = лучше)
            };
            // U16: Run100 — 13.8→12.8, Jump — 182→198, Pullup — 9→13, Shuttle — 9.8→8.9
            double[][] u16Progress = {
                new[] { 13.8, 13.4, 13.1, 12.8 },
                new[] { 182, 187, 193, 198.0 },
                new[] { 9, 10, 12, 13.0 },
                new[] { 9.8, 9.5, 9.2, 8.9 },
            };
            // U12
            double[][] u12Progress = {
                new[] { 6.0, 5.8, 5.6 },
                new[] { 138, 145, 152.0 },
            };

            var testDates4 = new[] { DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(-1) };
            var testDates3 = new[] { DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(-1) };

            void AddNormResults(Sportsman s, Normative[] norms, double[][] progress, DateTime[] dates)
            {
                for (int di = 0; di < dates.Length; di++)
                    for (int ni = 0; ni < norms.Length; ni++)
                    {
                        var baseVal = progress[ni][di];
                        var jitter = (rnd.NextDouble() - 0.5) * baseVal * 0.04; // ±2% разброс
                        normResults.Add(new NormativeSportsman
                        {
                            SportsmanId = s.Id,
                            NormativeId = norms[ni].Id,
                            Result      = Math.Round(baseVal + jitter, 2),
                            CreatedAt   = dates[di]
                        });
                    }
            }

            void AddLnResults(Sportsman s, DateTime[] dates)
            {
                foreach (var date in dates)
                {
                    lnResults.Add(new LocalNormativeSportsman { SportsmanId = s.Id, LocalNormativeId = lnSprintDist.Id, Result = Math.Round(rnd.NextDouble() * 500 + 300, 1), CreatedAt = date });
                    lnResults.Add(new LocalNormativeSportsman { SportsmanId = s.Id, LocalNormativeId = lnMaxSpeed.Id,   Result = Math.Round(rnd.NextDouble() * 6   + 22, 1), CreatedAt = date });
                    lnResults.Add(new LocalNormativeSportsman { SportsmanId = s.Id, LocalNormativeId = lnPlayerLoad.Id, Result = Math.Round(rnd.NextDouble() * 200 + 180, 1), CreatedAt = date });
                }
            }

            for (int i = 0; i < 10; i++) { AddNormResults(sportsmen[i], norm14list, u14Progress, testDates4); AddLnResults(sportsmen[i], testDates4); }
            for (int i = 10; i < 20; i++) { AddNormResults(sportsmen[i], norm16list, u16Progress, testDates4); AddLnResults(sportsmen[i], testDates4); }
            for (int i = 20; i < 28; i++) { AddNormResults(sportsmen[i], norm14list, u14Progress, testDates4); AddLnResults(sportsmen[i], testDates4); }
            for (int i = 28; i < 36; i++) { AddNormResults(sportsmen[i], norm16list, u16Progress, testDates4); AddLnResults(sportsmen[i], testDates4); }
            for (int i = 36; i < 42; i++) { AddNormResults(sportsmen[i], norm12list, u12Progress, testDates3); AddLnResults(sportsmen[i], testDates3); }

            await _context.NormativeSportsmen.AddRangeAsync(normResults);
            await _context.LocalNormativeSportsmen.AddRangeAsync(lnResults);
            await _context.SaveChangesAsync();

            // ── Матчи ─────────────────────────────────────────────────────────
            // U14 Иванов
            var u14Players = sportsmen.Take(10).ToList();
            var u16Players = sportsmen.Skip(10).Take(10).ToList();
            var u14TPlayers = sportsmen.Skip(20).Take(8).ToList();
            var u16TPlayers = sportsmen.Skip(28).Take(8).ToList();

            LineupEntry[] Lineup11(List<Sportsman> squad) => new[]
            {
                new LineupEntry { SportsmanId = squad[0].Id, Position = "GK",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[3].Id, Position = "CB",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[5].Id, Position = "RB",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[8].Id, Position = "LB",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[6].Id, Position = "CDM", Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[1].Id, Position = "CM",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[7].Id, Position = "CAM", Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[9].Id, Position = "RW",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[4].Id, Position = "LW",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[2].Id, Position = "ST",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = (squad.Count > 10 ? squad[10] : squad[2]).Id, Position = "ST", Type = PlayerType.Reserve },
            };

            LineupEntry[] Lineup8(List<Sportsman> squad) => new[]
            {
                new LineupEntry { SportsmanId = squad[0].Id, Position = "GK",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[1].Id, Position = "CB",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[5].Id, Position = "RB",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[6].Id, Position = "CDM", Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[2].Id, Position = "CM",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[7].Id, Position = "CAM", Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[4].Id, Position = "LW",  Type = PlayerType.Main },
                new LineupEntry { SportsmanId = squad[3].Id, Position = "ST",  Type = PlayerType.Main },
            };

            var matches = new List<Match>
            {
                // Синие (U14) - 1 (Иванов) — 6 матчей
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК (2012) - 1",     Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-360), TrainerComment = "Уверенная победа 3:0", Lineup = Lineup11(u14Players).ToList() },
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК (2012) - 2",     Type = GameType.Cup,      Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-300), TrainerComment = "Кубковая победа 2:1", Lineup = Lineup11(u14Players).ToList() },
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК (2013)",         Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Draw, Date = DateTime.UtcNow.AddDays(-240), TrainerComment = "Ничья 1:1, не реализовали моменты", Lineup = Lineup11(u14Players).ToList() },
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК-М",             Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Loss, Date = DateTime.UtcNow.AddDays(-180), TrainerComment = "Поражение 0:2, проблемы в обороне", Lineup = Lineup11(u14Players).ToList() },
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК-ЖЕН-1",        Type = GameType.Friendly, Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-90),  TrainerComment = "Товарищеская победа 4:2", Lineup = Lineup11(u14Players).ToList() },
                new Match { HomeTeamId = teamU14Ivanov.Id, OpponentTeamName = "АФК (2012) - 1",   Type = GameType.League,   Status = MatchStatus.Scheduled, Date = DateTime.UtcNow.AddDays(14) },

                // Синие (U16) - 1 (Иванов) — 6 матчей
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК-ЖЕН-2",        Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-350), TrainerComment = "Победа 2:0", Lineup = Lineup11(u16Players).ToList() },
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК (2012) - 2",   Type = GameType.Cup,      Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-280), TrainerComment = "1/4 финала, победа по пенальти", Lineup = Lineup11(u16Players).ToList() },
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК-М",            Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Draw, Date = DateTime.UtcNow.AddDays(-210), Lineup = Lineup11(u16Players).ToList() },
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК (2013)",       Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Loss, Date = DateTime.UtcNow.AddDays(-140), TrainerComment = "Сложный выезд, 1:3", Lineup = Lineup11(u16Players).ToList() },
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК-ЖЕН-1",       Type = GameType.Friendly, Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-60),  TrainerComment = "Товарищеская, 3:1", Lineup = Lineup11(u16Players).ToList() },
                new Match { HomeTeamId = teamU16Ivanov.Id, OpponentTeamName = "АФК (2012) - 1",  Type = GameType.League,   Status = MatchStatus.Scheduled, Date = DateTime.UtcNow.AddDays(21) },

                // Красные (U14) - 1 (Сергеев) — 4 матча
                new Match { HomeTeamId = teamU14Trainer.Id, OpponentTeamName = "АФК (2012) - 2", Type = GameType.Friendly, Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-200), TrainerComment = "Победа 2:0", Lineup = Lineup8(u14TPlayers).ToList() },
                new Match { HomeTeamId = teamU14Trainer.Id, OpponentTeamName = "АФК (2013)",     Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Draw, Date = DateTime.UtcNow.AddDays(-120), Lineup = Lineup8(u14TPlayers).ToList() },
                new Match { HomeTeamId = teamU14Trainer.Id, OpponentTeamName = "АФК-М",          Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Loss, Date = DateTime.UtcNow.AddDays(-50),  Lineup = Lineup8(u14TPlayers).ToList() },
                new Match { HomeTeamId = teamU14Trainer.Id, OpponentTeamName = "АФК-ЖЕН-2",     Type = GameType.Friendly, Status = MatchStatus.Scheduled, Date = DateTime.UtcNow.AddDays(10) },

                // Красные (U16) - 1 (Сергеев) — 4 матча
                new Match { HomeTeamId = teamU16Trainer.Id, OpponentTeamName = "АФК (2013)",     Type = GameType.Friendly, Status = MatchStatus.Finished, Result = MatchResult.Win,  Date = DateTime.UtcNow.AddDays(-180), Lineup = Lineup8(u16TPlayers).ToList() },
                new Match { HomeTeamId = teamU16Trainer.Id, OpponentTeamName = "АФК (2012) - 1", Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Draw, Date = DateTime.UtcNow.AddDays(-100), Lineup = Lineup8(u16TPlayers).ToList() },
                new Match { HomeTeamId = teamU16Trainer.Id, OpponentTeamName = "АФК-ЖЕН-1",     Type = GameType.League,   Status = MatchStatus.Finished, Result = MatchResult.Loss, Date = DateTime.UtcNow.AddDays(-30),  Lineup = Lineup8(u16TPlayers).ToList() },
                new Match { HomeTeamId = teamU16Trainer.Id, OpponentTeamName = "АФК-М",          Type = GameType.Friendly, Status = MatchStatus.Scheduled, Date = DateTime.UtcNow.AddDays(7) },
            };
            await _context.Matches.AddRangeAsync(matches);
            await _context.SaveChangesAsync();

            // ── События матчей ────────────────────────────────────────────────
            var events = new List<MatchEvent>();

            void AddGoals(Match m, List<Sportsman> squad, bool win, bool draw)
            {
                var goalMinutes = win
                    ? new[] { rnd.Next(10, 30), rnd.Next(40, 65), rnd.Next(70, 88) }
                    : draw ? new[] { rnd.Next(20, 50) }
                    : Array.Empty<int>();

                foreach (var min in goalMinutes)
                    events.Add(new MatchEvent { MatchId = m.Id, Type = MatchEventType.Goal, IsHomeTeam = true, Minute = min, SportsmanId = squad[rnd.Next(1, squad.Count)].Id });

                if (!win)
                    events.Add(new MatchEvent { MatchId = m.Id, Type = MatchEventType.Goal, IsHomeTeam = false, Minute = rnd.Next(20, 80) });
                if (m.Result == MatchResult.Loss)
                    events.Add(new MatchEvent { MatchId = m.Id, Type = MatchEventType.Goal, IsHomeTeam = false, Minute = rnd.Next(50, 88) });

                if (rnd.Next(2) == 0)
                    events.Add(new MatchEvent { MatchId = m.Id, Type = MatchEventType.YellowCard, IsHomeTeam = rnd.Next(2) == 0, Minute = rnd.Next(30, 80) });

                // Замены: 2–3 на матч, разные игроки, разные минуты
                var subCount = rnd.Next(2, 4);
                var usedOut = new HashSet<int>();
                var subMinutes = new[] { rnd.Next(46, 60), rnd.Next(61, 75), rnd.Next(76, 88) };
                for (int si = 0; si < subCount; si++)
                {
                    int outIdx = rnd.Next(squad.Count);
                    while (usedOut.Contains(outIdx)) outIdx = rnd.Next(squad.Count);
                    usedOut.Add(outIdx);
                    int inIdx = rnd.Next(squad.Count);
                    while (inIdx == outIdx) inIdx = rnd.Next(squad.Count);
                    events.Add(new MatchEvent { MatchId = m.Id, Type = MatchEventType.Substitution, IsHomeTeam = true, Minute = subMinutes[si], SportsmanId = squad[outIdx].Id, SubstituteSportsmanId = squad[inIdx].Id });
                }
            }

            foreach (var m in matches.Where(m => m.Status == MatchStatus.Finished))
            {
                var squad = m.HomeTeamId == teamU14Ivanov.Id ? u14Players
                    : m.HomeTeamId == teamU16Ivanov.Id ? u16Players
                    : m.HomeTeamId == teamU14Trainer.Id ? u14TPlayers
                    : u16TPlayers;
                AddGoals(m, squad, m.Result == MatchResult.Win, m.Result == MatchResult.Draw);
            }
            await _context.MatchEvents.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // ── Планы тренировок ──────────────────────────────────────────────
            var plans = new List<PlanTraining>
            {
                new PlanTraining { TrainerId = pIvanov.Id,  Name = "Базовая физподготовка U14", Description = "Акцент на выносливость и скоростную работу", CreatedAt = DateTime.UtcNow.AddDays(-180),
                    Workouts = """[{"name":"Разминка","duration":10,"description":"Лёгкий бег + динамическая растяжка"},{"name":"Интервалы 4×200м","duration":20,"description":"Бег 200м × 4 серии, отдых 90 сек"},{"name":"Квадрат с мячом","duration":25,"description":"Квадрат 4v4, акцент на короткий пас"},{"name":"Заминка","duration":10,"description":"Стретчинг"}]""" },
                new PlanTraining { TrainerId = pIvanov.Id,  Name = "Тактика U16 — высокий прессинг", Description = "Отработка высокого давления и контрпрессинга", CreatedAt = DateTime.UtcNow.AddDays(-120),
                    Workouts = """[{"name":"Разминка","duration":10,"description":"Активация + растяжка"},{"name":"Позиционная игра 7v7","duration":30,"description":"Команда держит высокую линию, немедленное давление при потере"},{"name":"Переходы 5v3","duration":20,"description":"Быстрый переход в прессинг после потери мяча"},{"name":"Двусторонняя игра","duration":25,"description":"Применение изученного в матчевом контексте"}]""" },
                new PlanTraining { TrainerId = pTrainer.Id, Name = "Техника дриблинга U14-Б", Description = "Индивидуальная техника с мячом", CreatedAt = DateTime.UtcNow.AddDays(-90),
                    Workouts = """[{"name":"Разминка с мячом","duration":10,"description":"Ведение мяча змейкой"},{"name":"Дриблинг 1v1","duration":20,"description":"Один против одного в коридоре 5×15м"},{"name":"Треугольники","duration":25,"description":"Пас в треугольнике + рывок"},{"name":"Двусторонняя 4v4","duration":20,"description":"Свободная игра с акцентом на обводку"}]""" },
                new PlanTraining { TrainerId = pTrainer.Id, Name = "Силовая U16-Б", Description = "ОФП: прыжки, скорость, мощность", CreatedAt = DateTime.UtcNow.AddDays(-60),
                    Workouts = """[{"name":"Разминка","duration":10,"description":"Пробежка + суставная гимнастика"},{"name":"Прыжки в длину","duration":15,"description":"3 серии × 5 прыжков с места"},{"name":"Спринты 30м","duration":20,"description":"10 спринтов 30м, отдых 60 сек"},{"name":"Планка + приседания","duration":15,"description":"3 круга: 60 сек планки, 20 приседаний"},{"name":"Растяжка","duration":10,"description":"Статический стретчинг"}]""" },
                new PlanTraining { TrainerId = pPetrov.Id,  Name = "Игровое знакомство U12", Description = "Первичное введение в игровые принципы", CreatedAt = DateTime.UtcNow.AddDays(-50),
                    Workouts = """[{"name":"Игры на разминку","duration":15,"description":"Эстафеты и активные игры"},{"name":"Пас в парах","duration":20,"description":"Пас двумя ногами на расстоянии 10м"},{"name":"Мини-футбол 3v3","duration":25,"description":"Игра на маленьких воротах, без офсайда"}]""" },
            };
            await _context.PlanTrainings.AddRangeAsync(plans);
            await _context.SaveChangesAsync();

            // ── Индивидуальные задания (PersonalWorkout) ───────────────────────
            var workouts = new List<PersonalWorkout>
            {
                // Вратарские задания для GK
                new PersonalWorkout { PersonalId = pIvanov.Id,  SportsmanId = sportsmen[0].Id,  CreatedAt = DateTime.UtcNow.AddDays(-30),
                    Workout = """{"title":"Вратарская реакция","exercises":[{"name":"Падения в сторону","sets":4,"reps":8,"description":"Падение с фиксацией мяча"},{"name":"Броски в угол","sets":3,"reps":10,"description":"Тренер бьёт, вратарь отбивает"}],"notes":"Акцент на скорость реакции левой рукой"}""" },
                new PersonalWorkout { PersonalId = pIvanov.Id,  SportsmanId = sportsmen[8].Id,  CreatedAt = DateTime.UtcNow.AddDays(-25),
                    Workout = """{"title":"Скоростная выносливость ЛЗ","exercises":[{"name":"Рывки по флангу","sets":6,"reps":1,"description":"60м спринт вдоль бровки"},{"name":"Кроссовый бег","sets":1,"reps":1,"description":"12 минут в аэробном темпе"}],"notes":"Работа на восстановление дыхания после спринтов"}""" },
                new PersonalWorkout { PersonalId = pIvanov.Id,  SportsmanId = sportsmen[10].Id, CreatedAt = DateTime.UtcNow.AddDays(-20),
                    Workout = """{"title":"Разрушение и первый пас","exercises":[{"name":"Отбор 1v1","sets":5,"reps":5,"description":"Отбор мяча в квадрате 5×5м"},{"name":"Первый пас после отбора","sets":4,"reps":8,"description":"Сразу после отбора — точный длинный пас"}],"notes":"Ключевой навык для опорника — скорость первого паса"}""" },
                new PersonalWorkout { PersonalId = pTrainer.Id, SportsmanId = sportsmen[22].Id, CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Workout = """{"title":"Финт Кройфа","exercises":[{"name":"Имитация без защитника","sets":5,"reps":10,"description":"Финт в обе стороны"},{"name":"Применение 1v1","sets":4,"reps":5,"description":"Против реального соперника"}],"notes":"Тимофей хорошо чувствует момент, отработать исполнение"}""" },
                new PersonalWorkout { PersonalId = pTrainer.Id, SportsmanId = sportsmen[20].Id, CreatedAt = DateTime.UtcNow.AddDays(-10),
                    Workout = """{"title":"Вратарь — выход из ворот","exercises":[{"name":"Выбор позиции","sets":3,"reps":6,"description":"Тренер разыгрывает выход 1v1, вратарь выбирает момент"},{"name":"Паши ногами","sets":3,"reps":10,"description":"Точность раздачи мяча ногой"}],"notes":"Смирнов отлично играет руками, нужно улучшить игру ногами"}""" },
                new PersonalWorkout { PersonalId = pTrainer.Id, SportsmanId = sportsmen[23].Id, CreatedAt = DateTime.UtcNow.AddDays(-8),
                    Workout = """{"title":"Завершение атак","exercises":[{"name":"Удары с передачи","sets":4,"reps":8,"description":"Первый удар после получения паса на ход"},{"name":"Удары в движении","sets":3,"reps":6,"description":"Ведение + удар в угол"}],"notes":"Артём хорошо открывается, нужно дорабатывать технику удара"}""" },
                new PersonalWorkout { PersonalId = pPetrov.Id,  SportsmanId = sportsmen[38].Id, CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Workout = """{"title":"Базовая техника паса","exercises":[{"name":"Пас внутренней стороной стопы","sets":3,"reps":15,"description":"Точный пас на расстоянии 8м"},{"name":"Пас с хода","sets":3,"reps":10,"description":"Пас партнёру в движении"}],"notes":"Захар — хорошая база, ускорить принятие решений"}""" },
            };
            await _context.PersonalWorkouts.AddRangeAsync(workouts);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                hint = "Embeddings: вызови POST /seed/embeddings отдельно",
                data = new
                {
                    users       = await _context.Users.CountAsync(),
                    personal    = await _context.Personal.CountAsync(),
                    sportsmen   = await _context.Sportsmen.CountAsync(),
                    groups      = await _context.Groups.CountAsync(),
                    teams       = await _context.Teams.CountAsync(),
                    trainings   = await _context.Trainings.CountAsync(),
                    metrics     = await _context.TrainingMetrics.CountAsync(),
                    attendances = await _context.Attendances.CountAsync(),
                    matches     = await _context.Matches.CountAsync(),
                    normatives  = await _context.NormativeSportsmen.CountAsync(),
                    plans       = await _context.PlanTrainings.CountAsync(),
                    workouts    = await _context.PersonalWorkouts.CountAsync(),
                }
            });
        }

        // ─── POST /seed/nikita ────────────────────────────────────────────────

        [HttpPost("nikita")]
        public async Task<IActionResult> SeedNikita()
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(s => s.FIO == "Волков Никита Сергеевич");
            if (sportsman == null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "nikitavol@academy.ru");
                if (user == null) return NotFound("Пользователь nikitavol@academy.ru не найден — сначала выполните POST /seed");
                sportsman = await _context.Sportsmen.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (sportsman == null) return NotFound("Спортсмен для nikitavol@academy.ru не найден");
            }

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "U14-А");
            if (group == null) return NotFound("Группа U14-А не найдена — сначала выполните POST /seed");

            var trainer = await _context.Personal.FirstOrDefaultAsync(p => p.Id == group.TrainerId);
            if (trainer == null) return NotFound("Тренер группы не найден");

            var rnd = new Random(42);

            var trainingTypes = new[] { "Общая", "Физическая", "Тактическая", "Техническая", "Силовая", "Вратарская", "Игровая", "Восстановительная" };

            // 30 старых тренировок (6–14 месяцев назад) + 10 свежих (последние 2 месяца)
            var oldDates = Enumerable.Range(0, 30)
                .Select(i => DateTime.UtcNow.AddDays(-(420 - i * 10)))  // от 14 до 6 месяцев назад
                .ToList();
            var recentDates = Enumerable.Range(0, 10)
                .Select(i => DateTime.UtcNow.AddDays(-(60 - i * 6)))    // последние 2 месяца
                .ToList();
            var trainingDates = oldDates.Concat(recentDates).ToList();

            var newTrainings = trainingDates.Select((d, i) => new Training
            {
                TrainerId = trainer.Id,
                GroupId   = group.Id,
                Type      = trainingTypes[i % trainingTypes.Length],
                Date      = d
            }).ToList();
            await _context.Trainings.AddRangeAsync(newTrainings);
            await _context.SaveChangesAsync();

            var metrics = newTrainings.Select(t => MakeWeakMetrics(rnd, t.Id, sportsman.Id, sportsman.Position, t.Date, sportsman.Age)).ToList();
            await _context.TrainingMetrics.AddRangeAsync(metrics);

            // Плохая посещаемость: каждая 3-я — прогул, каждая 5-я — опоздание
            var attendances = newTrainings.Select((t, i) => new Attendance
            {
                SportsmanId = sportsman.Id,
                TrainingId  = t.Id,
                Status      = i % 3 == 0 ? AttendanceStatus.Absent : i % 5 == 0 ? AttendanceStatus.Late : AttendanceStatus.Present
            }).ToList();
            await _context.Attendances.AddRangeAsync(attendances);

            // Нормативы для Никиты (если ещё нет norm14)
            var norm14Run60 = await _context.Normatives.FirstOrDefaultAsync(n => n.AgeGroup == 14 && n.Type == "Бег 60м");
            if (norm14Run60 != null)
            {
                var existingNorm = await _context.NormativeSportsmen.AnyAsync(n => n.SportsmanId == sportsman.Id);
                if (!existingNorm)
                {
                    var norms14 = await _context.Normatives.Where(n => n.AgeGroup == 14).ToListAsync();
                    double[][] prog = {
                        new[] { 9.3, 9.0, 8.8, 8.6 },
                        new[] { 155, 161, 167, 174.0 },
                        new[] { 4, 6, 7, 8.0 },
                        new[] { 10.8, 10.3, 10.0, 9.6 },
                    };
                    var dates = new[] { DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(-1) };
                    for (int di = 0; di < dates.Length; di++)
                        for (int ni = 0; ni < norms14.Count && ni < 4; ni++)
                            _context.NormativeSportsmen.Add(new NormativeSportsman
                            {
                                SportsmanId = sportsman.Id,
                                NormativeId = norms14[ni].Id,
                                Result      = Math.Round(prog[ni][di] + (rnd.NextDouble() - 0.5) * 0.2, 2),
                                CreatedAt   = dates[di]
                            });
                }
            }

            await _context.SaveChangesAsync();

            await _rag.UpsertSportsmanEmbeddingAsync(sportsman.Id);

            return Ok(new { success = true, data = new { trainings = newTrainings.Count, metrics = metrics.Count, attendances = attendances.Count } });
        }

        // ─── DELETE /seed/clear ───────────────────────────────────────────────

        [HttpPost("embeddings")]
        public async Task<IActionResult> SeedEmbeddings()
        {
            var result = await _rag.RebuildAllEmbeddingsAsync();
            return result.IsSuccess
                ? Ok(new { success = true, data = new { embeddings = result.Data } })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        [HttpDelete("embeddings")]
        public async Task<IActionResult> ClearEmbeddings()
        {
            _context.PlayerEmbeddings.RemoveRange(_context.PlayerEmbeddings);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = "Embeddings очищены" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
        {
            _context.TrainingMetrics.RemoveRange(_context.TrainingMetrics);
            _context.MatchEvents.RemoveRange(_context.MatchEvents);
            _context.Matches.RemoveRange(_context.Matches);
            _context.Attendances.RemoveRange(_context.Attendances);
            _context.NormativeSportsmen.RemoveRange(_context.NormativeSportsmen);
            _context.LocalNormativeSportsmen.RemoveRange(_context.LocalNormativeSportsmen);
            _context.Normatives.RemoveRange(_context.Normatives);
            _context.LocalNormatives.RemoveRange(_context.LocalNormatives);
            _context.PersonalWorkouts.RemoveRange(_context.PersonalWorkouts);
            _context.SportsmanGroups.RemoveRange(_context.SportsmanGroups);
            _context.Classes.RemoveRange(_context.Classes);
            _context.Trainings.RemoveRange(_context.Trainings);
            _context.PlanTrainings.RemoveRange(_context.PlanTrainings);
            _context.Images.RemoveRange(_context.Images);
            _context.Sportsmen.RemoveRange(_context.Sportsmen);
            _context.Groups.RemoveRange(_context.Groups);
            _context.Teams.RemoveRange(_context.Teams);
            _context.Personal.RemoveRange(_context.Personal);
            _context.Messages.RemoveRange(_context.Messages);
            _context.Broadcasts.RemoveRange(_context.Broadcasts);
            _context.AiMessages.RemoveRange(_context.AiMessages);
            _context.AiChats.RemoveRange(_context.AiChats);
            _context.PlayerEmbeddings.RemoveRange(_context.PlayerEmbeddings);
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = "БД очищена" });
        }
    }
}
