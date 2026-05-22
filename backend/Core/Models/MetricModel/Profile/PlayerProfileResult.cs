namespace Core.Models.MetricModel.Profile
{
    public class PlayerProfileResult
    {
        // Активные — где все ужесточённые условия выполнены (логика AND)
        public List<PlayerProfile>? Profiles { get; set; }
        // Потенциальные — где выполнена часть условий (OR), но не все
        public List<PlayerProfile>? PotentialProfiles { get; set; }
    }
}
