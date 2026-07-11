using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTeamAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_team_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_team_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_team_assignments_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_team_assignments_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_player_team_assignments_player_id_start_date_end_date_Id",
                table: "player_team_assignments",
                columns: new[] { "player_id", "start_date", "end_date", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_player_team_assignments_player_id_team_id",
                table: "player_team_assignments",
                columns: new[] { "player_id", "team_id" },
                unique: true,
                filter: "end_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_player_team_assignments_player_id_team_id_start_date_end_da~",
                table: "player_team_assignments",
                columns: new[] { "player_id", "team_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_player_team_assignments_team_id_start_date_end_date_player_~",
                table: "player_team_assignments",
                columns: new[] { "team_id", "start_date", "end_date", "player_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_team_assignments");
        }
    }
}
