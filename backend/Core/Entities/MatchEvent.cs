using Core.Enums.Match;

namespace Core.Entities
{
    public class MatchEvent
    {
        public long Id { get; set; }
        public long MatchId { get; set; }
        public MatchEventType Type { get; set; }
        public bool IsHomeTeam { get; set; }
        public int Minute { get; set; }
        public string? Comment { get; set; }
        public long? SportsmanId { get; set; }
        public long? SubstituteSportsmanId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Match Match { get; set; } = null!;
    }
}
