using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRenting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufactured_on = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rented_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.id);
                    table.CheckConstraint("ck_rentals_returned_at", "returned_at IS NULL OR returned_at >= rented_at");
                    table.ForeignKey(
                        name: "FK_rentals_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_rentals_active_customer",
                table: "rentals",
                column: "customer_id",
                unique: true,
                filter: "returned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_rentals_active_vehicle",
                table: "rentals",
                column: "vehicle_id",
                unique: true,
                filter: "returned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_license_plate",
                table: "vehicles",
                column: "license_plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DropTable(
                name: "rentals");

            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
