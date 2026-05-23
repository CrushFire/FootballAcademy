using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdToTraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_Groups_GroupId",
                table: "Trainings");

            migrationBuilder.AlterColumn<long>(
                name: "GroupId",
                table: "Trainings",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "TeamId",
                table: "Trainings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_TeamId",
                table: "Trainings",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_Groups_GroupId",
                table: "Trainings",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_Teams_TeamId",
                table: "Trainings",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_Groups_GroupId",
                table: "Trainings");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_Teams_TeamId",
                table: "Trainings");

            migrationBuilder.DropIndex(
                name: "IX_Trainings_TeamId",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Trainings");

            migrationBuilder.AlterColumn<long>(
                name: "GroupId",
                table: "Trainings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_Groups_GroupId",
                table: "Trainings",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
