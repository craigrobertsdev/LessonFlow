using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSetupStateToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountSetupState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    CompletedSteps = table.Column<int[]>(type: "integer[]", nullable: false),
                    SchoolName = table.Column<string>(type: "text", nullable: false),
                    CalendarYear = table.Column<int>(type: "integer", nullable: false),
                    YearLevelsTaught = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SubjectsTaught = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WorkingDays = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    IsLoading = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetupState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSetupState_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleConfig",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberOfLessons = table.Column<int>(type: "integer", nullable: false),
                    NumberOfBreaks = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetupScheduleConfig", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleConfig_AccountSetupState_UserId",
                        column: x => x.UserId,
                        principalTable: "AccountSetupState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleGrid",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetupScheduleGrid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleGrid_AccountSetupState_UserId",
                        column: x => x.UserId,
                        principalTable: "AccountSetupState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleConfig_ScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleConfigUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetupScheduleConfig_ScheduleSlots", x => new { x.ScheduleConfigUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleConfig_ScheduleSlots_AccountSetupSchedu~",
                        column: x => x.ScheduleConfigUserId,
                        principalTable: "AccountSetupScheduleConfig",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleConfig_ScheduleSlots_CurriculumSubjects~",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleGrid_ScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetupScheduleGrid_ScheduleSlots", x => new { x.DayColumnId, x.Id });
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleGrid_ScheduleSlots_AccountSetupSchedule~",
                        column: x => x.DayColumnId,
                        principalTable: "AccountSetupScheduleGrid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountSetupScheduleGrid_ScheduleSlots_CurriculumSubjects_S~",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleConfig_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleGrid_UserId",
                table: "AccountSetupScheduleGrid",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleGrid_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleConfig");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleGrid");

            migrationBuilder.DropTable(
                name: "AccountSetupState");
        }
    }
}
