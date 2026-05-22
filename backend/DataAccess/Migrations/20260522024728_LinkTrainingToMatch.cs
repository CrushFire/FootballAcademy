using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LinkTrainingToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Добавляем новую колонку MatchId в Trainings
            migrationBuilder.AddColumn<long>(
                name: "MatchId",
                table: "Trainings",
                type: "bigint",
                nullable: true);

            // 2) Переносим существующие связи Matches.TrainingId → Trainings.MatchId
            // (если у Match.TrainingId = X, то у Training X выставляем MatchId = id матча)
            migrationBuilder.Sql(@"
                UPDATE ""Trainings"" t
                SET ""MatchId"" = m.""Id""
                FROM ""Matches"" m
                WHERE m.""TrainingId"" = t.""Id"";
            ");

            // 3) Удаляем старую связь Matches.TrainingId
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Trainings_TrainingId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_TrainingId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TrainingId",
                table: "Matches");

            // 4) Индекс + FK на новую колонку
            migrationBuilder.CreateIndex(
                name: "IX_Trainings_MatchId",
                table: "Trainings",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_Matches_MatchId",
                table: "Trainings",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Возвращаем колонку TrainingId в Matches
            migrationBuilder.AddColumn<long>(
                name: "TrainingId",
                table: "Matches",
                type: "bigint",
                nullable: true);

            // Переносим связи обратно: первая Training с MatchId=X → Matches.TrainingId
            migrationBuilder.Sql(@"
                UPDATE ""Matches"" m
                SET ""TrainingId"" = sub.""Id""
                FROM (
                    SELECT DISTINCT ON (""MatchId"") ""Id"", ""MatchId""
                    FROM ""Trainings""
                    WHERE ""MatchId"" IS NOT NULL
                    ORDER BY ""MatchId"", ""Id""
                ) sub
                WHERE m.""Id"" = sub.""MatchId"";
            ");

            // Удаляем новую связь
            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_Matches_MatchId",
                table: "Trainings");

            migrationBuilder.DropIndex(
                name: "IX_Trainings_MatchId",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Trainings");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TrainingId",
                table: "Matches",
                column: "TrainingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Trainings_TrainingId",
                table: "Matches",
                column: "TrainingId",
                principalTable: "Trainings",
                principalColumn: "Id");
        }
    }
}
