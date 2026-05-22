using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.PersonalWorkoutModel;
using Core.Models.SportsmanModel;
using Core.Results;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SportsmanService : ISportsmanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;

        public SportsmanService(ApplicationDbContext context, IMapper mapper, ImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<Result<SportsmanResponse>> GetSportsmanByUserIdAsync(long userId)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.UserId == userId);
            if (sportsman == null)
                return Result<SportsmanResponse>.Failure("Данный спортсмен не найден", 404);
            return Result<SportsmanResponse>.Success(_mapper.Map<SportsmanResponse>(sportsman));
        }

        public async Task<Result<SportsmanResponse>> GetSportsmanAsync(long id)
        {
            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == id);

            if (sportsman == null)
            {
                return Result<SportsmanResponse>.Failure("Данный спортсмен не найден", 404);
            }

            var sportmanRes = _mapper.Map<SportsmanResponse>(sportsman);

            return Result<SportsmanResponse>.Success(sportmanRes);
        }

        public async Task<Result<List<SportsmanResponse>>> GetSportsmenAsync(Filter? filter)
        {
            var sportsmans = await _context.Sportsmen
                .ApplyFilter(filter)
                .ToListAsync();

            var sportsmansRes = _mapper.Map<List<SportsmanResponse>>(sportsmans);

            return Result<List<SportsmanResponse>>.Success(sportsmansRes);
        }

        public async Task<Result<SportsmanResponse>> CreateSportsmanAsync(SportsmanCreateRequest req)
        {
            var userExist = await _context.Users.AnyAsync(x => x.Id == req.UserId);
            if (!userExist)
            {
                return Result<SportsmanResponse>.Failure("Пользователь не найден", 404);
            }

            var newSportsman = _mapper.Map<Sportsman>(req);
            await _context.Sportsmen.AddAsync(newSportsman);
            await _context.SaveChangesAsync();

            if (req.Images?.Count > 0)
            {
                var imagesRes = await _imageService.SaveImagesAsync(req.Images, "sportsman", newSportsman.Id);

                if (!imagesRes.IsSuccess)
                {
                    return Result<SportsmanResponse>.Failure($"Ошибка при попытке сохранения изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);
                }

                foreach (var path in imagesRes.Data!)
                {
                    await _context.Images.AddAsync(new Image
                    {
                        SportsmanId = newSportsman.Id,
                        Path = path
                    });
                }

                await _context.SaveChangesAsync();
            }

            var sportsmanRes = _mapper.Map<SportsmanResponse>(newSportsman);
            return Result<SportsmanResponse>.Success(sportsmanRes);
        }

        public async Task<Result<bool>> DeleteSportsmanAsync(long id)
        {
            var sportsmanExist = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == id);
            if (sportsmanExist == null)
                return Result<bool>.Failure("Спортсмен не найден", 404);

            _context.SportsmanGroups.RemoveRange(_context.SportsmanGroups.Where(x => x.SportsmanId == id));
            _context.NormativeSportsmen.RemoveRange(_context.NormativeSportsmen.Where(x => x.SportsmanId == id));
            _context.LocalNormativeSportsmen.RemoveRange(_context.LocalNormativeSportsmen.Where(x => x.SportsmanId == id));
            _context.Attendances.RemoveRange(_context.Attendances.Where(x => x.SportsmanId == id));
            _context.TrainingMetrics.RemoveRange(_context.TrainingMetrics.Where(x => x.SportsmanId == id));
            _context.PersonalWorkouts.RemoveRange(_context.PersonalWorkouts.Where(x => x.SportsmanId == id));

            var images = await _context.Images.Where(x => x.SportsmanId == id).ToListAsync();
            if (images.Count > 0)
            {
                var deleteRes = await _imageService.DeleteImagesAsync(images);
                if (!deleteRes.IsSuccess)
                    return Result<bool>.Failure($"Ошибка при попытке удаления изображений: {deleteRes.ErrorMessage}", deleteRes.StatusCode);
                _context.Images.RemoveRange(images);
            }

            _context.Sportsmen.Remove(sportsmanExist);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<SportsmanResponse>> UpdateSportsmanAsync(SportsmanUpdateRequest req, long id)
        {
            var sportsmanExist = await _context.Sportsmen.FirstOrDefaultAsync(x => x.Id == id);
            if (sportsmanExist == null)
            {
                return Result<SportsmanResponse>.Failure("Спортсмен не найден", 404);
            }

            var sportsman = _mapper.Map(req, sportsmanExist);

            _context.Sportsmen.Update(sportsman);
            await _context.SaveChangesAsync();

            var sportsmanRes = _mapper.Map<SportsmanResponse>(sportsman);
            return Result<SportsmanResponse>.Success(sportsmanRes);
        }

        public async Task<Result<List<string>>> AddSportsmanImagesAsync(long sportsmanId, List<IFormFile> images)
        {
            var sportsmanExist = await _context.Sportsmen.AnyAsync(x => x.Id == sportsmanId);
            if (!sportsmanExist)
            {
                return Result<List<string>>.Failure("Спортсмен не найден", 404);
            }

            var imagesRes = await _imageService.SaveImagesAsync(images, "sportsman", sportsmanId);
            if (!imagesRes.IsSuccess)
            {
                return Result<List<string>>.Failure($"Ошибка при попытке сохранения изображений: {imagesRes.ErrorMessage}", imagesRes.StatusCode);
            }

            foreach (var path in imagesRes.Data!)
            {
                await _context.Images.AddAsync(new Image
                {
                    SportsmanId = sportsmanId,
                    Path = path
                });
            }

            await _context.SaveChangesAsync();
            return Result<List<string>>.Success(imagesRes.Data);
        }

        public async Task<Result<bool>> DeleteSportsmanImageAsync(long imageId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(x => x.Id == imageId);
            if (image == null)
            {
                return Result<bool>.Failure("Изображение не найдено", 404);
            }

            var deleteRes = await _imageService.DeleteImageAsync(image.Path);
            if (!deleteRes.IsSuccess)
            {
                return Result<bool>.Failure($"Ошибка при попытке удаления изображения: {deleteRes.ErrorMessage}", deleteRes.StatusCode);
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<PersonalWorkoutResponse>>> GetWorkoutsBySportsmanAsync(long sportsmanId, Filter? filter)
        {
            var sportsmanExists = await _context.Sportsmen.AnyAsync(x => x.Id == sportsmanId);
            if (!sportsmanExists)
                return Result<List<PersonalWorkoutResponse>>.Failure("Спортсмен не найден", 404);

            var workouts = await _context.PersonalWorkouts
                .Include(x => x.Personal)
                .Include(x => x.Sportsman)
                .Where(x => x.SportsmanId == sportsmanId)
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<List<PersonalWorkoutResponse>>.Success(_mapper.Map<List<PersonalWorkoutResponse>>(workouts));
        }

        public async Task<Result<List<SportsmanResponse>>> GetSportsmensForGroupAsync(long groupId)
        {
            var groupExists = await _context.Groups.AnyAsync(x => x.Id == groupId);
            if (!groupExists)
                return Result<List<SportsmanResponse>>.Failure("Группа не найдена", 404);

            var sportsmen = await _context.SportsmanGroups
                .Where(sg => sg.GroupId == groupId)
                .Include(sg => sg.Sportsman)
                .Select(sg => sg.Sportsman)
                .ToListAsync();

            return Result<List<SportsmanResponse>>.Success(_mapper.Map<List<SportsmanResponse>>(sportsmen));
        }
    }
}
