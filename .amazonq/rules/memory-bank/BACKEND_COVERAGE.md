# Итоговая реализация бекенда — покрытие сущностей

## Все сущности и их сервисы

| Сущность | Интерфейс | Сервис | Program.cs |
|---|---|---|---|
| User | IUserService | UserService | ✅ |
| Personal | IPersonalService | PersonalService | ✅ |
| Sportsman | ISportsmanService | SportsmanService | ✅ |
| PersonalWorkout | ISportsmanService | SportsmanService | ✅ |
| Group | IGroupService | GroupService | ✅ |
| SportsmanGroup | — | внутри GroupService | ✅ |
| Team | ITeamService | TeamService | ✅ |
| Training | ITrainingService | TrainingService | ✅ |
| TrainingMetrics | IMetricsService | MetricService | ✅ |
| PlanTraining | ISchedualeService | SchedualeService | ✅ |
| Attendance | ISchedualeService | SchedualeService | ✅ |
| Class | IClassService | ClassService | ✅ |
| Match | IMatchService | MatchService | ✅ |
| MatchEvent | IMatchService | MatchService (внутри) | ✅ |
| Normative | INormativeService | NormativeService | ✅ |
| NormativeSportsman | INormativeService | NormativeService (внутри) | ✅ |
| LocalNormative | INormativeService | NormativeService (внутри) | ✅ |
| LocalNormativeSportsman | INormativeService | NormativeService (внутри) | ✅ |
| Message | IMessageService | MessageService | ✅ |
| Broadcast | IMessageService | MessageService (внутри) | ✅ |
| Image | — | ImageService (прямой, без интерфейса) | ✅ |
| Auth | IAuthService | AuthService | ✅ |
| TrainingMetrics (аналитика) | IGraphService / IMainMetricService / IMedicalMetricService / IPentagonService / IProfileService | MetricAnalytic/* | ✅ |

## Сервисы без интерфейса (регистрируются напрямую)
- ImageService
- MetricAutoImportService (singleton)
- JwtTokenGen
- PasswordHasher
- ParserMetrics
- Coefficientes/* (AgeCoefficientService, PositionCoeffiecientService, AbsoluteStandardService)

## Статус
Все сущности покрыты. Пробелов нет.
