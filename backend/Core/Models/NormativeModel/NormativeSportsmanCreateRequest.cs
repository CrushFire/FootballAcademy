namespace Core.Models.NormativeModel
{
    public class NormativeSportsmanCreateRequest
    {
        public long SportsmanId { get; set; }
        public long NormativeId { get; set; }
        public double Result { get; set; }
    }
}
