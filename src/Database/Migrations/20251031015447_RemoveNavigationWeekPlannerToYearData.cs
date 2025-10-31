using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNavigationWeekPlannerToYearPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeekPlanners_YearPlanId",
                table: "WeekPlanners");

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlanners_YearPlanId_WeekStart",
                table: "WeekPlanners",
                columns: new[] { "YearPlanId", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeekPlanners_YearPlanId_WeekStart",
                table: "WeekPlanners");

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlanners_YearPlanId",
                table: "WeekPlanners",
                column: "YearPlanId");
        }
    }
}
