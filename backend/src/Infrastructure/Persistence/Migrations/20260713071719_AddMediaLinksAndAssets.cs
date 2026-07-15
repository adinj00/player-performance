using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaLinksAndAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_match_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unlinked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unlinked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_match_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_match_links_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_links_media_items_media_item_id",
                        column: x => x.media_item_id,
                        principalTable: "media_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_links_staff_users_linked_by_user_id",
                        column: x => x.linked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_links_staff_users_unlinked_by_user_id",
                        column: x => x.unlinked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "media_match_report_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unlinked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unlinked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_match_report_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_match_report_links_match_reports_match_report_id",
                        column: x => x.match_report_id,
                        principalTable: "match_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_report_links_media_items_media_item_id",
                        column: x => x.media_item_id,
                        principalTable: "media_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_report_links_staff_users_linked_by_user_id",
                        column: x => x.linked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_match_report_links_staff_users_unlinked_by_user_id",
                        column: x => x.unlinked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "media_player_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unlinked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unlinked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_player_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_player_links_media_items_media_item_id",
                        column: x => x.media_item_id,
                        principalTable: "media_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_player_links_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_player_links_staff_users_linked_by_user_id",
                        column: x => x.linked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_media_player_links_staff_users_unlinked_by_user_id",
                        column: x => x.unlinked_by_user_id,
                        principalTable: "staff_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_match_links_linked_by_user_id",
                table: "media_match_links",
                column: "linked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_match_links_match_id_unlinked_at_utc",
                table: "media_match_links",
                columns: new[] { "match_id", "unlinked_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_media_match_links_media_item_id_match_id_unlinked_at_utc",
                table: "media_match_links",
                columns: new[] { "media_item_id", "match_id", "unlinked_at_utc" },
                unique: true,
                filter: "unlinked_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_media_match_links_unlinked_by_user_id",
                table: "media_match_links",
                column: "unlinked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_match_report_links_linked_by_user_id",
                table: "media_match_report_links",
                column: "linked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_match_report_links_match_report_id_unlinked_at_utc",
                table: "media_match_report_links",
                columns: new[] { "match_report_id", "unlinked_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_media_match_report_links_media_item_id_match_report_id_unli~",
                table: "media_match_report_links",
                columns: new[] { "media_item_id", "match_report_id", "unlinked_at_utc" },
                unique: true,
                filter: "unlinked_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_media_match_report_links_unlinked_by_user_id",
                table: "media_match_report_links",
                column: "unlinked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_player_links_linked_by_user_id",
                table: "media_player_links",
                column: "linked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_player_links_media_item_id_player_id_unlinked_at_utc",
                table: "media_player_links",
                columns: new[] { "media_item_id", "player_id", "unlinked_at_utc" },
                unique: true,
                filter: "unlinked_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_media_player_links_player_id_unlinked_at_utc",
                table: "media_player_links",
                columns: new[] { "player_id", "unlinked_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_media_player_links_unlinked_by_user_id",
                table: "media_player_links",
                column: "unlinked_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_match_links");

            migrationBuilder.DropTable(
                name: "media_match_report_links");

            migrationBuilder.DropTable(
                name: "media_player_links");
        }
    }
}
