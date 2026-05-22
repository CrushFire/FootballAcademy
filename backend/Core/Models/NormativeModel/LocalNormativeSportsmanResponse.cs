namespace Core.Models.NormativeModel
{
    public class LocalNormativeSportsmanResponse
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public long LocalNormativeId { get; set; }
        public double Result { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
