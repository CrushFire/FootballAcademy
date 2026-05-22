namespace Core.Models.MetricModel.Pentagon
{
    public class MetricPentagonResponse
    {
        public long SportsmanId { get; set; }
        public string SportsmanName { get; set; }
        public PentagonScores Pentagon { get; set; }
    }
}
