using Core.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.TeamModel
{
    public class TeamCreateRequest
    {
        public long TrainerId { get; set; }

        [Required]
        public string Name { get; set; }

        public AgeGroup AgeGroup { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
