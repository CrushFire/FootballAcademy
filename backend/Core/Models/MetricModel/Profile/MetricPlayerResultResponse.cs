using Core.Models;

namespace Core.Models.MetricModel.Profile
{
    public class MetricPlayerResultResponse
    {
        public long SportsmanId { get; set; }
        public string SportsmanName { get; set; }
        public Filter? Filter { get; set; }
        public PlayerProfileResult? Profiles { get; set; }
    }
}
