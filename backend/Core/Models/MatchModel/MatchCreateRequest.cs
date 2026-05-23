using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchCreateRequest
    {
        public long HomeTeamId { get; set; }
        public long? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        public GameType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
