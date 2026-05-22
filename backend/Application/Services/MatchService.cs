using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Enums.Match;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MatchModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MatchService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<MatchResponse>> GetMatchAsync(long matchId)
        {
            var match = await _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .Include(x => x.Trainings)
                .FirstOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return Result<MatchResponse>.Failure("Матч не найден", 404);

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        public async Task<Result<List<MatchResponse>>> GetMatchesAsync(Filter? filter)
        {
            var query = _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .Include(x => x.Trainings)
                .AsQueryable();

            // Фильтрация по тренеру через HomeTeam.TrainerId ИЛИ OpponentTeam.TrainerId
            // (если матч между нашими командами — оба тренера должны видеть один и тот же матч)
            var trainerIds = filter?.Filters?.TrainerId;
            if (trainerIds != null && trainerIds.Any())
                query = query.Where(m =>
                    trainerIds.Contains(m.HomeTeam.TrainerId) ||
                    (m.OpponentTeam != null && trainerIds.Contains(m.OpponentTeam.TrainerId)));

            var matches = await query.ApplyFilter(filter).ToListAsync();

            return Result<List<MatchResponse>>.Success(matches.Select(MapToResponse).ToList());
        }

        public async Task<Result<MatchResponse>> CreateMatchAsync(MatchCreateRequest req)
        {
            // Валидация: ровно один из двух вариантов соперника
            if (req.OpponentTeamId.HasValue && !string.IsNullOrEmpty(req.OpponentTeamName))
                return Result<MatchResponse>.Failure("Укажите либо команду соперника из системы, либо её название — не оба варианта", 400);

            if (!req.OpponentTeamId.HasValue && string.IsNullOrEmpty(req.OpponentTeamName))
                return Result<MatchResponse>.Failure("Необходимо указать соперника", 400);

            var homeTeamExists = await _context.Teams.AnyAsync(x => x.Id == req.HomeTeamId);
            if (!homeTeamExists)
                return Result<MatchResponse>.Failure("Команда не найдена", 404);

            if (req.OpponentTeamId.HasValue)
            {
                var opponentExists = await _context.Teams.AnyAsync(x => x.Id == req.OpponentTeamId.Value);
                if (!opponentExists)
                    return Result<MatchResponse>.Failure("Команда соперника не найдена", 404);
            }

            // Собираем уникальные группы (из HomeGroupId и OpponentGroupId)
            // для последующего автосоздания Training (Type="Матч") на каждую.
            var groupIds = new List<long>();
            if (req.HomeGroupId.HasValue) groupIds.Add(req.HomeGroupId.Value);
            if (req.OpponentGroupId.HasValue && req.OpponentGroupId.Value != req.HomeGroupId)
                groupIds.Add(req.OpponentGroupId.Value);

            // Валидация: проверяем что все указанные группы существуют
            if (groupIds.Count > 0)
            {
                var existingGroups = await _context.Groups
                    .Where(g => groupIds.Contains(g.Id))
                    .ToListAsync();
                if (existingGroups.Count != groupIds.Count)
                    return Result<MatchResponse>.Failure("Одна из указанных групп не найдена", 404);
            }

            var match = new Match
            {
                HomeTeamId = req.HomeTeamId,
                OpponentTeamId = req.OpponentTeamId,
                OpponentTeamName = req.OpponentTeamName,
                Type = req.Type,
                Date = req.Date,
                Status = MatchStatus.Scheduled,
                Result = null,
                Events = new List<MatchEvent>()
            };

            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            // После создания матча — создаём по Training на каждую указанную группу
            if (groupIds.Count > 0)
            {
                var groupsToLink = await _context.Groups
                    .Where(g => groupIds.Contains(g.Id))
                    .ToListAsync();
                foreach (var g in groupsToLink)
                {
                    var newTraining = new Core.Entities.Training
                    {
                        TrainerId = g.TrainerId,
                        GroupId = g.Id,
                        MatchId = match.Id,
                        Type = "Матч",
                        Date = req.Date,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _context.Trainings.AddAsync(newTraining);
                }
                await _context.SaveChangesAsync();
            }

            await _context.Entry(match).Reference(x => x.HomeTeam).LoadAsync();
            if (match.OpponentTeamId.HasValue)
                await _context.Entry(match).Reference(x => x.OpponentTeam).LoadAsync();

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        public async Task<Result<MatchResponse>> UpdateMatchAsync(long matchId, MatchUpdateRequest req)
        {
            var match = await _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return Result<MatchResponse>.Failure("Матч не найден", 404);

            match.OpponentTeamName = req.OpponentTeamName;
            match.Type = req.Type;
            match.Date = req.Date;
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        public async Task<Result<bool>> DeleteMatchAsync(long matchId)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == matchId);
            if (match == null)
                return Result<bool>.Failure("Матч не найден", 404);

            // Каскадно удаляем все автосозданные Training (Type="Матч") с MatchId == matchId
            // вместе с их метриками и посещаемостью. Обычные тренировки которые тренер привязал
            // вручную (Type != "Матч") не трогаем — у них MatchId станет NULL автоматически.
            var autoTrainings = await _context.Trainings
                .Where(t => t.MatchId == matchId && t.Type == "Матч")
                .ToListAsync();

            if (autoTrainings.Count > 0)
            {
                var autoIds = autoTrainings.Select(t => t.Id).ToList();
                _context.TrainingMetrics.RemoveRange(
                    _context.TrainingMetrics.Where(m => autoIds.Contains(m.TrainingId)));
                _context.Attendances.RemoveRange(
                    _context.Attendances.Where(a => autoIds.Contains(a.TrainingId)));
                _context.Trainings.RemoveRange(autoTrainings);
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<MatchResponse>> StartMatchAsync(long matchId)
        {
            var match = await _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return Result<MatchResponse>.Failure("Матч не найден", 404);

            if (match.Status != MatchStatus.Scheduled)
                return Result<MatchResponse>.Failure("Матч уже начат или завершён", 400);

            match.Status = MatchStatus.InProgress;
            match.StartedAt = DateTime.UtcNow;
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        public async Task<Result<MatchResponse>> FinishMatchAsync(long matchId, MatchFinishRequest req)
        {
            var match = await _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return Result<MatchResponse>.Failure("Матч не найден", 404);

            if (match.Status == MatchStatus.Finished)
                return Result<MatchResponse>.Failure("Матч уже завершён", 400);

            match.Status = MatchStatus.Finished;
            match.Result = req.Result;
            match.TrainerComment = req.TrainerComment;
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        public async Task<Result<MatchEventResponse>> AddEventAsync(long matchId, MatchEventCreateRequest req)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == matchId);
            if (match == null)
                return Result<MatchEventResponse>.Failure("Матч не найден", 404);

            if (match.Status == MatchStatus.Finished)
                return Result<MatchEventResponse>.Failure("Нельзя добавлять события в завершённый матч", 400);

            if (req.Minute < 0 || req.Minute > 130)
                return Result<MatchEventResponse>.Failure("Минута должна быть в диапазоне 0–130", 400);

            var ev = new MatchEvent
            {
                MatchId = matchId,
                Type = req.Type,
                IsHomeTeam = req.IsHomeTeam,
                Minute = req.Minute,
                Comment = req.Comment,
                SportsmanId = req.SportsmanId,
                SubstituteSportsmanId = req.SubstituteSportsmanId
            };

            await _context.MatchEvents.AddAsync(ev);
            await _context.SaveChangesAsync();

            return Result<MatchEventResponse>.Success(_mapper.Map<MatchEventResponse>(ev));
        }

        public async Task<Result<bool>> DeleteEventAsync(long eventId)
        {
            var ev = await _context.MatchEvents
                .Include(x => x.Match)
                .FirstOrDefaultAsync(x => x.Id == eventId);

            if (ev == null)
                return Result<bool>.Failure("Событие не найдено", 404);

            if (ev.Match.Status == MatchStatus.Finished)
                return Result<bool>.Failure("Нельзя удалять события завершённого матча", 400);

            _context.MatchEvents.Remove(ev);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<MatchEventResponse>>> GetEventsAsync(long matchId)
        {
            var matchExists = await _context.Matches.AnyAsync(x => x.Id == matchId);
            if (!matchExists)
                return Result<List<MatchEventResponse>>.Failure("Матч не найден", 404);

            var events = await _context.MatchEvents
                .Where(x => x.MatchId == matchId)
                .OrderBy(x => x.Minute)
                .ToListAsync();

            return Result<List<MatchEventResponse>>.Success(_mapper.Map<List<MatchEventResponse>>(events));
        }

        public async Task<Result<MatchResponse>> UpdateLineupAsync(long matchId, List<LineupEntryDto> lineup)
        {
            var match = await _context.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.OpponentTeam)
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return Result<MatchResponse>.Failure("Матч не найден", 404);

            // Сортировка: сначала Main, потом Reserve
            var sortedLineup = lineup
                .OrderBy(l => l.Type == Core.Enums.Match.PlayerType.Main ? 0 : 1)
                .ThenBy(l => l.Position)
                .ToList();

            match.Lineup = sortedLineup.Select(l => new LineupEntry
            {
                SportsmanId = l.SportsmanId,
                Position = l.Position,
                Type = l.Type
            }).ToList();

            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return Result<MatchResponse>.Success(MapToResponse(match));
        }

        // Статистика вынесена из маппера в сервис
        public static MatchStatsDto CalculateStats(List<MatchEvent> events, bool isHome) => new()
        {
            Goals       = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.Goal),
            YellowCards = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.YellowCard),
            RedCards    = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.RedCard),
            Corners     = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.Corner),
            Fouls       = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.Foul),
            Penalties   = events.Count(e => e.IsHomeTeam == isHome && e.Type == MatchEventType.Penalty),
        };

        private MatchResponse MapToResponse(Match match)
        {
            var events = match.Events ?? new List<MatchEvent>();
            var response = _mapper.Map<MatchResponse>(match);
            response.HomeStats = CalculateStats(events, isHome: true);
            response.AwayStats = CalculateStats(events, isHome: false);
            return response;
        }
    }
}
