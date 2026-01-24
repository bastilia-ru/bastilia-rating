using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class PartyLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinrpgUserId",
                table: "UserBirthdayParties");

            migrationBuilder.RenameColumn(
                name: "JoinrpgUserId",
                table: "UserBirthdayParties",
                newName: "JoinRpgUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserBirthdayParties_JoinrpgUserId",
                table: "UserBirthdayParties",
                newName: "IX_UserBirthdayParties_JoinRpgUserId");

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "UserBirthdayParties",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinRpgUserId",
                table: "UserBirthdayParties",
                column: "JoinRpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinRpgUserId",
                table: "UserBirthdayParties");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "UserBirthdayParties");

            migrationBuilder.RenameColumn(
                name: "JoinRpgUserId",
                table: "UserBirthdayParties",
                newName: "JoinrpgUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserBirthdayParties_JoinRpgUserId",
                table: "UserBirthdayParties",
                newName: "IX_UserBirthdayParties_JoinrpgUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinrpgUserId",
                table: "UserBirthdayParties",
                column: "JoinrpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
