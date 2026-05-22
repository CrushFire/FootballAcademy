using Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Core.Models.SportsmanModel
{
    public class SportsmanUpdateRequest
    {
        public string FIO { get; set; }
        public DateTime BirthDate { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public Position? Position { get; set; }
        public char Gender { get; set; }
        public Specialization Specialization { get; set; }
    }
}
