using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Motix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SECTORS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SECTORS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MOTORCYCLES",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Plate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SectorId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOTORCYCLES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MOTORCYCLES_SECTORS_SectorId",
                        column: x => x.SectorId,
                        principalTable: "SECTORS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MOVEMENTS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    MotorcycleId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SectorId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOVEMENTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MOVEMENTS_MOTORCYCLES_MotorcycleId",
                        column: x => x.MotorcycleId,
                        principalTable: "MOTORCYCLES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOVEMENTS_SECTORS_SectorId",
                        column: x => x.SectorId,
                        principalTable: "SECTORS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOTORCYCLES_SectorId",
                table: "MOTORCYCLES",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_MOVEMENTS_MotorcycleId",
                table: "MOVEMENTS",
                column: "MotorcycleId");

            migrationBuilder.CreateIndex(
                name: "IX_MOVEMENTS_SectorId",
                table: "MOVEMENTS",
                column: "SectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOVEMENTS");

            migrationBuilder.DropTable(
                name: "MOTORCYCLES");

            migrationBuilder.DropTable(
                name: "SECTORS");
        }
    }
}
