namespace Core.Entities
{
    public class Normative
    {
        public long Id { get; set; }
        public int AgeGroup { get; set; }
        public char Gender { get; set; }
        public string Type { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public bool IsAboveYearOfStudy { get; set; }
        public double GradeExcellent { get; set; }
        public double GradeGood { get; set; }
        public double GradeSatisfactory { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<NormativeSportsman> NormativeSportsmen { get; set; } = new();
    }
}
