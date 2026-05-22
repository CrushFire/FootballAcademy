using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchCreateRequest
    {
        public long HomeTeamId { get; set; }
        public long? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        // Опционально: группы для автосоздания Training (Type="Матч") на каждую.
        // HomeGroupId — для нашей команды, OpponentGroupId — для соперника
        // (используется для матчей между нашими, когда у обеих сторон нужны метрики).
        public long? HomeGroupId { get; set; }
        public long? OpponentGroupId { get; set; }
        public GameType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
