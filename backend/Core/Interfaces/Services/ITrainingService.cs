using Core.Models;
using Core.Models.TrainingModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface ITrainingService
    {
        Task<Result<TrainingResponse>> GetTrainingAsync(long trainingId);
        Task<Result<List<TrainingResponse>>> GetTrainingsAsync(Filter? filter);
        Task<Result<List<TrainingResponse>>> GetTrainingsForSportsmanAsync(long sportsmanId, Filter? filter);
        Task<Result<TrainingResponse>> CreateTrainingAsync(TrainingCreateRequest req);
        Task<Result<TrainingResponse>> UpdateTrainingAsync(TrainingUpdateRequest req, long trainingId);
        Task<Result<bool>> DeleteTrainingAsync(long trainingId);

    }
}