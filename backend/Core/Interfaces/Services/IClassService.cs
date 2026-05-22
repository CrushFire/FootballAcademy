using Core.Models;
using Core.Models.ClassModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IClassService
    {
        Task<Result<ClassResponse>> GetClassAsync(long classId);
        Task<Result<List<ClassResponse>>> GetClassesAsync(Filter? filter);
        Task<Result<ClassResponse>> CreateClassAsync(ClassCreateRequest req);
        Task<Result<ClassResponse>> UpdateClassAsync(ClassUpdateRequest req, long classId);
        Task<Result<bool>> DeleteClassAsync(long classId);
    }
}
