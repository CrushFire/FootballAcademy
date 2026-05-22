using Core.Interfaces.Services;
using Core.Models;
using Core.Models.PersonalModel;
using Core.Models.PersonalWorkoutModel;
using Core.Models.PlanTrainingModel;
using FootballAcademy.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("personal")]
    public class PersonalController : BaseController
    {
        private readonly IPersonalService _personalService;

        public PersonalController(IPersonalService personalService)
        {
            _personalService = personalService;
        }

        [HttpGet("{personalId}")]
        public async Task<IActionResult> GetPersonal(long personalId)
        {
            var result = await _personalService.GetPersonalAsync(personalId);
            return result.ToActionResult();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            if (UserId == null) return Unauthorized();
            var result = await _personalService.GetPersonalByUserIdAsync(UserId.Value);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonals([FromQuery] Filter? filter)
        {
            var result = await _personalService.GetPersonalsAsync(filter);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePersonal([FromForm] PersonalCreateRequest req)
        {
            var result = await _personalService.CreatePersonalAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("{personalId}")]
        public async Task<IActionResult> UpdatePersonal([FromBody] PersonalUpdateRequest req, long personalId)
        {
            var result = await _personalService.UpdatePersonalAsync(req, personalId);
            return result.ToActionResult();
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] PersonalUpdateRequest req)
        {
            if (UserId == null) return Unauthorized();
            var personalRes = await _personalService.GetPersonalByUserIdAsync(UserId.Value);
            if (!personalRes.IsSuccess || personalRes.Data == null)
                return personalRes.ToActionResult();
            var result = await _personalService.UpdatePersonalAsync(req, personalRes.Data.Id);
            return result.ToActionResult();
        }

        [HttpDelete("{personalId}")]
        public async Task<IActionResult> DeletePersonal(long personalId)
        {
            var result = await _personalService.DeletePersonalAsync(personalId);
            return result.ToActionResult();
        }

        [HttpPost("{personalId}/images")]
        public async Task<IActionResult> AddImages(long personalId, [FromForm] List<IFormFile> newImages)
        {
            var result = await _personalService.AddPersonalImagesAsync(personalId, newImages);
            return result.ToActionResult();
        }

        [HttpPost("me/images")]
        public async Task<IActionResult> AddMyImages([FromForm] List<IFormFile> newImages)
        {
            if (UserId == null) return Unauthorized();
            var personalRes = await _personalService.GetPersonalByUserIdAsync(UserId.Value);
            if (!personalRes.IsSuccess || personalRes.Data == null)
                return personalRes.ToActionResult();
            var result = await _personalService.AddPersonalImagesAsync(personalRes.Data.Id, newImages);
            return result.ToActionResult();
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(long imageId)
        {
            var result = await _personalService.DeletePersonalImageAsync(imageId);
            return result.ToActionResult();
        }

        // PersonalWorkout
        [HttpPost("workout")]
        public async Task<IActionResult> CreateWorkout([FromBody] PersonalWorkoutCreateRequest req)
        {
            var result = await _personalService.CreateWorkoutAsync(req);
            return result.ToActionResult();
        }

        [HttpDelete("workout/{workoutId}")]
        public async Task<IActionResult> DeleteWorkout(long workoutId)
        {
            var result = await _personalService.DeleteWorkoutAsync(workoutId);
            return result.ToActionResult();
        }

        // Все workouts всех тренеров (для админ-страницы). Поддерживает фильтры.
        [HttpGet("workouts")]
        public async Task<IActionResult> GetAllWorkouts([FromQuery] Filter? filter)
        {
            var result = await _personalService.GetAllWorkoutsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("{personalId}/workouts")]
        public async Task<IActionResult> GetWorkoutsByPersonal(long personalId, [FromQuery] Filter? filter)
        {
            var result = await _personalService.GetWorkoutsByPersonalAsync(personalId, filter);
            return result.ToActionResult();
        }

        [HttpGet("me/workouts")]
        public async Task<IActionResult> GetMyWorkouts([FromQuery] Filter? filter)
        {
            if (UserId == null) return Unauthorized();
            var personalRes = await _personalService.GetPersonalByUserIdAsync(UserId.Value);
            if (!personalRes.IsSuccess || personalRes.Data == null)
                return personalRes.ToActionResult();
            var result = await _personalService.GetWorkoutsByPersonalAsync(personalRes.Data.Id, filter);
            return result.ToActionResult();
        }

        // PlanTraining
        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetPlanTraining(long planId)
        {
            var result = await _personalService.GetPlanTrainingAsync(planId);
            return result.ToActionResult();
        }

        // Все планы тренеров (для админ-страницы). Поддерживает фильтры.
        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans([FromQuery] Filter? filter)
        {
            var result = await _personalService.GetAllPlanTrainingsAsync(filter);
            return result.ToActionResult();
        }

        [HttpGet("{personalId}/plans")]
        public async Task<IActionResult> GetPlansByPersonal(long personalId, [FromQuery] Filter? filter)
        {
            var result = await _personalService.GetPlanTrainingsByPersonalAsync(personalId, filter);
            return result.ToActionResult();
        }

        [HttpGet("me/plans")]
        public async Task<IActionResult> GetMyPlans([FromQuery] Filter? filter)
        {
            if (UserId == null) return Unauthorized();
            var personalRes = await _personalService.GetPersonalByUserIdAsync(UserId.Value);
            if (!personalRes.IsSuccess || personalRes.Data == null)
                return personalRes.ToActionResult();
            var result = await _personalService.GetPlanTrainingsByPersonalAsync(personalRes.Data.Id, filter);
            return result.ToActionResult();
        }

        [HttpPost("plan")]
        public async Task<IActionResult> CreatePlanTraining([FromBody] PlanTrainingCreateRequest req)
        {
            var result = await _personalService.CreatePlanTrainingAsync(req);
            return result.ToActionResult();
        }

        [HttpPut("plan/{planId}")]
        public async Task<IActionResult> UpdatePlanTraining([FromBody] PlanTrainingUpdateRequest req, long planId)
        {
            var result = await _personalService.UpdatePlanTrainingAsync(req, planId);
            return result.ToActionResult();
        }

        [HttpDelete("plan/{planId}")]
        public async Task<IActionResult> DeletePlanTraining(long planId)
        {
            var result = await _personalService.DeletePlanTrainingAsync(planId);
            return result.ToActionResult();
        }
    }
}
