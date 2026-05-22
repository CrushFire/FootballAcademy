using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Enums.Match;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.TeamModel;
using Core.Results;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;

        public TeamService(ApplicationDbContext context, IMapper mapper, ImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<Result<TeamResponse>> GetTeamAsync(long teamId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
            if (team == null)
                return Result<TeamResponse>.Failure("Команда не найдена", 404);

            return Result<TeamResponse>.Success(_mapper.Map<TeamResponse>(team));
        }

        public async Task<Result<List<TeamResponse>>> GetTeamsAsync(Filter? filter)
        {
            var teams = await _context.Teams
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<TeamResponse>>.Success(_mapper.Map<List<TeamResponse>>(teams));
        }

        public async Task<Result<TeamResponse>> CreateTeamAsync(TeamCreateRequest req)
        {
            var trainerExists = await _context.Personal.AnyAsync(x => x.Id == req.TrainerId);
            if (!trainerExists)
                return Result<TeamResponse>.Failure("Тренер не найден", 404);

            var duplicate = await _context.Teams.AnyAsync(x => x.Name == req.Name);
            if (duplicate)
                return Result<TeamResponse>.Failure($"Команда с названием '{req.Name}' уже существует", 409);

            var team = _mapper.Map<Team>(req);
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            if (req.Images?.Count > 0)
            {
                var imagesRes = await _imageService.SaveImagesAsync(req.Images, "team", team.Id);
                if (!imagesRes.IsSuccess)
                    return Result<TeamResponse>.Failure($"Ошибка при сохранении изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);

                foreach (var path in imagesRes.Data!)
                    await _context.Images.AddAsync(new Image { TeamId = team.Id, Path = path });

                await _context.SaveChangesAsync();
            }

            return Result<TeamResponse>.Success(_mapper.Map<TeamResponse>(team));
        }

        public async Task<Result<TeamResponse>> UpdateTeamAsync(TeamUpdateRequest req, long teamId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
            if (team == null)
                return Result<TeamResponse>.Failure("Команда не найдена", 404);

            var duplicate = await _context.Teams.AnyAsync(x => x.Name == req.Name && x.Id != teamId);
            if (duplicate)
                return Result<TeamResponse>.Failure($"Команда с названием '{req.Name}' уже существует", 409);

            if (req.TrainerId.HasValue)
            {
                var trainerExists = await _context.Personal.AnyAsync(x => x.Id == req.TrainerId.Value);
                if (!trainerExists)
                    return Result<TeamResponse>.Failure("Тренер не найден", 404);
                team.TrainerId = req.TrainerId.Value;
            }

            team.Name = req.Name;
            team.AgeGroup = req.AgeGroup;
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return Result<TeamResponse>.Success(_mapper.Map<TeamResponse>(team));
        }

        public async Task<Result<bool>> DeleteTeamAsync(long teamId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
            if (team == null)
                return Result<bool>.Failure("Команда не найдена", 404);

            // Открепить спортсменов от команды
            var sportsmen = await _context.Sportsmen.Where(x => x.TeamId == teamId).ToListAsync();
            foreach (var s in sportsmen)
                s.TeamId = null;
            if (sportsmen.Count > 0)
                await _context.SaveChangesAsync();

            var images = await _context.Images.Where(x => x.TeamId == teamId).ToListAsync();
            if (images.Count > 0)
            {
                var deleteRes = await _imageService.DeleteImagesAsync(images);
                if (!deleteRes.IsSuccess)
                    return Result<bool>.Failure($"Ошибка при удалении изображений: {deleteRes.ErrorMessage}", deleteRes.StatusCode);

                _context.Images.RemoveRange(images);
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<string>>> AddTeamImagesAsync(long teamId, List<IFormFile> images)
        {
            var teamExists = await _context.Teams.AnyAsync(x => x.Id == teamId);
            if (!teamExists)
                return Result<List<string>>.Failure("Команда не найдена", 404);

            var imagesRes = await _imageService.SaveImagesAsync(images, "team", teamId);
            if (!imagesRes.IsSuccess)
                return Result<List<string>>.Failure($"Ошибка при сохранении изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);

            foreach (var path in imagesRes.Data!)
                await _context.Images.AddAsync(new Image { TeamId = teamId, Path = path });

            await _context.SaveChangesAsync();
            return Result<List<string>>.Success(imagesRes.Data);
        }

        public async Task<Result<bool>> DeleteTeamImageAsync(long imageId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(x => x.Id == imageId);
            if (image == null)
                return Result<bool>.Failure("Изображение не найдено", 404);

            var deleteRes = await _imageService.DeleteImageAsync(image.Path);
            if (!deleteRes.IsSuccess)
                return Result<bool>.Failure($"Ошибка при удалении изображения: {deleteRes.ErrorMessage}", deleteRes.StatusCode);

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddSportsmanToTeamAsync(long teamId, long sportsmanId)
        {
            var teamExists = await _context.Teams.AnyAsync(x => x.Id == teamId);
            if (!teamExists)
                return Result<bool>.Failure("Команда не найдена", 404);

            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<bool>.Failure("Спортсмен не найден", 404);

            if (sportsman.TeamId == teamId)
                return Result<bool>.Failure("Спортсмен уже состоит в этой команде", 409);

            sportsman.TeamId = teamId;
            _context.Sportsmen.Update(sportsman);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RemoveSportsmanFromTeamAsync(long sportsmanId)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == sportsmanId);
            if (sportsman == null)
                return Result<bool>.Failure("Спортсмен не найден", 404);

            if (sportsman.TeamId == null)
                return Result<bool>.Failure("Спортсмен не состоит ни в одной команде", 400);

            sportsman.TeamId = null;
            _context.Sportsmen.Update(sportsman);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<TeamStatsResponse>>> GetTeamsStatsAsync(Filter? filter)
        {
            var teams = await _context.Teams
                .Include(x => x.Matches)
                .ApplyFilter(filter)
                .ToListAsync();

            var stats = teams
                .Select(t => new TeamStatsResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    AgeGroup = t.AgeGroup,
                    Wins = t.Matches.Count(m => m.Result == MatchResult.Win),
                    Draws = t.Matches.Count(m => m.Result == MatchResult.Draw),
                    Losses = t.Matches.Count(m => m.Result == MatchResult.Loss),
                    TotalMatches = t.Matches.Count
                })
                .ApplyStatSort(filter?.Filters)
                .ToList();

            return Result<List<TeamStatsResponse>>.Success(stats);
        }
    }
}
