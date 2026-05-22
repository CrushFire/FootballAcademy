using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.GroupModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GroupService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<GroupResponse>> GetGroupAsync(long groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                return Result<GroupResponse>.Failure("Группа не найдена", 404);

            return Result<GroupResponse>.Success(_mapper.Map<GroupResponse>(group));
        }

        public async Task<Result<List<GroupResponse>>> GetGroupsAsync(Filter? filter)
        {
            var groups = await _context.Groups
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<GroupResponse>>.Success(_mapper.Map<List<GroupResponse>>(groups));
        }

        public async Task<Result<List<GroupResponse>>> GetGroupsForSportsmanAsync(long sportsmanId)
        {
            var groups = await _context.SportsmanGroups
                .Where(sg => sg.SportsmanId == sportsmanId)
                .Include(sg => sg.Group)
                .Select(sg => sg.Group)
                .ToListAsync();

            return Result<List<GroupResponse>>.Success(_mapper.Map<List<GroupResponse>>(groups));
        }

        public async Task<Result<GroupResponse>> CreateGroupAsync(GroupCreateRequest req)
        {
            var trainerExists = await _context.Personal.AnyAsync(x => x.Id == req.TrainerId);
            if (!trainerExists)
                return Result<GroupResponse>.Failure("Тренер не найден", 404);

            var duplicate = await _context.Groups.AnyAsync(x => x.Name == req.Name);
            if (duplicate)
                return Result<GroupResponse>.Failure($"Группа с названием '{req.Name}' уже существует", 409);

            var group = _mapper.Map<Group>(req);
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            return Result<GroupResponse>.Success(_mapper.Map<GroupResponse>(group));
        }

        public async Task<Result<GroupResponse>> UpdateGroupAsync(GroupUpdateRequest req, long groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                return Result<GroupResponse>.Failure("Группа не найдена", 404);

            var duplicate = await _context.Groups.AnyAsync(x => x.Name == req.Name && x.Id != groupId);
            if (duplicate)
                return Result<GroupResponse>.Failure($"Группа с названием '{req.Name}' уже существует", 409);

            if (req.TrainerId.HasValue)
            {
                var trainerExists = await _context.Personal.AnyAsync(x => x.Id == req.TrainerId.Value);
                if (!trainerExists)
                    return Result<GroupResponse>.Failure("Тренер не найден", 404);
                group.TrainerId = req.TrainerId.Value;
            }

            group.Name = req.Name;
            group.Description = req.Description;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            return Result<GroupResponse>.Success(_mapper.Map<GroupResponse>(group));
        }

        public async Task<Result<bool>> DeleteGroupAsync(long groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                return Result<bool>.Failure("Группа не найдена", 404);

            // Удалить связи спортсменов с группой
            _context.SportsmanGroups.RemoveRange(_context.SportsmanGroups.Where(x => x.GroupId == groupId));
            // Удалить расписание группы
            _context.Classes.RemoveRange(_context.Classes.Where(x => x.GroupId == groupId));

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddSportsmanToGroupAsync(long groupId, long sportsmanId)
        {
            var groupExists = await _context.Groups.AnyAsync(x => x.Id == groupId);
            if (!groupExists)
                return Result<bool>.Failure("Группа не найдена", 404);

            var sportsmanExists = await _context.Sportsmen.AnyAsync(x => x.Id == sportsmanId);
            if (!sportsmanExists)
                return Result<bool>.Failure("Спортсмен не найден", 404);

            var alreadyIn = await _context.SportsmanGroups
                .AnyAsync(x => x.GroupId == groupId && x.SportsmanId == sportsmanId);
            if (alreadyIn)
                return Result<bool>.Success(true);

            await _context.SportsmanGroups.AddAsync(new SportsmanGroup
            {
                GroupId = groupId,
                SportsmanId = sportsmanId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RemoveSportsmanFromGroupAsync(long groupId, long sportsmanId)
        {
            var link = await _context.SportsmanGroups
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.SportsmanId == sportsmanId);
            if (link == null)
                return Result<bool>.Failure("Связь не найдена", 404);

            _context.SportsmanGroups.Remove(link);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
