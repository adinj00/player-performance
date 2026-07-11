using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairVenuesAndOpponentsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some existing development databases record the original Unit 25
            // migration without its venue/opponent schema. The guards keep this
            // repair harmless for databases where that migration completed.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS opponents (
                    "Id" uuid NOT NULL,
                    name character varying(120) NOT NULL,
                    normalized_name character varying(120) NOT NULL,
                    is_archived boolean NOT NULL DEFAULT FALSE,
                    created_at_utc timestamp with time zone NOT NULL,
                    updated_at_utc timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_opponents" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_opponents_normalized_name"
                    ON opponents (normalized_name);
                CREATE TABLE IF NOT EXISTS venues (
                    "Id" uuid NOT NULL,
                    name character varying(120) NOT NULL,
                    normalized_name character varying(120) NOT NULL,
                    is_archived boolean NOT NULL DEFAULT FALSE,
                    created_at_utc timestamp with time zone NOT NULL,
                    updated_at_utc timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_venues" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_venues_normalized_name"
                    ON venues (normalized_name);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not drop tables that may have been created by the original
            // Unit 25 migration.
        }
    }
}
