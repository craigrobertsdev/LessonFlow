using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExplicitForeignKeyYearPlanToWeekPlannerTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_WeekPlannerTemplates_YearPlans_YearPlanId",
            //    table: "WeekPlannerTemplates");

            //migrationBuilder.DropIndex(
            //    name: "IX_WeekPlannerTemplates_YearPlanId",
            //    table: "WeekPlannerTemplates");

            //migrationBuilder.DropColumn(
            //    name: "YearPlanId",
            //    table: "WeekPlannerTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "WeekPlannerTemplateId",
                table: "YearPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_YearPlans_WeekPlannerTemplateId",
                table: "YearPlans",
                column: "WeekPlannerTemplateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YearPlans_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "YearPlans",
                column: "WeekPlannerTemplateId",
                principalTable: "WeekPlannerTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YearPlans_WeekPlannerTemplates_WeekPlannerTemplateId",
                table: "YearPlans");

            migrationBuilder.DropIndex(
                name: "IX_YearPlans_WeekPlannerTemplateId",
                table: "YearPlans");

            migrationBuilder.DropColumn(
                name: "WeekPlannerTemplateId",
                table: "YearPlans");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "YearPlanId",
            //    table: "WeekPlannerTemplates",
            //    type: "uuid",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_WeekPlannerTemplates_YearPlanId",
            //    table: "WeekPlannerTemplates",
            //    column: "YearPlanId",
            //    unique: true);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WeekPlannerTemplates_YearPlans_YearPlanId",
            //    table: "WeekPlannerTemplates",
            //    column: "YearPlanId",
            //    principalTable: "YearPlans",
            //    principalColumn: "Id");
        }
    }
}
