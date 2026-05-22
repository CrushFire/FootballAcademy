using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchEventCreateRequest
    {
        public MatchEventType Type { get; set; }
        public bool IsHomeTeam { get; set; }
        public int Minute { get; set; }
        public string? Comment { get; set; }
        public long? SportsmanId { get; set; }
        public long? SubstituteSportsmanId { get; set; }
    }
}
