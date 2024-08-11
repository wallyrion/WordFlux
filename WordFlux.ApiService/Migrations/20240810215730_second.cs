using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Example",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "Translation",
                table: "Cards");

            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "Cards",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Cards");

            migrationBuilder.AddColumn<string>(
                name: "Example",
                table: "Cards",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Translation",
                table: "Cards",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
