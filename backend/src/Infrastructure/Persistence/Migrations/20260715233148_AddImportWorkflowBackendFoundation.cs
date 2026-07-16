using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportWorkflowBackendFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "import_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stored_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_system = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    source_label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    file_format = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    configuration_revision = table.Column<int>(type: "integer", nullable: false),
                    validated_configuration_revision = table.Column<int>(type: "integer", nullable: true),
                    validated_processor_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    validated_processor_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    validated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preview_generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    validation_completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_row_count = table.Column<int>(type: "integer", nullable: true),
                    preview_row_count = table.Column<int>(type: "integer", nullable: true),
                    valid_row_count = table.Column<int>(type: "integer", nullable: true),
                    invalid_row_count = table.Column<int>(type: "integer", nullable: true),
                    warning_count = table.Column<int>(type: "integer", nullable: true),
                    failure_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    failure_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    result_summary_json = table.Column<string>(type: "jsonb", nullable: false),
                    processing_operation = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    processing_lease_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processing_started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processing_requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processing_processor_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    processing_processor_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    confirmed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_jobs_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_staff_users_cancelled_by_user_id",
                        column: x => x.cancelled_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_staff_users_confirmed_by_user_id",
                        column: x => x.confirmed_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_staff_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_staff_users_processing_requested_by_user_id",
                        column: x => x.processing_requested_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_stored_files_stored_file_id",
                        column: x => x.stored_file_id,
                        principalTable: "stored_files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_jobs_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_preview_columns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    source_header = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    normalized_header = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    detected_data_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_preview_columns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_preview_columns_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_preview_rows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_row_number = table.Column<int>(type: "integer", nullable: false),
                    values_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_preview_rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_preview_rows_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_validation_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    source_row_number = table.Column<int>(type: "integer", nullable: true),
                    column_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_validation_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_validation_issues_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_cancelled_by_user_id",
                table: "import_jobs",
                column: "cancelled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_confirmed_by_user_id",
                table: "import_jobs",
                column: "confirmed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_created_by_user_id_created_at_utc",
                table: "import_jobs",
                columns: new[] { "created_by_user_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_import_type_source_system_created_at_utc",
                table: "import_jobs",
                columns: new[] { "import_type", "source_system", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_match_id_created_at_utc",
                table: "import_jobs",
                columns: new[] { "match_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_processing_requested_by_user_id",
                table: "import_jobs",
                column: "processing_requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_stored_file_id",
                table: "import_jobs",
                column: "stored_file_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_jobs_team_id_status_created_at_utc",
                table: "import_jobs",
                columns: new[] { "team_id", "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_import_preview_columns_import_job_id_ordinal",
                table: "import_preview_columns",
                columns: new[] { "import_job_id", "ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_preview_rows_import_job_id_source_row_number",
                table: "import_preview_rows",
                columns: new[] { "import_job_id", "source_row_number" });

            migrationBuilder.CreateIndex(
                name: "IX_import_validation_issues_import_job_id_severity_source_row_~",
                table: "import_validation_issues",
                columns: new[] { "import_job_id", "severity", "source_row_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_preview_columns");

            migrationBuilder.DropTable(
                name: "import_preview_rows");

            migrationBuilder.DropTable(
                name: "import_validation_issues");

            migrationBuilder.DropTable(
                name: "import_jobs");
        }
    }
}
