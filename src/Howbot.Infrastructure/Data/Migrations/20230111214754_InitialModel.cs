using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Howbot.Infrastructure.Data.Migrations
{
  /// <inheritdoc />
  public partial class InitialModel : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "UrlStatusHistories",
          columns: table => new
          {
            Id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            Uri = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
            RequestDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            StatusCode = table.Column<int>(type: "integer", nullable: false),
            RequestId = table.Column<string>(type: "text", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UrlStatusHistories", x => x.Id);
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "UrlStatusHistories");
    }
  }
}
