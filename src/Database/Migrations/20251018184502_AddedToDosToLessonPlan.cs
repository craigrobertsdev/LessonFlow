using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedToDosToLessonPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TodoItem",
                columns: table => new
                {
                    LessonPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDoItem", x => new { x.Id, x.LessonPlanId });
                    table.ForeignKey(
                        name: "FK_ToDoItem_LessonPlans_LessonPlanId",
                        column: x => x.LessonPlanId,
                        principalTable: "LessonPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToDoItem_LessonPlanId",
                table: "TodoItem",
                column: "LessonPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoItem");
        }
    }
}
