namespace Core.Models.NormativeModel
{
    public class NormativeSportsmanResponse
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public long NormativeId { get; set; }
        public double Result { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
