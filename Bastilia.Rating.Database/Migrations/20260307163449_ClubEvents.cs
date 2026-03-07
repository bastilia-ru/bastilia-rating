using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class ClubEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserBirthdayParties",
                newName: "ClubEvents");

            migrationBuilder.RenameColumn(
                name: "UserBirthdayPartyId",
                table: "ClubEvents",
                newName: "ClubEventId");

            migrationBuilder.RenameColumn(
                name: "PartyDate",
                table: "ClubEvents",
                newName: "EventDate");

            migrationBuilder.RenameIndex(
                name: "IX_UserBirthdayParties_JoinRpgUserId",
                table: "ClubEvents",
                newName: "IX_ClubEvents_JoinRpgUserId");

            // Rename PK constraint
            migrationBuilder.Sql(@"ALTER INDEX ""PK_UserBirthdayParties"" RENAME TO ""PK_ClubEvents"";");

            // Drop old FK (was CASCADE, required), re-add as optional (no action)
            migrationBuilder.DropForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinRpgUserId",
                table: "ClubEvents");

            migrationBuilder.AlterColumn<int>(
                name: "JoinRpgUserId",
                table: "ClubEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "ClubEvents",
                type: "text",
                nullable: false,
                defaultValueSql: "'BirthdayParty'");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ClubEvents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ClubEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubEvents_Users_JoinRpgUserId",
                table: "ClubEvents",
                column: "JoinRpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubEvents_ProjectId",
                table: "ClubEvents",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubEvents_BastiliaProjects_ProjectId",
                table: "ClubEvents",
                column: "ProjectId",
                principalTable: "BastiliaProjects",
                principalColumn: "BastiliaProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClubEvents_Users_JoinRpgUserId",
                table: "ClubEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ClubEvents_BastiliaProjects_ProjectId",
                table: "ClubEvents");

            migrationBuilder.DropIndex(
                name: "IX_ClubEvents_ProjectId",
                table: "ClubEvents");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "ClubEvents");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ClubEvents");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ClubEvents");

            migrationBuilder.AlterColumn<int>(
                name: "JoinRpgUserId",
                table: "ClubEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.RenameIndex(
                name: "IX_ClubEvents_JoinRpgUserId",
                table: "ClubEvents",
                newName: "IX_UserBirthdayParties_JoinRpgUserId");

            migrationBuilder.RenameColumn(
                name: "EventDate",
                table: "ClubEvents",
                newName: "PartyDate");

            migrationBuilder.RenameColumn(
                name: "ClubEventId",
                table: "ClubEvents",
                newName: "UserBirthdayPartyId");

            migrationBuilder.Sql(@"ALTER INDEX ""PK_ClubEvents"" RENAME TO ""PK_UserBirthdayParties"";");

            migrationBuilder.RenameTable(
                name: "ClubEvents",
                newName: "UserBirthdayParties");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBirthdayParties_Users_JoinRpgUserId",
                table: "UserBirthdayParties",
                column: "JoinRpgUserId",
                principalTable: "Users",
                principalColumn: "JoinRpgUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
