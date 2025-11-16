using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BastiliaProjects",
                columns: table => new
                {
                    BastiliaProjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectName = table.Column<string>(type: "text", nullable: false),
                    ProjectType = table.Column<string>(type: "text", nullable: false),
                    BrandType = table.Column<string>(type: "text", nullable: false),
                    OngoingProject = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectOfTheYear = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BastiliaProjects", x => x.BastiliaProjectId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    JoinRpgUserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    ParticipateInRating = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.JoinRpgUserId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementTemplates",
                columns: table => new
                {
                    AchievementTemplateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: true),
                    AchievementName = table.Column<string>(type: "text", nullable: false),
                    AchievementDescription = table.Column<string>(type: "text", nullable: false),
                    AchievementImageUrl = table.Column<string>(type: "text", nullable: false),
                    AchievementRatingValue = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTemplates", x => x.AchievementTemplateId);
                    table.ForeignKey(
                        name: "FK_AchievementTemplates_BastiliaProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "BastiliaProjects",
                        principalColumn: "BastiliaProjectId");
                    table.ForeignKey(
                        name: "FK_AchievementTemplates_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId");
                });

            migrationBuilder.CreateTable(
                name: "ProjectAdmins",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AddDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RemoveDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAdmins", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ProjectAdmins_BastiliaProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "BastiliaProjects",
                        principalColumn: "BastiliaProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAdmins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersBastiliaStatuses",
                columns: table => new
                {
                    JoinrpgUserId = table.Column<int>(type: "integer", nullable: false),
                    BeginDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StatusType = table.Column<string>(type: "text", nullable: false),
                    UserJoinRpgUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBastiliaStatuses", x => new { x.JoinrpgUserId, x.BeginDate });
                    table.ForeignKey(
                        name: "FK_UsersBastiliaStatuses_Users_UserJoinRpgUserId",
                        column: x => x.UserJoinRpgUserId,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AchievementTemplateId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GrantedBy = table.Column<int>(type: "integer", nullable: false),
                    GrantedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RemovedBy = table.Column<int>(type: "integer", nullable: true),
                    RemovedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OverrideName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementTemplates_AchievementTemplateId",
                        column: x => x.AchievementTemplateId,
                        principalTable: "AchievementTemplates",
                        principalColumn: "AchievementTemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_GrantedBy",
                        column: x => x.GrantedBy,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_RemovedBy",
                        column: x => x.RemovedBy,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "JoinRpgUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementTemplateId",
                table: "Achievements",
                column: "AchievementTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_GrantedBy",
                table: "Achievements",
                column: "GrantedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_RemovedBy",
                table: "Achievements",
                column: "RemovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_UserId",
                table: "Achievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementTemplates_OwnerId",
                table: "AchievementTemplates",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementTemplates_ProjectId",
                table: "AchievementTemplates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAdmins_UserId",
                table: "ProjectAdmins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersBastiliaStatuses_UserJoinRpgUserId",
                table: "UsersBastiliaStatuses",
                column: "UserJoinRpgUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "ProjectAdmins");

            migrationBuilder.DropTable(
                name: "UsersBastiliaStatuses");

            migrationBuilder.DropTable(
                name: "AchievementTemplates");

            migrationBuilder.DropTable(
                name: "BastiliaProjects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
