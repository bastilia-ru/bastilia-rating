using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class YearlyAch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "YearlyAchievement",
                table: "AchievementTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YearlyAchievement",
                table: "AchievementTemplates");
        }
    }
}
