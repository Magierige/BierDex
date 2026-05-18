using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BierDex.Migrations
{
    /// <inheritdoc />
    public partial class slugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "Beers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Beers_slug",
                table: "Beers",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beers_slug",
                table: "Beers");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "Beers");
        }
    }
}
