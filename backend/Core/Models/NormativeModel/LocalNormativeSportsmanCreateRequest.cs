namespace Core.Models.NormativeModel
{
    public class LocalNormativeSportsmanCreateRequest
    {
        public long SportsmanId { get; set; }
        public long LocalNormativeId { get; set; }
        public double Result { get; set; }
    }
}
