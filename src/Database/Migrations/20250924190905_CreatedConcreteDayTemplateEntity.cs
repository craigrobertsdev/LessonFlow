using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreatedConcreteDayTemplateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Periods",
                table: "DayTemplates");

            migrationBuilder.CreateTable(
                name: "BreakTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    StartPeriod = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    DayTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    BreakDuty = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakPeriod_DayTemplates_DayTemplateId",
                        column: x => x.DayTemplateId,
                        principalTable: "DayTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    StartPeriod = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    DayTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubjectName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPeriod_DayTemplates_DayTemplateId",
                        column: x => x.DayTemplateId,
                        principalTable: "DayTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NitTemplatePeriod",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    StartPeriod = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    DayTemplateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NitPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NitPeriod_DayTemplates_DayTemplateId",
                        column: x => x.DayTemplateId,
                        principalTable: "DayTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreakPeriod_DayTemplateId",
                table: "BreakTemplate",
                column: "DayTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPeriod_DayTemplateId",
                table: "LessonTemplate",
                column: "DayTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NitPeriod_DayTemplateId",
                table: "NitTemplatePeriod",
                column: "DayTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreakTemplate");

            migrationBuilder.DropTable(
                name: "LessonTemplate");

            migrationBuilder.DropTable(
                name: "NitTemplatePeriod");

            migrationBuilder.AddColumn<string>(
                name: "Periods",
                table: "DayTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
