using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.TeamModel
{
    public class TeamUpdateRequest
    {
        [Required]
        public string Name { get; set; }

        public AgeGroup AgeGroup { get; set; }

        public long? TrainerId { get; set; }
    }
}
