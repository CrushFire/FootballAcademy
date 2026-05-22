using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.TrainingModel
{
    public class TrainingResponse
    {
        public long Id { get; set; }
        public long TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public long? PlanTrainingId { get; set; }
        public string? PlanTrainingName { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string? OtherInformation { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MetricsCount { get; set; }
    }
}
