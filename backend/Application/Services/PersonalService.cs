using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.PersonalModel;
using Core.Models.PersonalWorkoutModel;
using Core.Models.PlanTrainingModel;
using Core.Results;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class PersonalService : IPersonalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;

        public PersonalService(ApplicationDbContext context, IMapper mapper, ImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<Result<PersonalResponse>> GetPersonalAsync(long id)
        {
            var personal = await _context.Personal
                .Include(p => p.Images)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (personal == null)
                return Result<PersonalResponse>.Failure("Персонал не найден", 404);

            return Result<PersonalResponse>.Success(_mapper.Map<PersonalResponse>(personal));
        }

        public async Task<Result<PersonalResponse>> GetPersonalByUserIdAsync(long userId)
        {
            var personal = await _context.Personal
                .Include(p => p.Images)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (personal == null)
                return Result<PersonalResponse>.Failure("Персонал не найден", 404);

            return Result<PersonalResponse>.Success(_mapper.Map<PersonalResponse>(personal));
        }

        public async Task<Result<List<PersonalResponse>>> GetPersonalsAsync(Filter? filter)
        {
            var personals = await _context.Personal
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PersonalResponse>>.Success(_mapper.Map<List<PersonalResponse>>(personals));
        }

        public async Task<Result<PersonalResponse>> CreatePersonalAsync(PersonalCreateRequest req)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == req.UserId);
            if (!userExists)
                return Result<PersonalResponse>.Failure("Пользователь не найден", 404);

            var newPersonal = _mapper.Map<Personal>(req);
            await _context.Personal.AddAsync(newPersonal);
            await _context.SaveChangesAsync();

            if (req.Images?.Count > 0)
            {
                var imagesRes = await _imageService.SaveImagesAsync(req.Images, "personal", newPersonal.Id);
                if (!imagesRes.IsSuccess)
                    return Result<PersonalResponse>.Failure($"Ошибка при сохранении изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);

                foreach (var path in imagesRes.Data!)
                    await _context.Images.AddAsync(new Image { PersonalId = newPersonal.Id, Path = path });

                await _context.SaveChangesAsync();
            }

            return Result<PersonalResponse>.Success(_mapper.Map<PersonalResponse>(newPersonal));
        }

        public async Task<Result<PersonalResponse>> UpdatePersonalAsync(PersonalUpdateRequest req, long id)
        {
            var personal = await _context.Personal.FirstOrDefaultAsync(x => x.Id == id);
            if (personal == null)
                return Result<PersonalResponse>.Failure("Персонал не найден", 404);

            _mapper.Map(req, personal);
            _context.Personal.Update(personal);
            await _context.SaveChangesAsync();

            return Result<PersonalResponse>.Success(_mapper.Map<PersonalResponse>(personal));
        }

        public async Task<Result<bool>> DeletePersonalAsync(long id)
        {
            var personal = await _context.Personal.FirstOrDefaultAsync(x => x.Id == id);
            if (personal == null)
                return Result<bool>.Failure("Персонал не найден", 404);

            // Удалить тренировки со всеми зависимостями
            var trainingIds = await _context.Trainings
                .Where(x => x.TrainerId == id)
                .Select(x => x.Id)
                .ToListAsync();

            if (trainingIds.Count > 0)
            {
                _context.TrainingMetrics.RemoveRange(_context.TrainingMetrics.Where(x => trainingIds.Contains(x.TrainingId)));
                _context.Attendances.RemoveRange(_context.Attendances.Where(x => trainingIds.Contains(x.TrainingId)));
                // Связь Training.MatchId → Match: при удалении Training Match остаётся.
                _context.Trainings.RemoveRange(_context.Trainings.Where(x => trainingIds.Contains(x.Id)));
            }

            // Удалить группы со связями спортсменов
            var groupIds = await _context.Groups
                .Where(x => x.TrainerId == id)
                .Select(x => x.Id)
                .ToListAsync();

            if (groupIds.Count > 0)
            {
                _context.SportsmanGroups.RemoveRange(_context.SportsmanGroups.Where(x => groupIds.Contains(x.GroupId)));
                _context.Groups.RemoveRange(_context.Groups.Where(x => groupIds.Contains(x.Id)));
            }

            // Удалить команды (обнулить TeamId у спортсменов, удалить изображения команд)
            var teamIds = await _context.Teams
                .Where(x => x.TrainerId == id)
                .Select(x => x.Id)
                .ToListAsync();

            if (teamIds.Count > 0)
            {
                var teamSportsmen = await _context.Sportsmen.Where(x => x.TeamId != null && teamIds.Contains(x.TeamId!.Value)).ToListAsync();
                foreach (var s in teamSportsmen) s.TeamId = null;

                var teamImages = await _context.Images.Where(x => x.TeamId != null && teamIds.Contains(x.TeamId!.Value)).ToListAsync();
                if (teamImages.Count > 0)
                {
                    await _imageService.DeleteImagesAsync(teamImages);
                    _context.Images.RemoveRange(teamImages);
                }

                _context.Teams.RemoveRange(_context.Teams.Where(x => teamIds.Contains(x.Id)));
            }

            // Удалить планы тренировок и индивидуальные задания
            _context.PlanTrainings.RemoveRange(_context.PlanTrainings.Where(x => x.TrainerId == id));
            _context.PersonalWorkouts.RemoveRange(_context.PersonalWorkouts.Where(x => x.PersonalId == id));

            var images = await _context.Images.Where(x => x.PersonalId == id).ToListAsync();
            if (images.Count > 0)
            {
                var deleteRes = await _imageService.DeleteImagesAsync(images);
                if (!deleteRes.IsSuccess)
                    return Result<bool>.Failure($"Ошибка при удалении изображений: {deleteRes.ErrorMessage}", deleteRes.StatusCode);
                _context.Images.RemoveRange(images);
            }

            _context.Personal.Remove(personal);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<string>>> AddPersonalImagesAsync(long personalId, List<IFormFile> images)
        {
            var personalExists = await _context.Personal.AnyAsync(x => x.Id == personalId);
            if (!personalExists)
                return Result<List<string>>.Failure("Персонал не найден", 404);

            var imagesRes = await _imageService.SaveImagesAsync(images, "personal", personalId);
            if (!imagesRes.IsSuccess)
                return Result<List<string>>.Failure($"Ошибка при сохранении изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);

            foreach (var path in imagesRes.Data!)
                await _context.Images.AddAsync(new Image { PersonalId = personalId, Path = path });

            await _context.SaveChangesAsync();
            return Result<List<string>>.Success(imagesRes.Data);
        }

        public async Task<Result<bool>> DeletePersonalImageAsync(long imageId)
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

        public async Task<Result<PersonalWorkoutResponse>> CreateWorkoutAsync(PersonalWorkoutCreateRequest req)
        {
            var personalExists = await _context.Personal.AnyAsync(x => x.Id == req.PersonalId);
            if (!personalExists)
                return Result<PersonalWorkoutResponse>.Failure("Тренер не найден", 404);

            var sportsmanExists = await _context.Sportsmen.AnyAsync(x => x.Id == req.SportsmanId);
            if (!sportsmanExists)
                return Result<PersonalWorkoutResponse>.Failure("Спортсмен не найден", 404);

            var workout = _mapper.Map<PersonalWorkout>(req);
            await _context.PersonalWorkouts.AddAsync(workout);
            await _context.SaveChangesAsync();

            await _context.Entry(workout).Reference(x => x.Sportsman).LoadAsync();
            await _context.Entry(workout).Reference(x => x.Personal).LoadAsync();

            return Result<PersonalWorkoutResponse>.Success(_mapper.Map<PersonalWorkoutResponse>(workout));
        }

        public async Task<Result<bool>> DeleteWorkoutAsync(long workoutId)
        {
            var workout = await _context.PersonalWorkouts.FirstOrDefaultAsync(x => x.Id == workoutId);
            if (workout == null)
                return Result<bool>.Failure("Задание не найдено", 404);

            _context.PersonalWorkouts.Remove(workout);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<PersonalWorkoutResponse>>> GetWorkoutsByPersonalAsync(long personalId, Filter? filter)
        {
            var personalExists = await _context.Personal.AnyAsync(x => x.Id == personalId);
            if (!personalExists)
                return Result<List<PersonalWorkoutResponse>>.Failure("Тренер не найден", 404);

            var workouts = await _context.PersonalWorkouts
                .Include(x => x.Sportsman)
                .Include(x => x.Personal)
                .Where(x => x.PersonalId == personalId)
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PersonalWorkoutResponse>>.Success(_mapper.Map<List<PersonalWorkoutResponse>>(workouts));
        }

        public async Task<Result<List<PersonalWorkoutResponse>>> GetAllWorkoutsAsync(Filter? filter)
        {
            var workouts = await _context.PersonalWorkouts
                .Include(x => x.Sportsman)
                .Include(x => x.Personal)
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PersonalWorkoutResponse>>.Success(_mapper.Map<List<PersonalWorkoutResponse>>(workouts));
        }

        // PlanTraining
        public async Task<Result<PlanTrainingResponse>> GetPlanTrainingAsync(long planId)
        {
            var plan = await _context.PlanTrainings
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x => x.Id == planId);
            if (plan == null)
                return Result<PlanTrainingResponse>.Failure("План тренировки не найден", 404);

            return Result<PlanTrainingResponse>.Success(_mapper.Map<PlanTrainingResponse>(plan));
        }

        public async Task<Result<List<PlanTrainingResponse>>> GetPlanTrainingsByPersonalAsync(long personalId, Filter? filter)
        {
            var personalExists = await _context.Personal.AnyAsync(x => x.Id == personalId);
            if (!personalExists)
                return Result<List<PlanTrainingResponse>>.Failure("Тренер не найден", 404);

            var plans = await _context.PlanTrainings
                .Include(x => x.Trainer)
                .Where(x => x.TrainerId == personalId)
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PlanTrainingResponse>>.Success(_mapper.Map<List<PlanTrainingResponse>>(plans));
        }

        public async Task<Result<List<PlanTrainingResponse>>> GetAllPlanTrainingsAsync(Filter? filter)
        {
            var plans = await _context.PlanTrainings
                .Include(x => x.Trainer)
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PlanTrainingResponse>>.Success(_mapper.Map<List<PlanTrainingResponse>>(plans));
        }

        public async Task<Result<PlanTrainingResponse>> CreatePlanTrainingAsync(PlanTrainingCreateRequest req)
        {
            var trainerExists = await _context.Personal.AnyAsync(x => x.Id == req.TrainerId);
            if (!trainerExists)
                return Result<PlanTrainingResponse>.Failure("Тренер не найден", 404);

            var plan = _mapper.Map<PlanTraining>(req);
            await _context.PlanTrainings.AddAsync(plan);
            await _context.SaveChangesAsync();

            await _context.Entry(plan).Reference(x => x.Trainer).LoadAsync();

            return Result<PlanTrainingResponse>.Success(_mapper.Map<PlanTrainingResponse>(plan));
        }

        public async Task<Result<PlanTrainingResponse>> UpdatePlanTrainingAsync(PlanTrainingUpdateRequest req, long planId)
        {
            var plan = await _context.PlanTrainings
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x => x.Id == planId);
            if (plan == null)
                return Result<PlanTrainingResponse>.Failure("План тренировки не найден", 404);

            _mapper.Map(req, plan);
            _context.PlanTrainings.Update(plan);
            await _context.SaveChangesAsync();

            return Result<PlanTrainingResponse>.Success(_mapper.Map<PlanTrainingResponse>(plan));
        }

        public async Task<Result<bool>> DeletePlanTrainingAsync(long planId)
        {
            var plan = await _context.PlanTrainings.FirstOrDefaultAsync(x => x.Id == planId);
            if (plan == null)
                return Result<bool>.Failure("План тренировки не найден", 404);

            _context.PlanTrainings.Remove(plan);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
