using Core.Enums;

namespace Core.Models.NormativeModel
{
    public class LocalNormativeResponse
    {
        public long Id { get; set; }
        public Specialization Specialization { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public char Gender { get; set; }
        public double Value { get; set; }
        public bool IsMoreBetter { get; set; }
        public bool IsAboveYearOfStudy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
