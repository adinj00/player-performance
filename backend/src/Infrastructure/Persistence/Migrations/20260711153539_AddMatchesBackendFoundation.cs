using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PlayerPerformance.Infrastructure.Persistence;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260711153539_AddMatchesBackendFoundation")]
public partial class AddMatchesBackendFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "matches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                season_id = table.Column<Guid>(type: "uuid", nullable: false),
                competition_id = table.Column<Guid>(type: "uuid", nullable: false),
                team_id = table.Column<Guid>(type: "uuid", nullable: false),
                opponent_id = table.Column<Guid>(type: "uuid", nullable: false),
                venue_id = table.Column<Guid>(type: "uuid", nullable: true),
                kickoff_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                round = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                location_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                team_score = table.Column<int>(type: "integer", nullable: true),
                opponent_score = table.Column<int>(type: "integer", nullable: true),
                is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_matches", x => x.Id);
                table.ForeignKey("FK_matches_competitions_competition_id", x => x.competition_id, "competitions", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_matches_opponents_opponent_id", x => x.opponent_id, "opponents", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_matches_seasons_season_id", x => x.season_id, "seasons", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_matches_teams_team_id", x => x.team_id, "teams", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_matches_venues_venue_id", x => x.venue_id, "venues", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_matches_competition_id", table: "matches", column: "competition_id");
        migrationBuilder.CreateIndex(name: "IX_matches_opponent_id", table: "matches", column: "opponent_id");
        migrationBuilder.CreateIndex(name: "IX_matches_season_id_kickoff_at_utc", table: "matches", columns: ["season_id", "kickoff_at_utc"]);
        migrationBuilder.CreateIndex(name: "IX_matches_status_is_archived", table: "matches", columns: ["status", "is_archived"]);
        migrationBuilder.CreateIndex(name: "IX_matches_team_id_kickoff_at_utc", table: "matches", columns: ["team_id", "kickoff_at_utc"]);
        migrationBuilder.CreateIndex(name: "IX_matches_venue_id", table: "matches", column: "venue_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "matches");
}
