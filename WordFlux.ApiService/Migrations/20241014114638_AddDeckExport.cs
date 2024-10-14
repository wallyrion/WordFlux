using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Export",
                table: "Decks",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Export",
                table: "Decks");
        }
    }
}
