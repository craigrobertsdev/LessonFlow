using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonFlow.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnsAccountSetupState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "AccountSetupState");

            migrationBuilder.DropColumn(
                name: "IsLoading",
                table: "AccountSetupState");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectsTaught",
                table: "AccountSetupState",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SubjectsTaught",
                table: "AccountSetupState",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "AccountSetupState",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLoading",
                table: "AccountSetupState",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
