using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchLineupAndSubstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SportsmanId",
                table: "MatchEvents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SubstituteSportsmanId",
                table: "MatchEvents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lineup",
                table: "Matches",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SportsmanId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "SubstituteSportsmanId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "Lineup",
                table: "Matches");
        }
    }
}
