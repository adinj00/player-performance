using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations;

[Migration("20260711110000_AddVenuesAndOpponents")]
public partial class AddVenuesAndOpponents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "opponents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_opponents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "venues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_venues", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_opponents_normalized_name", table: "opponents", column: "normalized_name", unique: true);
        migrationBuilder.CreateIndex(name: "IX_venues_normalized_name", table: "venues", column: "normalized_name", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "opponents");
        migrationBuilder.DropTable(name: "venues");
    }
}
