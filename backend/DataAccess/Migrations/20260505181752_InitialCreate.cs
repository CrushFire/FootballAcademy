using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalNormatives",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Specialization = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<char>(type: "character(1)", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    IsMoreBetter = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalNormatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Normatives",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgeGroup = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<char>(type: "character(1)", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    IsAboveYearOfStudy = table.Column<bool>(type: "boolean", nullable: false),
                    GradeExcellent = table.Column<double>(type: "double precision", nullable: false),
                    GradeGood = table.Column<double>(type: "double precision", nullable: false),
                    GradeSatisfactory = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Normatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Broadcasts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByRole = table.Column<string>(type: "text", nullable: false),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    ExpireAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Broadcasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Broadcasts_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    FIO = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personal_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    SenderRole = table.Column<string>(type: "text", nullable: false),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    BroadcastId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Broadcasts_BroadcastId",
                        column: x => x.BroadcastId,
                        principalTable: "Broadcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Messages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Personal_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Personal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanTrainings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainerId = table.Column<long>(type: "bigint", nullable: false),
                    Workouts = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanTrainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanTrainings_Personal_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Personal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AgeGroup = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Personal_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Personal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    SportHall = table.Column<string>(type: "text", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    BeginTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WeekType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainerId = table.Column<long>(type: "bigint", nullable: false),
                    PlanTrainingId = table.Column<long>(type: "bigint", nullable: true),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OtherInformation = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trainings_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trainings_Personal_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Personal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trainings_PlanTrainings_PlanTrainingId",
                        column: x => x.PlanTrainingId,
                        principalTable: "PlanTrainings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sportsmen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    FIO = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: true),
                    TeamId = table.Column<long>(type: "bigint", nullable: true),
                    Gender = table.Column<char>(type: "character(1)", nullable: false),
                    Specialization = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sportsmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sportsmen_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sportsmen_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HomeTeamId = table.Column<long>(type: "bigint", nullable: false),
                    OpponentTeamId = table.Column<long>(type: "bigint", nullable: true),
                    OpponentTeamName = table.Column<string>(type: "text", nullable: true),
                    TrainingId = table.Column<long>(type: "bigint", nullable: true),
                    TrainerComment = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_OpponentTeamId",
                        column: x => x.OpponentTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Matches_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<long>(type: "bigint", nullable: false),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: true),
                    PersonalId = table.Column<long>(type: "bigint", nullable: true),
                    TeamId = table.Column<long>(type: "bigint", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Images_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Images_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalNormativeSportsmen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    LocalNormativeId = table.Column<long>(type: "bigint", nullable: false),
                    Result = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalNormativeSportsmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalNormativeSportsmen_LocalNormatives_LocalNormativeId",
                        column: x => x.LocalNormativeId,
                        principalTable: "LocalNormatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalNormativeSportsmen_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormativeSportsmen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    NormativeId = table.Column<long>(type: "bigint", nullable: false),
                    Result = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormativeSportsmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormativeSportsmen_Normatives_NormativeId",
                        column: x => x.NormativeId,
                        principalTable: "Normatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NormativeSportsmen_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalWorkouts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    PersonalId = table.Column<long>(type: "bigint", nullable: false),
                    Workout = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalWorkouts_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalWorkouts_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SportsmanGroups",
                columns: table => new
                {
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsmanGroups", x => new { x.SportsmanId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_SportsmanGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportsmanGroups_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<long>(type: "bigint", nullable: false),
                    SportsmanId = table.Column<long>(type: "bigint", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    TotalDistance = table.Column<int>(type: "integer", nullable: false),
                    LowSpeedDistance = table.Column<int>(type: "integer", nullable: false),
                    ModerateSpeedDistance = table.Column<int>(type: "integer", nullable: false),
                    HighSpeedDistance = table.Column<int>(type: "integer", nullable: false),
                    SprintDistance = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone1 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone2 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone3 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone4 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone5 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone6 = table.Column<int>(type: "integer", nullable: false),
                    TimeInSpeedZone7 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone1 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone2 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone3 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone4 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone5 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone6 = table.Column<int>(type: "integer", nullable: false),
                    DistanceInSpeedZone7 = table.Column<int>(type: "integer", nullable: false),
                    AverageSpeed = table.Column<double>(type: "double precision", nullable: false),
                    MaximumSpeed = table.Column<double>(type: "double precision", nullable: false),
                    AccelerationCount = table.Column<int>(type: "integer", nullable: false),
                    DecelerationCount = table.Column<int>(type: "integer", nullable: false),
                    MaxAcceleration = table.Column<double>(type: "double precision", nullable: false),
                    MaxDeceleration = table.Column<double>(type: "double precision", nullable: false),
                    SprintEfforts = table.Column<int>(type: "integer", nullable: false),
                    HighSpeedEfforts = table.Column<int>(type: "integer", nullable: false),
                    PlayerLoad = table.Column<double>(type: "double precision", nullable: false),
                    Energy = table.Column<double>(type: "double precision", nullable: false),
                    WorkRatio = table.Column<double>(type: "double precision", nullable: false),
                    MetabolicPower = table.Column<double>(type: "double precision", nullable: false),
                    Impacts = table.Column<int>(type: "integer", nullable: false),
                    AverageSatellites = table.Column<int>(type: "integer", nullable: false),
                    AverageHDOP = table.Column<double>(type: "double precision", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: true),
                    PositionGroup = table.Column<string>(type: "text", nullable: true),
                    ExplosiveEfforts = table.Column<int>(type: "integer", nullable: false),
                    AverageHeartRate = table.Column<int>(type: "integer", nullable: false),
                    MaxHeartRate = table.Column<int>(type: "integer", nullable: false),
                    HeartRateExertion = table.Column<double>(type: "double precision", nullable: false),
                    TimeInHRZone1 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRZone2 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRZone3 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRZone4 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRZone5 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRZone6 = table.Column<int>(type: "integer", nullable: false),
                    TimeInHRRedZone = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingMetrics_Sportsmen_SportsmanId",
                        column: x => x.SportsmanId,
                        principalTable: "Sportsmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingMetrics_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsHomeTeam = table.Column<bool>(type: "boolean", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchEvents_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SportsmanId",
                table: "Attendances",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_TrainingId",
                table: "Attendances",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_CreatedById",
                table: "Broadcasts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_GroupId",
                table: "Classes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TrainerId",
                table: "Groups",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_PersonalId",
                table: "Images",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_SportsmanId",
                table: "Images",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_TeamId",
                table: "Images",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalNormativeSportsmen_LocalNormativeId",
                table: "LocalNormativeSportsmen",
                column: "LocalNormativeId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalNormativeSportsmen_SportsmanId",
                table: "LocalNormativeSportsmen",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_OpponentTeamId",
                table: "Matches",
                column: "OpponentTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TrainingId",
                table: "Matches",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_MatchId",
                table: "MatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_BroadcastId",
                table: "Messages",
                column: "BroadcastId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_NormativeSportsmen_NormativeId",
                table: "NormativeSportsmen",
                column: "NormativeId");

            migrationBuilder.CreateIndex(
                name: "IX_NormativeSportsmen_SportsmanId",
                table: "NormativeSportsmen",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Personal_UserId",
                table: "Personal",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalWorkouts_PersonalId",
                table: "PersonalWorkouts",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalWorkouts_SportsmanId",
                table: "PersonalWorkouts",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanTrainings_TrainerId",
                table: "PlanTrainings",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_SportsmanGroups_GroupId",
                table: "SportsmanGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Sportsmen_TeamId",
                table: "Sportsmen",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Sportsmen_UserId",
                table: "Sportsmen",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TrainerId",
                table: "Teams",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingMetrics_SportsmanId",
                table: "TrainingMetrics",
                column: "SportsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingMetrics_TrainingId",
                table: "TrainingMetrics",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_GroupId",
                table: "Trainings",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_PlanTrainingId",
                table: "Trainings",
                column: "PlanTrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_TrainerId",
                table: "Trainings",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "LocalNormativeSportsmen");

            migrationBuilder.DropTable(
                name: "MatchEvents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "NormativeSportsmen");

            migrationBuilder.DropTable(
                name: "PersonalWorkouts");

            migrationBuilder.DropTable(
                name: "SportsmanGroups");

            migrationBuilder.DropTable(
                name: "TrainingMetrics");

            migrationBuilder.DropTable(
                name: "LocalNormatives");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Broadcasts");

            migrationBuilder.DropTable(
                name: "Normatives");

            migrationBuilder.DropTable(
                name: "Sportsmen");

            migrationBuilder.DropTable(
                name: "Trainings");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "PlanTrainings");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
