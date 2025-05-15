using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howbot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAndUpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Guilds",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "GuildUser",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildUser_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildUser_GuildId",
                table: "GuildUser",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildUser");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Guilds",
                newName: "EncryptedSessionId");
        }
    }
}
