using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstPeriodOfBlock",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPeriods",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstPeriodOfBlock",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPeriods",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstPeriodOfBlock",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "NumberOfPeriods",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "IsFirstPeriodOfBlock",
                table: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "NumberOfPeriods",
                table: "AccountSetupScheduleConfig_ScheduleSlots");
        }
    }
}
