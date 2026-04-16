using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BierDex.Migrations
{
    /// <inheritdoc />
    public partial class abv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "abv",
                table: "Beers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "abv",
                table: "Beers");
        }
    }
}
