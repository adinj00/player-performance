using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchReportWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_correction_requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_correction_requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_correction_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    archived_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_reports_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_reports_match_id",
                table: "match_reports",
                column: "match_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_reports_status",
                table: "match_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_match_reports_status_updated_at_utc",
                table: "match_reports",
                columns: new[] { "status", "updated_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_reports");
        }
    }
}
