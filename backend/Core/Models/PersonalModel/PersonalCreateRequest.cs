using Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Core.Models.PersonalModel
{
    public class PersonalCreateRequest
    {
        public long UserId { get; set; }
        public string FIO { get; set; }
        public string Position { get; set; }
        public PersonalType Type { get; set; }
        public string Description { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
