using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveTodoListToWeekPlanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItem_LessonPlans_LessonPlanId",
                table: "TodoItem");

            migrationBuilder.RenameColumn(
                name: "LessonPlanId",
                table: "TodoItem",
                newName: "WeekPlannerId");

            migrationBuilder.RenameIndex(
                name: "IX_TodoItem_LessonPlanId",
                table: "TodoItem",
                newName: "IX_TodoItem_WeekPlannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItem_WeekPlanners_WeekPlannerId",
                table: "TodoItem",
                column: "WeekPlannerId",
                principalTable: "WeekPlanners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItem_WeekPlanners_WeekPlannerId",
                table: "TodoItem");

            migrationBuilder.RenameColumn(
                name: "WeekPlannerId",
                table: "TodoItem",
                newName: "LessonPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_TodoItem_WeekPlannerId",
                table: "TodoItem",
                newName: "IX_TodoItem_LessonPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItem_LessonPlans_LessonPlanId",
                table: "TodoItem",
                column: "LessonPlanId",
                principalTable: "LessonPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
