using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class LineupEntryDto
    {
        public long SportsmanId { get; set; }
        public string Position { get; set; } = string.Empty;
        public PlayerType Type { get; set; } = PlayerType.Main;
    }
}
