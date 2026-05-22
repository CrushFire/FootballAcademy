using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models.AttendanceModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class SchedualeService : ISchedualeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SchedualeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<AttendanceResponse>>> MarkAttendanceAsync(long trainingId, List<AttendanceCreateRequest> req)
        {
            var trainingExists = await _context.Trainings.AnyAsync(x => x.Id == trainingId);
            if (!trainingExists)
                return Result<List<AttendanceResponse>>.Failure("Тренировка не найдена", 404);

            var sportsmanIds = req.Select(x => x.SportsmanId).ToList();

            var existingAttendances = await _context.Attendances
                .Where(x => x.TrainingId == trainingId && sportsmanIds.Contains(x.SportsmanId))
                .ToListAsync();

            _context.Attendances.RemoveRange(existingAttendances);

            var attendances = req.Select(x =>
            {
                var attendance = _mapper.Map<Attendance>(x);
                attendance.TrainingId = trainingId;
                return attendance;
            }).ToList();

            await _context.Attendances.AddRangeAsync(attendances);
            await _context.SaveChangesAsync();

            return Result<List<AttendanceResponse>>.Success(_mapper.Map<List<AttendanceResponse>>(attendances));
        }

        public async Task<Result<List<AttendanceResponse>>> GetAttendanceAsync(long trainingId)
        {
            var trainingExists = await _context.Trainings.AnyAsync(x => x.Id == trainingId);
            if (!trainingExists)
                return Result<List<AttendanceResponse>>.Failure("Тренировка не найдена", 404);

            var attendances = await _context.Attendances
                .Where(x => x.TrainingId == trainingId)
                .ToListAsync();

            return Result<List<AttendanceResponse>>.Success(_mapper.Map<List<AttendanceResponse>>(attendances));
        }
        public async Task<Result<List<AttendanceResponse>>> GetAttendanceBySportsmanAsync(long sportsmanId)
        {
            var attendances = await _context.Attendances
                .Where(x => x.SportsmanId == sportsmanId)
                .Include(x => x.Training)
                .OrderByDescending(x => x.Training.Date)
                .ToListAsync();

            var result = attendances.Select(a => new AttendanceResponse
            {
                Id = a.Id,
                TrainingId = a.TrainingId,
                SportsmanId = a.SportsmanId,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                TrainingDate = a.Training?.Date ?? a.CreatedAt,
                TrainingType = a.Training?.Type,
            }).ToList();

            return Result<List<AttendanceResponse>>.Success(result);
        }
    }
}
