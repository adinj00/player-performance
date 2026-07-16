using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingGpsBackendFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "training_session_id",
                table: "import_jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "training_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_date = table.Column<DateOnly>(type: "date", nullable: false),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_sessions_staff_users_cancelled_by_user_id",
                        column: x => x.cancelled_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_sessions_staff_users_completed_by_user_id",
                        column: x => x.completed_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_sessions_staff_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_sessions_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "training_session_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    removed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    removed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_session_participants_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_session_participants_staff_users_added_by_user_id",
                        column: x => x.added_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_session_participants_staff_users_removed_by_user_id",
                        column: x => x.removed_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_session_participants_training_sessions_training_se~",
                        column: x => x.training_session_id,
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_physical_workloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on = table.Column<DateOnly>(type: "date", nullable: false),
                    training_session_participant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    player_match_appearance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_revision_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_physical_workloads", x => x.Id);
                    table.CheckConstraint("ck_workload_exactly_one_context", "(training_session_participant_id IS NOT NULL) <> (player_match_appearance_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_player_physical_workloads_player_match_appearances_player_m~",
                        column: x => x.player_match_appearance_id,
                        principalTable: "player_match_appearances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_physical_workloads_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_physical_workloads_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_physical_workloads_training_session_participants_tra~",
                        column: x => x.training_session_participant_id,
                        principalTable: "training_session_participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "physical_workload_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_physical_workload_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    source_kind = table.Column<string>(type: "text", nullable: false),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    processor_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    processor_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_physical_workload_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_physical_workload_revisions_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_physical_workload_revisions_player_physical_workloads_playe~",
                        column: x => x.player_physical_workload_id,
                        principalTable: "player_physical_workloads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_physical_workload_revisions_staff_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "physical_metric_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_workload_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    unit_code = table.Column<string>(type: "text", nullable: false),
                    threshold_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    threshold_unit_code = table.Column<string>(type: "text", nullable: true),
                    threshold_direction = table.Column<string>(type: "text", nullable: true),
                    threshold_scope = table.Column<string>(type: "text", nullable: true),
                    method_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    method_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_physical_metric_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_physical_metric_values_physical_workload_revisions_physical~",
                        column: x => x.physical_workload_revision_id,
                        principalTable: "physical_workload_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_training_session_id",
                table: "import_jobs",
                column: "training_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_physical_metric_values_metric_code",
                table: "physical_metric_values",
                column: "metric_code");

            migrationBuilder.CreateIndex(
                name: "IX_physical_metric_values_physical_workload_revision_id_metric~",
                table: "physical_metric_values",
                columns: new[] { "physical_workload_revision_id", "metric_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_physical_workload_revisions_import_job_id",
                table: "physical_workload_revisions",
                column: "import_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_physical_workload_revisions_player_physical_workload_id_rev~",
                table: "physical_workload_revisions",
                columns: new[] { "player_physical_workload_id", "revision_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_physical_workload_revisions_recorded_by_user_id",
                table: "physical_workload_revisions",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_physical_workloads_player_id_occurred_on",
                table: "player_physical_workloads",
                columns: new[] { "player_id", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "IX_player_physical_workloads_player_match_appearance_id",
                table: "player_physical_workloads",
                column: "player_match_appearance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_physical_workloads_team_id_occurred_on",
                table: "player_physical_workloads",
                columns: new[] { "team_id", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "IX_player_physical_workloads_training_session_participant_id",
                table: "player_physical_workloads",
                column: "training_session_participant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_session_participants_added_by_user_id",
                table: "training_session_participants",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_session_participants_player_id_added_at_utc",
                table: "training_session_participants",
                columns: new[] { "player_id", "added_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_session_participants_removed_by_user_id",
                table: "training_session_participants",
                column: "removed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_session_participants_training_session_id_player_id",
                table: "training_session_participants",
                columns: new[] { "training_session_id", "player_id" },
                unique: true,
                filter: "removed_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_training_session_participants_training_session_id_removed_a~",
                table: "training_session_participants",
                columns: new[] { "training_session_id", "removed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_cancelled_by_user_id",
                table: "training_sessions",
                column: "cancelled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_completed_by_user_id",
                table: "training_sessions",
                column: "completed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_created_by_user_id",
                table: "training_sessions",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_status_session_date",
                table: "training_sessions",
                columns: new[] { "status", "session_date" });

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_team_id_session_date_status",
                table: "training_sessions",
                columns: new[] { "team_id", "session_date", "status" });

            migrationBuilder.AddForeignKey(
                name: "FK_import_jobs_training_sessions_training_session_id",
                table: "import_jobs",
                column: "training_session_id",
                principalTable: "training_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_jobs_training_sessions_training_session_id",
                table: "import_jobs");

            migrationBuilder.DropTable(
                name: "physical_metric_values");

            migrationBuilder.DropTable(
                name: "physical_workload_revisions");

            migrationBuilder.DropTable(
                name: "player_physical_workloads");

            migrationBuilder.DropTable(
                name: "training_session_participants");

            migrationBuilder.DropTable(
                name: "training_sessions");

            migrationBuilder.DropIndex(
                name: "IX_import_jobs_training_session_id",
                table: "import_jobs");

            migrationBuilder.DropColumn(
                name: "training_session_id",
                table: "import_jobs");
        }
    }
}
