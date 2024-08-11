using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class reviewDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "Cards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ReviewInterval",
                table: "Cards",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ReviewInterval",
                table: "Cards");
        }
    }
}
