using Core.Enums;

namespace Core.Models.SportsmanModel
{
    public class SportsmanResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FIO { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public Position? Position { get; set; }
        public long? TeamId { get; set; }
        public char Gender { get; set; }
        public Specialization Specialization { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Core.Models.ImageDto>? Images { get; set; }
    }
}
