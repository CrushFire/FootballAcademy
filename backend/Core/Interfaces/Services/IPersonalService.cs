using Core.Models;
using Core.Models.PersonalModel;
using Core.Models.PersonalWorkoutModel;
using Core.Models.PlanTrainingModel;
using Core.Results;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Services
{
    public interface IPersonalService
    {
        Task<Result<PersonalResponse>> GetPersonalAsync(long id);
        Task<Result<PersonalResponse>> GetPersonalByUserIdAsync(long userId);
        Task<Result<List<PersonalResponse>>> GetPersonalsAsync(Filter? filter);
        Task<Result<PersonalResponse>> CreatePersonalAsync(PersonalCreateRequest req);
        Task<Result<PersonalResponse>> UpdatePersonalAsync(PersonalUpdateRequest req, long id);
        Task<Result<bool>> DeletePersonalAsync(long id);
        Task<Result<List<string>>> AddPersonalImagesAsync(long personalId, List<IFormFile> images);
        Task<Result<bool>> DeletePersonalImageAsync(long imageId);

        Task<Result<PersonalWorkoutResponse>> CreateWorkoutAsync(PersonalWorkoutCreateRequest req);
        Task<Result<bool>> DeleteWorkoutAsync(long workoutId);
        Task<Result<List<PersonalWorkoutResponse>>> GetWorkoutsByPersonalAsync(long personalId, Filter? filter);
        Task<Result<List<PersonalWorkoutResponse>>> GetAllWorkoutsAsync(Filter? filter);

        Task<Result<PlanTrainingResponse>> GetPlanTrainingAsync(long planId);
        Task<Result<List<PlanTrainingResponse>>> GetPlanTrainingsByPersonalAsync(long personalId, Filter? filter);
        Task<Result<List<PlanTrainingResponse>>> GetAllPlanTrainingsAsync(Filter? filter);
        Task<Result<PlanTrainingResponse>> CreatePlanTrainingAsync(PlanTrainingCreateRequest req);
        Task<Result<PlanTrainingResponse>> UpdatePlanTrainingAsync(PlanTrainingUpdateRequest req, long planId);
        Task<Result<bool>> DeletePlanTrainingAsync(long planId);
    }
}
