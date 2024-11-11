using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddExampleTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExampleTasks",
                table: "Cards",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExampleTasks",
                table: "Cards");
        }
    }
}
