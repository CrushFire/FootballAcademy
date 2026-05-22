using AutoMapper;
using Core.Entities;
using Core.Models.AttendanceModel;
using Core.Models.Auth;
using Core.Models.BroadcastModel;
using Core.Models.MessageModel;
using Core.Models.GroupModel;
using Core.Models.MatchModel;
using Core.Models.MetricModel;
using Core.Models.MetricModel.Coefficientes;
using Core.Models.NormativeModel;
using Core.Models.PersonalModel;
using Core.Models.PersonalWorkoutModel;
using Core.Models.PlanTrainingModel;
using Core.Models.SportsmanModel;
using Core.Models.TeamModel;
using Core.Models.TrainingModel;
using Core.Models.User;
using System.Collections.Generic;
using System.Linq;
using Core.Models.ClassModel;
using Core.Models;

namespace FootballAcademy.Mappers
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            // User
            CreateMap<UserCreateRequest, User>();
            CreateMap<UserUpdateRequest, User>();
            CreateMap<User, UserResponse>();
            CreateMap<User, UserAdminResponse>();
            CreateMap<RegistrationRequest, User>();

            // Personal
            CreateMap<PersonalCreateRequest, Personal>();
            CreateMap<PersonalUpdateRequest, Personal>();
            CreateMap<Personal, PersonalResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => new ImageDto { Id = i.Id, Path = i.Path }).ToList() : null));

            // PersonalWorkout
            CreateMap<PersonalWorkoutCreateRequest, PersonalWorkout>();
            CreateMap<PersonalWorkout, PersonalWorkoutResponse>()
                .ForMember(dest => dest.SportsmanFIO, opt => opt.MapFrom(src => src.Sportsman != null ? src.Sportsman.FIO : string.Empty))
                .ForMember(dest => dest.PersonalFIO, opt => opt.MapFrom(src => src.Personal != null ? src.Personal.FIO : string.Empty));

            // PlanTraining
            CreateMap<PlanTrainingCreateRequest, PlanTraining>();
            CreateMap<PlanTrainingUpdateRequest, PlanTraining>();
            CreateMap<PlanTraining, PlanTrainingResponse>()
                .ForMember(dest => dest.TrainerFIO, opt => opt.MapFrom(src => src.Trainer != null ? src.Trainer.FIO : string.Empty));

            // Sportsman
            CreateMap<SportsmanCreateRequest, Sportsman>();
            CreateMap<SportsmanUpdateRequest, Sportsman>();
            CreateMap<Sportsman, SportsmanResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => new ImageDto { Id = i.Id, Path = i.Path }).ToList() : null));

            // Group
            CreateMap<GroupCreateRequest, Group>();
            CreateMap<GroupUpdateRequest, Group>();
            CreateMap<Group, GroupResponse>();

            // Team
            CreateMap<TeamCreateRequest, Team>();
            CreateMap<TeamUpdateRequest, Team>();
            CreateMap<Team, TeamResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => new ImageDto { Id = i.Id, Path = i.Path }).ToList() : null));

            // Training
            CreateMap<TrainingCreateRequest, Training>();
            CreateMap<TrainingUpdateRequest, Training>();
            CreateMap<Training, TrainingResponse>()
                .ForMember(dest => dest.PlanTrainingName, opt => opt.MapFrom(src => src.PlanTraining != null ? src.PlanTraining.Name : null))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : string.Empty))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trainer != null ? src.Trainer.FIO : null))
                .ForMember(dest => dest.MetricsCount, opt => opt.MapFrom(src => src.Metrics != null ? src.Metrics.Count : 0));
            CreateMap<List<TrainingMetrics>, TrainingMetrics>().ConvertUsing<MetricsAggregator>();
            CreateMap<TrainingMetrics, MetricResponse>();
            CreateMap<MetricCreateRequest, TrainingMetrics>();
            CreateMap<MetricUpdateRequest, TrainingMetrics>();

            // Normative
            CreateMap<NormativeCreateRequest, Normative>();
            CreateMap<NormativeUpdateRequest, Normative>();
            CreateMap<Normative, NormativeResponse>();
            CreateMap<LocalNormativeCreateRequest, LocalNormative>();
            CreateMap<LocalNormativeUpdateRequest, LocalNormative>();
            CreateMap<LocalNormative, LocalNormativeResponse>();
            CreateMap<NormativeSportsmanCreateRequest, NormativeSportsman>();
            CreateMap<NormativeSportsman, NormativeSportsmanResponse>();
            CreateMap<LocalNormativeSportsmanCreateRequest, LocalNormativeSportsman>();
            CreateMap<LocalNormativeSportsman, LocalNormativeSportsmanResponse>();

            // Class
            CreateMap<ClassCreateRequest, Class>();
            CreateMap<ClassUpdateRequest, Class>();
            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : string.Empty))
                .ForMember(dest => dest.WeekDate, opt => opt.Ignore());

            // Attendance
            CreateMap<AttendanceCreateRequest, Attendance>();
            CreateMap<Attendance, AttendanceResponse>();

            // Message
            CreateMap<Message, MessageResponse>();
            CreateMap<Broadcast, BroadcastResponse>()
                .ForMember(dest => dest.RecipientsCount, opt => opt.Ignore());

            // Match
            CreateMap<MatchEvent, MatchEventResponse>();
            CreateMap<Core.Entities.LineupEntry, LineupEntryDto>();
            CreateMap<Match, MatchResponse>()
                .ForMember(dest => dest.HomeTeamName, opt => opt.MapFrom(src => src.HomeTeam != null ? src.HomeTeam.Name : string.Empty))
                .ForMember(dest => dest.OpponentTeamName, opt => opt.MapFrom(src => src.OpponentTeam != null ? src.OpponentTeam.Name : src.OpponentTeamName))
                .ForMember(dest => dest.TrainingIds, opt => opt.MapFrom(src => src.Trainings != null ? src.Trainings.Select(t => t.Id).ToList() : new List<long>()))
                .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.Events != null ? src.Events.OrderBy(e => e.Minute).ToList() : new List<MatchEvent>()))
                .ForMember(dest => dest.Lineup, opt => opt.MapFrom(src => src.Lineup))
                .ForMember(dest => dest.HomeStats, opt => opt.Ignore())
                .ForMember(dest => dest.AwayStats, opt => opt.Ignore());
        }
    }

    public class MetricsAggregator : ITypeConverter<List<TrainingMetrics>, TrainingMetrics>
    {
        public TrainingMetrics Convert(List<TrainingMetrics> source, TrainingMetrics destination, ResolutionContext context)
        {
            var first = source.First();
            return new TrainingMetrics
            {
                SportsmanId = first.SportsmanId,
                Duration = (int)source.Average(m => m.Duration),
                TotalDistance = (int)source.Average(m => m.TotalDistance),
                LowSpeedDistance = (int)source.Average(m => m.LowSpeedDistance),
                ModerateSpeedDistance = (int)source.Average(m => m.ModerateSpeedDistance),
                HighSpeedDistance = (int)source.Average(m => m.HighSpeedDistance),
                SprintDistance = (int)source.Average(m => m.SprintDistance),
                TimeInSpeedZone1 = (int)source.Average(m => m.TimeInSpeedZone1),
                TimeInSpeedZone2 = (int)source.Average(m => m.TimeInSpeedZone2),
                TimeInSpeedZone3 = (int)source.Average(m => m.TimeInSpeedZone3),
                TimeInSpeedZone5 = (int)source.Average(m => m.TimeInSpeedZone5),
                TimeInSpeedZone6 = (int)source.Average(m => m.TimeInSpeedZone6),
                TimeInSpeedZone7 = (int)source.Average(m => m.TimeInSpeedZone7),
                MaximumSpeed = source.Max(m => m.MaximumSpeed),  // Max: лучший результат за период, а не средний
                MaxAcceleration = source.Average(m => m.MaxAcceleration),
                MaxDeceleration = source.Average(m => m.MaxDeceleration),
                AccelerationCount = (int)source.Average(m => m.AccelerationCount),
                DecelerationCount = (int)source.Average(m => m.DecelerationCount),
                SprintEfforts = (int)source.Average(m => m.SprintEfforts),
                ExplosiveEfforts = (int)source.Average(m => m.ExplosiveEfforts),
                PlayerLoad = source.Average(m => m.PlayerLoad),
                Impacts = (int)source.Average(m => m.Impacts),
                TimeInHRRedZone = (int)source.Average(m => m.TimeInHRRedZone),
                AverageHeartRate = (int)source.Average(m => m.AverageHeartRate),
                MaxHeartRate = (int)source.Average(m => m.MaxHeartRate),
                Energy = source.Average(m => m.Energy),
                Position = first.Position
            };
        }
    }
}
