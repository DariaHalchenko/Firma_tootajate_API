using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firma_tootajate_API.Migrations
{
    /// <inheritdoc />
    public partial class addedTootajate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tootajates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Isikukood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tunnitasu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parool = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tootajates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Worktimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TootajateId = table.Column<int>(type: "int", nullable: false),
                    Kuupaev = table.Column<DateOnly>(type: "date", nullable: false),
                    Sissepaas = table.Column<TimeOnly>(type: "time", nullable: false),
                    Valjapaas = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worktimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Worktimes_Tootajates_TootajateId",
                        column: x => x.TootajateId,
                        principalTable: "Tootajates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Worktimes_TootajateId",
                table: "Worktimes",
                column: "TootajateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Worktimes");

            migrationBuilder.DropTable(
                name: "Tootajates");
        }
    }
}
