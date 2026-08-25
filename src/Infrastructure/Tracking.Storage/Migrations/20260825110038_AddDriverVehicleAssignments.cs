using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracking.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverVehicleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverVehicleAssignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriverId = table.Column<long>(type: "INTEGER", nullable: false),
                    VehicleId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverVehicleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverVehicleAssignments_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverVehicleAssignments_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverVehicleAssignments_DriverId_StartAt",
                table: "DriverVehicleAssignments",
                columns: new[] { "DriverId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverVehicleAssignments_VehicleId_StartAt",
                table: "DriverVehicleAssignments",
                columns: new[] { "VehicleId", "StartAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverVehicleAssignments");
        }
    }
}
