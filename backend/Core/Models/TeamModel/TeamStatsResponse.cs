using Core.Enums;

namespace Core.Models.TeamModel
{
    public class TeamStatsResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public AgeGroup AgeGroup { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int TotalMatches { get; set; }
        public double WinRate => TotalMatches > 0 ? Math.Round((double)Wins / TotalMatches * 100, 1) : 0;
    }
}
