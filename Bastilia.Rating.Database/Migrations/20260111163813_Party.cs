using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class Party : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBirthdayParties",
                columns: table => new
                {
                    UserBirthdayPartyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JoinrpgUserId = table.Column<int>(type: "integer", nullable: false),
                    PartyDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBirthdayParties", x => x.UserBirthdayPartyId);
                    table.ForeignKey(
                        name: "FK_UserBirthdayParties_Users_JoinrpgUserId",
                        column: x => x.JoinrpgUserId,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBirthdayParties_JoinrpgUserId",
                table: "UserBirthdayParties",
                column: "JoinrpgUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBirthdayParties");
        }
    }
}
