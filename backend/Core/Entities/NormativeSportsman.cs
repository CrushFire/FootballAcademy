namespace Core.Entities
{
    public class NormativeSportsman
    {
        public long Id { get; set; }
        public long SportsmanId { get; set; }
        public long NormativeId { get; set; }
        public double Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public Sportsman Sportsman { get; set; } = null!;
        public Normative Normative { get; set; } = null!;
    }
}
