using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bastilia.Rating.Database.Migrations
{
    /// <inheritdoc />
    public partial class DailyJobRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyJobRuns",
                columns: table => new
                {
                    DailyJobRunId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DayOfRun = table.Column<DateOnly>(type: "date", nullable: false),
                    JobStatus = table.Column<int>(type: "integer", nullable: false),
                    MachineName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyJobRuns", x => x.DailyJobRunId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyJobRuns_JobName_DayOfRun",
                table: "DailyJobRuns",
                columns: new[] { "JobName", "DayOfRun" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyJobRuns");
        }
    }
}
