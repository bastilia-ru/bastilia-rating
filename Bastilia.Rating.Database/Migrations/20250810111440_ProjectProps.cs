using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class ProjectProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JoinrpgProjectId",
                table: "BastiliaProjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KogdaIgraProjectId",
                table: "BastiliaProjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectUri",
                table: "BastiliaProjects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinrpgProjectId",
                table: "BastiliaProjects");

            migrationBuilder.DropColumn(
                name: "KogdaIgraProjectId",
                table: "BastiliaProjects");

            migrationBuilder.DropColumn(
                name: "ProjectUri",
                table: "BastiliaProjects");
        }
    }
}
