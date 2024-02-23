using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howbot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingSearchProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SearchProvider",
                table: "Guilds",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchProvider",
                table: "Guilds");
        }
    }
}
