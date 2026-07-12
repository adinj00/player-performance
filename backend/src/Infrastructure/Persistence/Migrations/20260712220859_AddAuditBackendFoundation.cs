using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditBackendFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    previous_values = table.Column<string>(type: "jsonb", nullable: false),
                    new_values = table.Column<string>(type: "jsonb", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_staff_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "action", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_actor_user_id_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "actor_user_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_type_entity_id_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
