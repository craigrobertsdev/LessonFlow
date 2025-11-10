using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeleteBehaviousFromAccountSetupStateToWeekPlannerTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSetupState_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "AccountSetupState");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSetupState_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "AccountSetupState",
                column: "WeekPlannerTemplateId",
                principalTable: "WeekPlannerTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSetupState_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "AccountSetupState");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSetupState_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "AccountSetupState",
                column: "WeekPlannerTemplateId",
                principalTable: "WeekPlannerTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
