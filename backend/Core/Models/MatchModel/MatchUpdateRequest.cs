using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchUpdateRequest
    {
        public string? OpponentTeamName { get; set; }
        public GameType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
