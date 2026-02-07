using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PianificazioneTurni.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dipendenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Patente = table.Column<bool>(type: "bit", nullable: false),
                    Scadenza = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dipendenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Navi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Pontile = table.Column<int>(type: "int", nullable: true),
                    DataArrivo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrarioArrivo = table.Column<int>(type: "int", nullable: false),
                    DataPartenza = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrarioPartenza = table.Column<int>(type: "int", nullable: false),
                    RichiedeGruisti = table.Column<bool>(type: "bit", nullable: false),
                    RichiedeMulettisti = table.Column<bool>(type: "bit", nullable: false),
                    RichiedeAddettiTerminal = table.Column<bool>(type: "bit", nullable: false),
                    RichiedeOrmeggiatori = table.Column<bool>(type: "bit", nullable: false),
                    RichiedeAddettiSicurezza = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assegnazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NaveId = table.Column<int>(type: "int", nullable: false),
                    DipendenteId = table.Column<int>(type: "int", nullable: false),
                    Fascia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assegnazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assegnazioni_Dipendenti_DipendenteId",
                        column: x => x.DipendenteId,
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assegnazioni_Navi_NaveId",
                        column: x => x.NaveId,
                        principalTable: "Navi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assegnazioni_DipendenteId",
                table: "Assegnazioni",
                column: "DipendenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Assegnazioni_NaveId",
                table: "Assegnazioni",
                column: "NaveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assegnazioni");

            migrationBuilder.DropTable(
                name: "Dipendenti");

            migrationBuilder.DropTable(
                name: "Navi");
        }
    }
}
