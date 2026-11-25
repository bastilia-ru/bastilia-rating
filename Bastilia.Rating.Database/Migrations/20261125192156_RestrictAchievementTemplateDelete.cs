using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations;

/// <inheritdoc />
public partial class _20261125192156_RestrictAchievementTemplateDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Achievements_AchievementTemplates_AchievementTemplateId",
            table: "Achievements");

        migrationBuilder.AddForeignKey(
            name: "FK_Achievements_AchievementTemplates_AchievementTemplateId",
            table: "Achievements",
            column: "AchievementTemplateId",
            principalTable: "AchievementTemplates",
            principalColumn: "AchievementTemplateId",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Achievements_AchievementTemplates_AchievementTemplateId",
            table: "Achievements");

        migrationBuilder.AddForeignKey(
            name: "FK_Achievements_AchievementTemplates_AchievementTemplateId",
            table: "Achievements",
            column: "AchievementTemplateId",
            principalTable: "AchievementTemplates",
            principalColumn: "AchievementTemplateId",
            onDelete: ReferentialAction.Cascade);
    }
}
