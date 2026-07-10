using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerPerformance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAccessProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staff_access_profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CanVerifyReports = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanImportData = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanViewMedicalDetails = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_access_profiles", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staff_access_profiles");
        }
    }
}
