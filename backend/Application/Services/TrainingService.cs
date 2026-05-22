using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.TrainingModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TrainingService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<TrainingResponse>> GetTrainingAsync(long trainingId)
        {
            var training = await _context.Trainings
                .Include(t => t.Group)
                .Include(t => t.PlanTraining)
                .Include(t => t.Trainer)
                .Include(t => t.Metrics)
                .FirstOrDefaultAsync(x => x.Id == trainingId);
            if (training == null)
            {
                return Result<TrainingResponse>.Failure("Тренировка не найдена", 404);
            }

            var trainingRes = _mapper.Map<TrainingResponse>(training);
            return Result<TrainingResponse>.Success(trainingRes);
        }

        public async Task<Result<List<TrainingResponse>>> GetTrainingsAsync(Filter? filter)
        {
            var trainings = await _context.Trainings
                .Include(t => t.Group)
                .Include(t => t.PlanTraining)
                .Include(t => t.Trainer)
                .Include(t => t.Metrics)
                .ApplyFilter(filter)
                .ToListAsync();

            var trainingsRes = _mapper.Map<List<TrainingResponse>>(trainings);
            return Result<List<TrainingResponse>>.Success(trainingsRes);
        }

        public async Task<Result<List<TrainingResponse>>> GetTrainingsForSportsmanAsync(long sportsmanId, Filter? filter)
        {
            var trainings = await _context.TrainingMetrics
                .Include(t => t.Training)
                .Where(t => t.SportsmanId == sportsmanId)
                .Select(t => t.Training)
                .Distinct()
                .ApplyFilter(filter)
                .ToListAsync();

            var trainingsRes = _mapper.Map<List<TrainingResponse>>(trainings);
            return Result<List<TrainingResponse>>.Success(trainingsRes);
        }

        public async Task<Result<TrainingResponse>> CreateTrainingAsync(TrainingCreateRequest req)
        {
            var newTraining = _mapper.Map<Training>(req);
            newTraining.CreatedAt = DateTime.UtcNow;
            await _context.Trainings.AddAsync(newTraining);
            await _context.SaveChangesAsync();

            var trainingRes = _mapper.Map<TrainingResponse>(newTraining);
            return Result<TrainingResponse>.Success(trainingRes);
        }

        public async Task<Result<TrainingResponse>> UpdateTrainingAsync(TrainingUpdateRequest req, long trainingId)
        {
            var trainingExist = await _context.Trainings.FirstOrDefaultAsync(x => x.Id == trainingId);
            if (trainingExist == null)
            {
                return Result<TrainingResponse>.Failure("Тренировка не найдена", 404);
            }

            var newTraining = _mapper.Map(req, trainingExist);
            _context.Trainings.Update(newTraining);
            await _context.SaveChangesAsync();

            var trainingRes = _mapper.Map<TrainingResponse>(newTraining);
            return Result<TrainingResponse>.Success(trainingRes);
        }

        public async Task<Result<bool>> DeleteTrainingAsync(long trainingId)
        {
            var trainingExist = await _context.Trainings.FirstOrDefaultAsync(x => x.Id == trainingId);
            if (trainingExist == null)
                return Result<bool>.Failure("Тренировка не найдена", 404);

            _context.TrainingMetrics.RemoveRange(_context.TrainingMetrics.Where(x => x.TrainingId == trainingId));
            _context.Attendances.RemoveRange(_context.Attendances.Where(x => x.TrainingId == trainingId));

            // Связь Training.MatchId → Match: при удалении Training FK просто исчезает,
            // Match остаётся жить. Обнулять ничего не нужно.
            _context.Trainings.Remove(trainingExist);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
