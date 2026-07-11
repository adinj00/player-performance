using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManualMatchStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "applied_tracking_level",
                table: "match_reports",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "goalkeeper_match_stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_match_appearance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saves = table.Column<int>(type: "integer", nullable: true),
                    goals_conceded = table.Column<int>(type: "integer", nullable: true),
                    clean_sheet = table.Column<bool>(type: "boolean", nullable: true),
                    penalty_saves = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goalkeeper_match_stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_goalkeeper_match_stats_match_reports_match_report_id",
                        column: x => x.match_report_id,
                        principalTable: "match_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_goalkeeper_match_stats_player_match_appearances_player_matc~",
                        column: x => x.player_match_appearance_id,
                        principalTable: "player_match_appearances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_match_stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_match_appearance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goals = table.Column<int>(type: "integer", nullable: true),
                    assists = table.Column<int>(type: "integer", nullable: true),
                    yellow_cards = table.Column<int>(type: "integer", nullable: true),
                    red_cards = table.Column<int>(type: "integer", nullable: true),
                    shots = table.Column<int>(type: "integer", nullable: true),
                    shots_on_target = table.Column<int>(type: "integer", nullable: true),
                    passes_attempted = table.Column<int>(type: "integer", nullable: true),
                    passes_completed = table.Column<int>(type: "integer", nullable: true),
                    key_passes = table.Column<int>(type: "integer", nullable: true),
                    duels_attempted = table.Column<int>(type: "integer", nullable: true),
                    duels_won = table.Column<int>(type: "integer", nullable: true),
                    fouls_committed = table.Column<int>(type: "integer", nullable: true),
                    fouls_won = table.Column<int>(type: "integer", nullable: true),
                    offsides = table.Column<int>(type: "integer", nullable: true),
                    ball_recoveries = table.Column<int>(type: "integer", nullable: true),
                    possession_losses = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_match_stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_match_stats_match_reports_match_report_id",
                        column: x => x.match_report_id,
                        principalTable: "match_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_match_stats_player_match_appearances_player_match_ap~",
                        column: x => x.player_match_appearance_id,
                        principalTable: "player_match_appearances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goalkeeper_match_stats_match_report_id",
                table: "goalkeeper_match_stats",
                column: "match_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalkeeper_match_stats_player_match_appearance_id",
                table: "goalkeeper_match_stats",
                column: "player_match_appearance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_match_stats_match_report_id",
                table: "player_match_stats",
                column: "match_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_match_stats_player_match_appearance_id",
                table: "player_match_stats",
                column: "player_match_appearance_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goalkeeper_match_stats");

            migrationBuilder.DropTable(
                name: "player_match_stats");

            migrationBuilder.DropColumn(
                name: "applied_tracking_level",
                table: "match_reports");
        }
    }
}
