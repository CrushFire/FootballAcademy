using Core.Enums;
using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class Filters
    {
        // Общие фильтры
        public List<long> Id { get; set; } = new List<long>();
        public List<string> Name { get; set; } = new List<string>();
        public DateRange? CreatedAt { get; set; }
        
        // Фильтры для Sportsman
        public List<string> FIO { get; set; } = new List<string>();
        public List<long> UserId { get; set; } = new List<long>();
        public List<long> SportsmanId { get; set; } = new List<long>();
        public List<string> Position { get; set; } = new List<string>();
        public List<char> Gender { get; set; } = new List<char>();
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public int? HeightFrom { get; set; }
        public int? HeightTo { get; set; }
        public int? WeightFrom { get; set; }
        public int? WeightTo { get; set; }
        public DateRange? BirthDate { get; set; }
        
        // Фильтры для Group
        public List<long> GroupId { get; set; } = new List<long>();
        
        // Фильтры для Team
        public List<long> TeamId { get; set; } = new List<long>();
        public List<AgeGroup> AgeGroup { get; set; } = new List<AgeGroup>();
        
        // Фильтры для Trainer
        public List<long> TrainerId { get; set; } = new List<long>();
        
        // Фильтры для Training
        public List<string> Type { get; set; } = new List<string>();
        public List<long> PlanTrainingId { get; set; } = new List<long>();
        public DateRange? Date { get; set; }

        // Фильтры для Match
        public List<char> Result { get; set; } = new List<char>(); // W, D, L
        public List<long> HomeTeamId { get; set; } = new List<long>();
        public List<long> OpponentTeamId { get; set; } = new List<long>();
        
        // Сортировка статистики команд (применяется после агрегации)
        public string? StatSortField { get; set; }  // Wins, Draws, Losses, WinRate, TotalMatches
        public string? StatSortDirection { get; set; } = "desc"; // asc / desc
        
        // Фильтры для User
        public List<string> Role { get; set; } = new List<string>();

        // Фильтры для Class
        public List<DayOfWeek> DayOfWeek { get; set; } = new List<DayOfWeek>();
        public List<string> SportHall { get; set; } = new List<string>();

        // Фильтры для Message
        public List<long> SenderId { get; set; } = new List<long>();
        public List<long> ReceiverId { get; set; } = new List<long>();
        public List<long> BroadcastId { get; set; } = new List<long>();
        public bool? IsRead { get; set; }

        // Фильтры для Broadcast
        public List<string> TargetType { get; set; } = new List<string>();
        public List<long> CreatedById { get; set; } = new List<long>();
    }
}
