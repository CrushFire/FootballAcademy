using System.ComponentModel.DataAnnotations;

namespace Core.Models.GroupModel
{
    public class GroupUpdateRequest
    {
        [Required]
        [RegularExpression(@"^[А-Яа-яA-Za-z]+\d+$", ErrorMessage = "Название группы должно быть в формате: КатегорияЧисло (например: Дошкольники5, Подростки14)")]
        public string Name { get; set; }

        public string Description { get; set; }

        public long? TrainerId { get; set; }
    }
}
