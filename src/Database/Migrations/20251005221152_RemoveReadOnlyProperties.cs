using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReadOnlyProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YearLevels",
                table: "YearData",
                newName: "YearLevelsTaught");

            migrationBuilder.AlterColumn<string>(
                name: "BreakDutyOverrides",
                table: "DayPlan",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YearLevelsTaught",
                table: "YearData",
                newName: "YearLevels");

            migrationBuilder.AlterColumn<string>(
                name: "BreakDutyOverrides",
                table: "DayPlan",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
