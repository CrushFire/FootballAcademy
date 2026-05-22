using Core.Models;
using Core.Models.MatchModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IMatchService
    {
        Task<Result<MatchResponse>> GetMatchAsync(long matchId);
        Task<Result<List<MatchResponse>>> GetMatchesAsync(Filter? filter);
        Task<Result<MatchResponse>> CreateMatchAsync(MatchCreateRequest req);
        Task<Result<MatchResponse>> UpdateMatchAsync(long matchId, MatchUpdateRequest req);
        Task<Result<bool>> DeleteMatchAsync(long matchId);
        Task<Result<MatchResponse>> StartMatchAsync(long matchId);
        Task<Result<MatchResponse>> FinishMatchAsync(long matchId, MatchFinishRequest req);
        Task<Result<MatchEventResponse>> AddEventAsync(long matchId, MatchEventCreateRequest req);
        Task<Result<bool>> DeleteEventAsync(long eventId);
        Task<Result<List<MatchEventResponse>>> GetEventsAsync(long matchId);
        Task<Result<MatchResponse>> UpdateLineupAsync(long matchId, List<LineupEntryDto> lineup);
    }
}
