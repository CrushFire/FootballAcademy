using Core.Enums.Match;

namespace Core.Entities
{
    public class Match
    {
        public long Id { get; set; }
        public long HomeTeamId { get; set; }
        public long? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        public string? TrainerComment { get; set; }
        public GameType Type { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
        public MatchResult? Result { get; set; }
        public DateTime Date { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Team HomeTeam { get; set; } = null!;
        public Team? OpponentTeam { get; set; }
        // Один матч может иметь N привязанных тренировок (одна на каждую группу,
        // обычно 1 для внешнего соперника или 2 для домашнего матча между нашими).
        public List<Training> Trainings { get; set; } = new();
        public List<MatchEvent> Events { get; set; } = new();
        public List<LineupEntry> Lineup { get; set; } = new();
    }
}
