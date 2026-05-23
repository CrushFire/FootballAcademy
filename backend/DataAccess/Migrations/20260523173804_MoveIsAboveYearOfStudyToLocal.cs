using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MoveIsAboveYearOfStudyToLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAboveYearOfStudy",
                table: "Normatives");

            migrationBuilder.AddColumn<bool>(
                name: "IsAboveYearOfStudy",
                table: "LocalNormatives",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAboveYearOfStudy",
                table: "LocalNormatives");

            migrationBuilder.AddColumn<bool>(
                name: "IsAboveYearOfStudy",
                table: "Normatives",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
