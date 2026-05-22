using Core.Enums.Match;

namespace Core.Models.MatchModel
{
    public class MatchFinishRequest
    {
        public MatchResult Result { get; set; }
        public string? TrainerComment { get; set; }
    }
}
