using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LearnLanguage",
                table: "Cards",
                type: "character varying(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NativeLanguage",
                table: "Cards",
                type: "character varying(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceLanguage",
                table: "Cards",
                type: "character varying(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetLanguage",
                table: "Cards",
                type: "character varying(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearnLanguage",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "NativeLanguage",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "SourceLanguage",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "TargetLanguage",
                table: "Cards");
        }
    }
}
