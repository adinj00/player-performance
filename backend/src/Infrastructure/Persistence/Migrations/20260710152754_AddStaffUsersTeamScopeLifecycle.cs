using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffUsersTeamScopeLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "staff_access_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE staff_access_profiles AS profile
                SET display_name = user_record."Email"
                FROM staff_users AS user_record
                WHERE profile."UserId" = user_record."Id" AND profile.display_name = '';
                """);

            migrationBuilder.AddColumn<string>(
                name: "team_scope_type",
                table: "staff_access_profiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "ALL_TEAMS");

            migrationBuilder.CreateTable(
                name: "staff_team_scopes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_team_scopes", x => new { x.UserId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_staff_team_scopes_staff_access_profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "staff_access_profiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_team_scopes_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staff_team_scopes_TeamId",
                table: "staff_team_scopes",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staff_team_scopes");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "staff_access_profiles");

            migrationBuilder.DropColumn(
                name: "team_scope_type",
                table: "staff_access_profiles");
        }
    }
}
