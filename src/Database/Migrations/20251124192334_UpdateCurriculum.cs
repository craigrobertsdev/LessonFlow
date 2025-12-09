using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssociatedStrands",
                table: "Resources");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Resources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConceptualOrganiserResource",
                columns: table => new
                {
                    AssociatedTopicsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptualOrganiserResource", x => new { x.AssociatedTopicsId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_ConceptualOrganiserResource_ConceptualOrganisers_Associated~",
                        column: x => x.AssociatedTopicsId,
                        principalTable: "ConceptualOrganisers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptualOrganiserResource_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptualOrganiserResource_ResourceId",
                table: "ConceptualOrganiserResource",
                column: "ResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptualOrganiserResource");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Resources");

            migrationBuilder.AddColumn<string>(
                name: "AssociatedTopics",
                table: "Resources",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
