using Core.Enums;

namespace Core.Models.PersonalModel
{
    public class PersonalUpdateRequest
    {
        public string FIO { get; set; }
        public string Position { get; set; }
        public PersonalType? Type { get; set; }
        public string Description { get; set; }
    }
}
