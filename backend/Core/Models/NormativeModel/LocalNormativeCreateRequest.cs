using Core.Enums;

namespace Core.Models.NormativeModel
{
    public class LocalNormativeCreateRequest
    {
        public Specialization Specialization { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public char Gender { get; set; }
        public double Value { get; set; }
        public bool IsMoreBetter { get; set; }
    }
}
