using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSubjectToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSetupScheduleConfig_ScheduleSlots_CurriculumSubjects~",
                table: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountSetupScheduleGrid_ScheduleSlots_CurriculumSubjects_S~",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_AccountSetupScheduleGrid_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_AccountSetupScheduleConfig_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "AccountSetupScheduleGrid_ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "AccountSetupScheduleConfig_ScheduleSlots");

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleGrid_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSetupScheduleConfig_ScheduleSlots_SubjectId",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSetupScheduleConfig_ScheduleSlots_CurriculumSubjects~",
                table: "AccountSetupScheduleConfig_ScheduleSlots",
                column: "SubjectId",
                principalTable: "CurriculumSubjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSetupScheduleGrid_ScheduleSlots_CurriculumSubjects_S~",
                table: "AccountSetupScheduleGrid_ScheduleSlots",
                column: "SubjectId",
                principalTable: "CurriculumSubjects",
                principalColumn: "Id");
        }
    }
}
