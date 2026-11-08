using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserTelegramFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramUsername",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramUsername",
                table: "Users");
        }
    }
}
