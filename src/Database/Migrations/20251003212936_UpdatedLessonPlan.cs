using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLessonPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanningNotes",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "NumberOfBreaks",
                table: "AccountSetupState");

            migrationBuilder.AddColumn<int>(
                name: "PeriodType",
                table: "LessonPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeriodType",
                table: "LessonPlans");

            migrationBuilder.AddColumn<string>(
                name: "PlanningNotes",
                table: "LessonPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBreaks",
                table: "AccountSetupState",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
