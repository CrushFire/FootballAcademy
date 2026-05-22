namespace Core.Models.NormativeModel
{
    public class NormativeUpdateRequest
    {
        public int AgeGroup { get; set; }
        public char Gender { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool IsAboveYearOfStudy { get; set; }
        public double GradeExcellent { get; set; }
        public double GradeGood { get; set; }
        public double GradeSatisfactory { get; set; }
    }
}
