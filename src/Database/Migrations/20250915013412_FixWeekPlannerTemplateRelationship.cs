using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixWeekPlannerTemplateRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleConfig");

            migrationBuilder.DropTable(
                name: "AccountSetupScheduleGrid");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountSetupStateId",
                table: "WeekPlannerTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "AccountSetupState",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBreaks",
                table: "AccountSetupState",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPeriods",
                table: "AccountSetupState",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "AccountSetupState",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlannerTemplates_AccountSetupStateId",
                table: "WeekPlannerTemplates",
                column: "AccountSetupStateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WeekPlannerTemplates_AccountSetupState_AccountSetupStateId",
                table: "WeekPlannerTemplates",
                column: "AccountSetupStateId",
                principalTable: "AccountSetupState",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeekPlannerTemplates_AccountSetupState_AccountSetupStateId",
                table: "WeekPlannerTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WeekPlannerTemplates_AccountSetupStateId",
                table: "WeekPlannerTemplates");

            migrationBuilder.DropColumn(
                name: "AccountSetupStateId",
                table: "WeekPlannerTemplates");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "AccountSetupState");

            migrationBuilder.DropColumn(
                name: "NumberOfBreaks",
                table: "AccountSetupState");

            migrationBuilder.DropColumn(
                name: "NumberOfPeriods",
                table: "AccountSetupState");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "AccountSetupState");

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleConfig",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    NumberOfBreaks = table.Column<int>(type: "integer", nullable: false),
                    NumberOfLessons = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
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
                    ScheduleConfigUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsFirstPeriodOfBlock = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NumberOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Subject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AccountSetupScheduleGrid_ScheduleSlots",
                columns: table => new
                {
                    DayColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsFirstPeriodOfBlock = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NumberOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Subject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleGrid_UserId",
                table: "AccountSetupScheduleGrid",
                column: "UserId");
        }
    }
}
