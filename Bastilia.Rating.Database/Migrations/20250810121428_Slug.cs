using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class Slug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Users");
        }
    }
}
