using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchResponse
    {
        public long Id { get; set; }
        public long HomeTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public long? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        // Ids привязанных тренировок (по одной на каждую группу-участника)
        public List<long> TrainingIds { get; set; } = new();
        public GameType Type { get; set; }
        public MatchStatus Status { get; set; }
        public MatchResult? Result { get; set; }
        public string? TrainerComment { get; set; }
        public DateTime Date { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public MatchStatsDto HomeStats { get; set; }
        public MatchStatsDto AwayStats { get; set; }
        public List<MatchEventResponse> Events { get; set; }
        public List<LineupEntryDto> Lineup { get; set; } = new();
    }

    public class MatchStatsDto
    {
        public int Goals { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int Corners { get; set; }
        public int Fouls { get; set; }
        public int Penalties { get; set; }
    }
}
