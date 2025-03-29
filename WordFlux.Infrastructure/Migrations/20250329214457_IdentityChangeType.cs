using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class IdentityChangeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserTokens\" DROP CONSTRAINT IF EXISTS \"FK_AspNetUserTokens_AspNetUsers_UserId\"");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserRoles\" DROP CONSTRAINT IF EXISTS \"FK_AspNetUserRoles_AspNetUsers_UserId\"");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserRoles\" DROP CONSTRAINT IF EXISTS \"FK_AspNetUserRoles_AspNetRoles_RoleId\"");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserLogins\" DROP CONSTRAINT IF EXISTS \"FK_AspNetUserLogins_AspNetUsers_UserId\"");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserClaims\" DROP CONSTRAINT IF EXISTS \"FK_AspNetUserClaims_AspNetUsers_UserId\"");
            migrationBuilder.Sql("ALTER TABLE \"AspNetRoleClaims\" DROP CONSTRAINT IF EXISTS \"FK_AspNetRoleClaims_AspNetRoles_RoleId\"");

            // Drop the specific FK causing the issue
            migrationBuilder.Sql("ALTER TABLE \"AspNetRoles\" DROP CONSTRAINT IF EXISTS \"FK_AspNetRoles_AspNetUsers_AppUserId\"");
            migrationBuilder.Sql("ALTER TABLE \"Decks\" DROP CONSTRAINT IF EXISTS \"FK_Decks_AspNetUsers_UserId\"");

            // Drop index on Decks.UserId
            migrationBuilder.DropIndex(
                name: "IX_Decks_UserId",
                table: "Decks");

            // Alter columns to UUID
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"Id\" TYPE uuid USING \"Id\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetRoles\" ALTER COLUMN \"Id\" TYPE uuid USING \"Id\"::uuid");

            migrationBuilder.Sql("ALTER TABLE \"AspNetUserTokens\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserRoles\" ALTER COLUMN \"RoleId\" TYPE uuid USING \"RoleId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserLogins\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserClaims\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"AspNetRoles\" ALTER COLUMN \"AppUserId\" TYPE uuid USING \"AppUserId\"::uuid"); // Simplified conversion
            migrationBuilder.Sql("ALTER TABLE \"AspNetRoleClaims\" ALTER COLUMN \"RoleId\" TYPE uuid USING \"RoleId\"::uuid");
            migrationBuilder.Sql("ALTER TABLE \"Decks\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid");

            // Recreate all foreign keys including the missing one
            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetUserTokens\" ADD CONSTRAINT \"FK_AspNetUserTokens_AspNetUsers_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetUserRoles\" ADD CONSTRAINT \"FK_AspNetUserRoles_AspNetUsers_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetUserRoles\" ADD CONSTRAINT \"FK_AspNetUserRoles_AspNetRoles_RoleId\" FOREIGN KEY (\"RoleId\") REFERENCES \"AspNetRoles\" (\"Id\") ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetUserLogins\" ADD CONSTRAINT \"FK_AspNetUserLogins_AspNetUsers_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetUserClaims\" ADD CONSTRAINT \"FK_AspNetUserClaims_AspNetUsers_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetRoleClaims\" ADD CONSTRAINT \"FK_AspNetRoleClaims_AspNetRoles_RoleId\" FOREIGN KEY (\"RoleId\") REFERENCES \"AspNetRoles\" (\"Id\") ON DELETE CASCADE");

            // Add the missing foreign key for AppUserId in AspNetRoles
            migrationBuilder.Sql(
                "ALTER TABLE \"AspNetRoles\" ADD CONSTRAINT \"FK_AspNetRoles_AspNetUsers_AppUserId\" FOREIGN KEY (\"AppUserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE RESTRICT");        
            
            migrationBuilder.Sql(
                "ALTER TABLE \"Decks\" ADD CONSTRAINT \"FK_Decks_AspNetUsers_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE");
            
            migrationBuilder.CreateIndex(
                name: "IX_Decks_UserId",
                table: "Decks",
                column: "UserId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}