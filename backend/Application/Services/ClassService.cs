using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.ClassModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ClassService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ClassResponse>> GetClassAsync(long classId)
        {
            var cls = await _context.Classes.FirstOrDefaultAsync(x => x.Id == classId);
            if (cls == null)
                return Result<ClassResponse>.Failure("Занятие не найдено", 404);

            return Result<ClassResponse>.Success(MapWeekDate(cls));
        }

        public async Task<Result<List<ClassResponse>>> GetClassesAsync(Filter? filter)
        {
            var query = _context.Classes.Include(x => x.Group).AsQueryable();

            var trainerIds = filter?.Filters?.TrainerId;
            if (trainerIds != null && trainerIds.Any())
                query = query.Where(c => trainerIds.Contains(c.Group.TrainerId));

            var classes = await query.ApplyFilter(filter).ToListAsync();
            return Result<List<ClassResponse>>.Success(classes.Select(MapWeekDate).ToList());
        }

        public async Task<Result<ClassResponse>> CreateClassAsync(ClassCreateRequest req)
        {
            var groupExists = await _context.Groups.AnyAsync(x => x.Id == req.GroupId);

            if (!groupExists)
                return Result<ClassResponse>.Failure("Группа не найдена", 404);

            var duplicate = await _context.Classes.AnyAsync(x =>
                x.GroupId == req.GroupId &&
                x.DayOfWeek == req.DayOfWeek &&
                x.BeginTime == req.BeginTime);

            if (duplicate)
                return Result<ClassResponse>.Failure("Занятие с данными параметрами (группа, день, время) уже существует", 409);

            var newClass = _mapper.Map<Class>(req);
            await _context.Classes.AddAsync(newClass);
            await _context.SaveChangesAsync();

            return Result<ClassResponse>.Success(MapWeekDate(newClass));
        }

        public async Task<Result<ClassResponse>> UpdateClassAsync(ClassUpdateRequest req, long classId)
        {
            var cls = await _context.Classes.FirstOrDefaultAsync(x => x.Id == classId);
            if (cls == null)
                return Result<ClassResponse>.Failure("Занятие не найдено", 404);

            var duplicate = await _context.Classes.AnyAsync(x =>
                x.Id != classId &&
                x.GroupId == cls.GroupId &&
                x.DayOfWeek == req.DayOfWeek &&
                x.BeginTime == req.BeginTime);

            if (duplicate)
                return Result<ClassResponse>.Failure("Занятие с данными параметрами (группа, день, время) уже существует", 409);

            var newCls = _mapper.Map(req, cls);
            _context.Classes.Update(cls);
            await _context.SaveChangesAsync();

            return Result<ClassResponse>.Success(MapWeekDate(cls));
        }

        public async Task<Result<bool>> DeleteClassAsync(long classId)
        {
            var cls = await _context.Classes.FirstOrDefaultAsync(x => x.Id == classId);
            if (cls == null)
                return Result<bool>.Failure("Занятие не найдено", 404);

            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        // вычисляет дату занятия на текущей неделе (пн-вс)
        private static DateTime GetWeekDate(DayOfWeek dayOfWeek, TimeOnly beginTime)
        {
            var numDays = (((int)DateTime.Today.DayOfWeek + 6) % 7);
            var monday = DateTime.Today.AddDays(-numDays);
            return monday.AddDays(numDays).Add(beginTime.ToTimeSpan());
        }

        private ClassResponse MapWeekDate(Class cls)
        {
            var response = _mapper.Map<ClassResponse>(cls);
            response.WeekDate = GetWeekDate(cls.DayOfWeek, cls.BeginTime);
            return response;
        }
    }
}
