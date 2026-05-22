using Core.Enums.Match;

namespace Core.Entities
{
    public class LineupEntry
    {
        public long SportsmanId { get; set; }
        public string Position { get; set; } = string.Empty;
        public PlayerType Type { get; set; } = PlayerType.Main;
    }
}
