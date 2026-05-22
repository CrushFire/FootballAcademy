using Core.Models;
using Core.Models.NormativeModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface INormativeService
    {
        Task<Result<NormativeResponse>> GetNormativeAsync(long id);
        Task<Result<List<NormativeResponse>>> GetNormativesAsync(Filter? filter);
        Task<Result<NormativeResponse>> CreateNormativeAsync(NormativeCreateRequest req);
        Task<Result<NormativeResponse>> UpdateNormativeAsync(NormativeUpdateRequest req, long id);
        Task<Result<bool>> DeleteNormativeAsync(long id);

        Task<Result<LocalNormativeResponse>> GetLocalNormativeAsync(long id);
        Task<Result<List<LocalNormativeResponse>>> GetLocalNormativesAsync(Filter? filter);
        Task<Result<LocalNormativeResponse>> CreateLocalNormativeAsync(LocalNormativeCreateRequest req);
        Task<Result<LocalNormativeResponse>> UpdateLocalNormativeAsync(LocalNormativeUpdateRequest req, long id);
        Task<Result<bool>> DeleteLocalNormativeAsync(long id);

        Task<Result<NormativeSportsmanResponse>> AddNormativeResultAsync(NormativeSportsmanCreateRequest req);
        Task<Result<List<NormativeSportsmanResponse>>> GetNormativeResultsAsync(long sportsmanId);

        Task<Result<LocalNormativeSportsmanResponse>> AddLocalNormativeResultAsync(LocalNormativeSportsmanCreateRequest req);
        Task<Result<List<LocalNormativeSportsmanResponse>>> GetLocalNormativeResultsAsync(long sportsmanId);
    }
}
