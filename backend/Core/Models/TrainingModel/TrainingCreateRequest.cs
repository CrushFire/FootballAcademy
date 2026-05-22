using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.TrainingModel
{
    public class TrainingCreateRequest
    {
        public long TrainerId { get; set; }
        public long? PlanTrainingId { get; set; }
        public long GroupId { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string? OtherInformation { get; set; }
    }
}
