using System.ComponentModel.DataAnnotations;

namespace Core.Models.GroupModel
{
    public class GroupCreateRequest
    {
        public long TrainerId { get; set; }

        // формат: КатегорияЧисло
        [Required]
        [RegularExpression(@"^[А-Яа-яA-Za-z]+\d+$", ErrorMessage = "Название группы должно быть в формате: КатегорияЧисло (Юниоры16)")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
