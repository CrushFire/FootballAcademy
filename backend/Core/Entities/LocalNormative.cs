using Core.Enums;

namespace Core.Entities
{
    public class LocalNormative
    {
        public long Id { get; set; }
        public Specialization Specialization { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public char Gender { get; set; }
        public double Value { get; set; }
        public bool IsMoreBetter { get; set; }
        // true = не менее, false = не более
        // для норматива например пробежать не более прыгнуть не менее
        public DateTime CreatedAt { get; set; }
        public List<LocalNormativeSportsman> LocalNormativeSportsmen { get; set; } = new();
    }
}
