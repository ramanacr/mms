using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingScheduler.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MicrosoftTenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CustomDomain = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetingRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Amenities = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExchangeEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRooms_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    OrganizerEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Attendees = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    GraphSeriesId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RecurrenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSeries_MeetingRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "MeetingRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookingInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    OrganizerEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Attendees = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GraphInstanceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingInstances_BookingSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "BookingSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingInstances_MeetingRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "MeetingRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingInstances_RoomId",
                table: "BookingInstances",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingInstances_SeriesId",
                table: "BookingInstances",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingInstances_TenantId_RoomId_StartAt_EndAt",
                table: "BookingInstances",
                columns: new[] { "TenantId", "RoomId", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeries_RoomId",
                table: "BookingSeries",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRooms_TenantId_Name",
                table: "MeetingRooms",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_MicrosoftTenantId",
                table: "Tenants",
                column: "MicrosoftTenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingInstances");

            migrationBuilder.DropTable(
                name: "BookingSeries");

            migrationBuilder.DropTable(
                name: "MeetingRooms");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
