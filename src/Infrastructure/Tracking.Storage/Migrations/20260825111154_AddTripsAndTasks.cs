using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracking.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddTripsAndTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TripNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DriverVehicleAssignmentId = table.Column<long>(type: "INTEGER", nullable: false),
                    DriverId = table.Column<long>(type: "INTEGER", nullable: false),
                    VehicleId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartLocation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EndLocation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ScheduledStartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledEndAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualStartAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_DriverVehicleAssignments_DriverVehicleAssignmentId",
                        column: x => x.DriverVehicleAssignmentId,
                        principalTable: "DriverVehicleAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TripId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ScheduledAt",
                table: "Tasks",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TripId_Sequence",
                table: "Tasks",
                columns: new[] { "TripId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverId",
                table: "Trips",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverVehicleAssignmentId",
                table: "Trips",
                column: "DriverVehicleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ScheduledStartAt",
                table: "Trips",
                column: "ScheduledStartAt");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Status",
                table: "Trips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TripNumber",
                table: "Trips",
                column: "TripNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_VehicleId",
                table: "Trips",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
