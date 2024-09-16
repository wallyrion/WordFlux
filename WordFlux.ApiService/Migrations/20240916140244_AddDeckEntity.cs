using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeckId",
                table: "Cards",
                type: "uuid",
                nullable: true);

            
            migrationBuilder.CreateTable(
                name: "Deck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deck_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.Sql(
                """
                  DELETE FROM "Cards" WHERE "CreatedBy" NOT IN (SELECT "Id"::uuid FROM "AspNetUsers");
                """);
            
            migrationBuilder.Sql(
            """
                DO $$
                 DECLARE
                     user_record RECORD;
                     new_deck_id UUID;
                 BEGIN
                     FOR user_record IN SELECT DISTINCT "CreatedBy" FROM "Cards" LOOP
                         new_deck_id := gen_random_uuid();  -- Use gen_random_uuid() instead of uuid_generate_v4()
                         
                         INSERT INTO "Deck" ("Id", "CreatedAt", "UserId", "Name", "Type")
                         VALUES (new_deck_id, NOW(), user_record."CreatedBy"::text, 'Default', 0);
                         
                         UPDATE "Cards"
                         SET "DeckId" = new_deck_id
                         WHERE "CreatedBy" = user_record."CreatedBy";
                     END LOOP;
                 END;
             $$;
             """);
            
            migrationBuilder.AlterColumn<Guid>(
                name: "DeckId",
                table: "Cards",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
            
            migrationBuilder.CreateIndex(
                name: "IX_Cards_DeckId",
                table: "Cards",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Deck_UserId",
                table: "Deck",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Deck_DeckId",
                table: "Cards",
                column: "DeckId",
                principalTable: "Deck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Deck_DeckId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "Deck");

            migrationBuilder.DropIndex(
                name: "IX_Cards_DeckId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "Cards");
        }
    }
}
