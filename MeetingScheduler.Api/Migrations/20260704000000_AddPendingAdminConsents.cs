using System;
using MeetingScheduler.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingScheduler.Api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260704000000_AddPendingAdminConsents")]
    public partial class AddPendingAdminConsents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingAdminConsents",
                columns: table => new
                {
                    State = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpectedMicrosoftTenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RequestedByEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingAdminConsents", x => x.State);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingAdminConsents_ExpectedMicrosoftTenantId_UsedAt",
                table: "PendingAdminConsents",
                columns: new[] { "ExpectedMicrosoftTenantId", "UsedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingAdminConsents");
        }
    }
}
