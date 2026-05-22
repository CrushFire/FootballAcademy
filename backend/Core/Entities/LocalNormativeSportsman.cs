namespace Core.Entities
{
    public class LocalNormativeSportsman
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public long LocalNormativeId { get; set; }
        public double Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public Sportsman Sportsman { get; set; } = null!;
        public LocalNormative LocalNormative { get; set; } = null!;
    }
}
