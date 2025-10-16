using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBeforeAndAfterSchoolDutiesToDayTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AfterSchoolDuty",
                table: "DayTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeSchoolDuty",
                table: "DayTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterSchoolDuty",
                table: "DayTemplates");

            migrationBuilder.DropColumn(
                name: "BeforeSchoolDuty",
                table: "DayTemplates");
        }
    }
}
