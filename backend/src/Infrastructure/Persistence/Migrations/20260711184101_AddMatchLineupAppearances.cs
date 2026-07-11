using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchLineupAppearances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_lineup_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_lineup_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_lineup_entries_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_match_lineup_entries_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_lineups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    formation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    captain_player_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_lineups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_lineups_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_match_lineups_players_captain_player_id",
                        column: x => x.captain_player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_substitutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_out_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_in_id = table.Column<Guid>(type: "uuid", nullable: false),
                    minute = table.Column<int>(type: "integer", nullable: false),
                    stoppage_time_minute = table.Column<int>(type: "integer", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_substitutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_substitutions_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_match_substitutions_players_player_in_id",
                        column: x => x.player_in_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_match_substitutions_players_player_out_id",
                        column: x => x.player_out_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_match_appearances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    minutes_played = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_match_appearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_match_appearances_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_match_appearances_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_lineup_entries_match_id_player_id",
                table: "match_lineup_entries",
                columns: new[] { "match_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_lineup_entries_player_id",
                table: "match_lineup_entries",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_match_lineups_captain_player_id",
                table: "match_lineups",
                column: "captain_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_match_lineups_match_id",
                table: "match_lineups",
                column: "match_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_substitutions_match_id_sequence",
                table: "match_substitutions",
                columns: new[] { "match_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_substitutions_player_in_id",
                table: "match_substitutions",
                column: "player_in_id");

            migrationBuilder.CreateIndex(
                name: "IX_match_substitutions_player_out_id",
                table: "match_substitutions",
                column: "player_out_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_match_appearances_match_id_player_id",
                table: "player_match_appearances",
                columns: new[] { "match_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_match_appearances_player_id",
                table: "player_match_appearances",
                column: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_lineup_entries");

            migrationBuilder.DropTable(
                name: "match_lineups");

            migrationBuilder.DropTable(
                name: "match_substitutions");

            migrationBuilder.DropTable(
                name: "player_match_appearances");
        }
    }
}
