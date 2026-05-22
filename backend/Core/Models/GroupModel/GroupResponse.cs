namespace Core.Models.GroupModel
{
    public class GroupResponse
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
