using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersBastiliaStatuses_Users_UserJoinRpgUserId",
                table: "UsersBastiliaStatuses");

            migrationBuilder.DropIndex(
                name: "IX_UsersBastiliaStatuses_UserJoinRpgUserId",
                table: "UsersBastiliaStatuses");

            migrationBuilder.DropColumn(
                name: "UserJoinRpgUserId",
                table: "UsersBastiliaStatuses");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersBastiliaStatuses_Users_JoinrpgUserId",
                table: "UsersBastiliaStatuses",
                column: "JoinrpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersBastiliaStatuses_Users_JoinrpgUserId",
                table: "UsersBastiliaStatuses");

            migrationBuilder.AddColumn<int>(
                name: "UserJoinRpgUserId",
                table: "UsersBastiliaStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UsersBastiliaStatuses_UserJoinRpgUserId",
                table: "UsersBastiliaStatuses",
                column: "UserJoinRpgUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersBastiliaStatuses_Users_UserJoinRpgUserId",
                table: "UsersBastiliaStatuses",
                column: "UserJoinRpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
