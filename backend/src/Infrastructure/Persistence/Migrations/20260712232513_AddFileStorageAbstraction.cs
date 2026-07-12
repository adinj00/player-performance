using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageAbstraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stored_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_files", x => x.Id);
                    table.CheckConstraint("CK_stored_files_archive_metadata_consistent", "(is_archived = FALSE AND archived_at_utc IS NULL AND archived_by_user_id IS NULL) OR (is_archived = TRUE AND archived_at_utc IS NOT NULL AND archived_by_user_id IS NOT NULL)");
                    table.CheckConstraint("CK_stored_files_size_bytes_positive", "size_bytes > 0");
                    table.ForeignKey(
                        name: "FK_stored_files_staff_users_archived_by_user_id",
                        column: x => x.archived_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stored_files_staff_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_archived_by_user_id",
                table: "stored_files",
                column: "archived_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_is_archived_archived_at_utc",
                table: "stored_files",
                columns: new[] { "is_archived", "archived_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_storage_key",
                table: "stored_files",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_uploaded_by_user_id_created_at_utc",
                table: "stored_files",
                columns: new[] { "uploaded_by_user_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stored_files");
        }
    }
}
