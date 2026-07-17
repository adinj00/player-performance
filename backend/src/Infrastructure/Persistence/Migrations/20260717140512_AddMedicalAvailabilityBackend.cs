using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalAvailabilityBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "injury_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    current_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_on = table.Column<DateOnly>(type: "date", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_injury_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_injury_records_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_injury_records_staff_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_injury_records_staff_users_resolved_by_user_id",
                        column: x => x.resolved_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_injury_records_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_availabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_availabilities_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_availabilities_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "injury_record_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    injury_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    body_area = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    diagnosis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    restricted_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_injury_record_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_injury_record_revisions_injury_records_injury_record_id",
                        column: x => x.injury_record_id,
                        principalTable: "injury_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_injury_record_revisions_staff_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_availability_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_availability_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    effective_on = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_return_on = table.Column<DateOnly>(type: "date", nullable: true),
                    coach_visible_note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_availability_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_player_availability_revisions_player_availabilities_player_~",
                        column: x => x.player_availability_id,
                        principalTable: "player_availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_availability_revisions_staff_users_recorded_by_user_~",
                        column: x => x.recorded_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_injury_record_revisions_injury_record_id_revision_number",
                table: "injury_record_revisions",
                columns: new[] { "injury_record_id", "revision_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_injury_record_revisions_recorded_at_utc",
                table: "injury_record_revisions",
                column: "recorded_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_injury_record_revisions_recorded_by_user_id",
                table: "injury_record_revisions",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_injury_records_created_by_user_id",
                table: "injury_records",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_injury_records_player_id_occurred_on",
                table: "injury_records",
                columns: new[] { "player_id", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "IX_injury_records_resolved_by_user_id",
                table: "injury_records",
                column: "resolved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_injury_records_team_id_status_occurred_on",
                table: "injury_records",
                columns: new[] { "team_id", "status", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "IX_player_availabilities_player_id_team_id",
                table: "player_availabilities",
                columns: new[] { "player_id", "team_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_availabilities_team_id",
                table: "player_availabilities",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_availability_revisions_player_availability_id_revisi~",
                table: "player_availability_revisions",
                columns: new[] { "player_availability_id", "revision_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_availability_revisions_recorded_at_utc",
                table: "player_availability_revisions",
                column: "recorded_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_player_availability_revisions_recorded_by_user_id",
                table: "player_availability_revisions",
                column: "recorded_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "injury_record_revisions");

            migrationBuilder.DropTable(
                name: "player_availability_revisions");

            migrationBuilder.DropTable(
                name: "injury_records");

            migrationBuilder.DropTable(
                name: "player_availabilities");
        }
    }
}
