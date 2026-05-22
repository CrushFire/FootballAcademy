using Core.Entities;
using Core.Results;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class ImageService
    {
        public async Task<Result<string>> SaveImageAsync(IFormFile image, string entityType, long entityId, int index = 0)
        {
            if (image == null || image.Length == 0)
                return Result<string>.Failure("Файл изображения пустой", 400);

            var extension = Path.GetExtension(image.FileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var fileName = index > 0 
                ? $"{entityType}_{entityId}_{timestamp}_{index}{extension}"
                : $"{entityType}_{entityId}_{timestamp}{extension}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            try
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images"));

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                return Result<string>.Success("images/" + fileName);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Ошибка при попытке сохраненить изображения: {ex.Message}", 500);
            }
        }

        public async Task<Result<List<string>>> SaveImagesAsync(List<IFormFile> images, string entityType, long entityId)
        {
            var paths = new List<string>();

            if (images == null || images.Count == 0)
                return Result<List<string>>.Success(paths);

            for (int i = 0; i < images.Count; i++)
            {
                var result = await SaveImageAsync(images[i], entityType, entityId, i + 1);
                if (result.IsSuccess && result.Data != null)
                    paths.Add(result.Data);
                else
                    return Result<List<string>>.Failure($"Ошибка при попытке сохранения изображения: {result.ErrorMessage}", result.StatusCode);
            }

            return Result<List<string>>.Success(paths);
        }

        public async Task<Result<bool>> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return Result<bool>.Failure("Путь к изображению пустой", 400);
            }

            var startIndex = imagePath.IndexOf("images/");
            if (startIndex == -1)
            {
                return Result<bool>.Failure("Некорректный путь к изображению", 400);
            }

            try
            {
                var fileName = imagePath.Substring(startIndex + "images/".Length);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return await Task.FromResult(Result<bool>.Success(true));
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Ошибка при попытке удаления изображения: {ex.Message}", 500);
            }
        }

        public async Task<Result<bool>> DeleteImagesAsync(List<Image> images)
        {
            if (images?.Count == 0 || images == null)
                return Result<bool>.Success(true);

            foreach (var image in images)
            {
                var result = await DeleteImageAsync(image.Path);
                if (!result.IsSuccess)
                    return result;
            }

            return Result<bool>.Success(true);
        }
    }
}
