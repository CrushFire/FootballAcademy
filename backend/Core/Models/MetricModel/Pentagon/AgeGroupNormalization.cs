namespace Core.Models.MetricModel
{
    public class AgeGroupNormalization
    {
        // Относительные нормы (по группе спортсменов)
        public double MaxSpeed { get; set; }
        public double MaxDistancePerMinute { get; set; }
        public double MaxPlayerLoad { get; set; }
        public double MaxSprintRatio { get; set; }
        public double MaxExplosiveContribution { get; set; }

        // Абсолютные нормы (по стандартам для возраста)
        public double AbsoluteMaxSpeed { get; set; }
        public double AbsoluteMaxDistancePerMinute { get; set; }
        public double AbsoluteMaxPlayerLoad { get; set; }
        public double AbsoluteMaxSprintRatio { get; set; }
        public double AbsoluteMaxExplosiveContribution { get; set; }

        // Количество спортсменов в группе для этого периода
        public int SportsmanCountInGroup { get; set; }
    }
}
