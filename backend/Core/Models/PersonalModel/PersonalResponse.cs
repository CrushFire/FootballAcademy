using Core.Enums;

namespace Core.Models.PersonalModel
{
    public class PersonalResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FIO { get; set; }
        public string Position { get; set; }
        public PersonalType Type { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Core.Models.ImageDto>? Images { get; set; }
    }
}
