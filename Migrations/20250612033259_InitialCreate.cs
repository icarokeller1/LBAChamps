using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ligas",
                columns: table => new
                {
                    IdLiga = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Esporte = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ligas", x => x.IdLiga);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Times",
                columns: table => new
                {
                    IdTime = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IdLiga = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Times", x => x.IdTime);
                    table.ForeignKey(
                        name: "FK_Times_Ligas_IdLiga",
                        column: x => x.IdLiga,
                        principalTable: "Ligas",
                        principalColumn: "IdLiga",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    IdJogador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Posicao = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NumeroCamisa = table.Column<int>(type: "int", nullable: false),
                    IdTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.IdJogador);
                    table.ForeignKey(
                        name: "FK_Jogadores_Times_IdTime",
                        column: x => x.IdTime,
                        principalTable: "Times",
                        principalColumn: "IdTime",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    IdPartida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IdTimeCasa = table.Column<int>(type: "int", nullable: false),
                    IdTimeFora = table.Column<int>(type: "int", nullable: false),
                    IdLiga = table.Column<int>(type: "int", nullable: false),
                    PlacarCasa = table.Column<int>(type: "int", nullable: false),
                    PlacarFora = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.IdPartida);
                    table.CheckConstraint("CK_Partida_TimesDiferentes", "[IdTimeCasa] <> [IdTimeFora]");
                    table.ForeignKey(
                        name: "FK_Partidas_Ligas_IdLiga",
                        column: x => x.IdLiga,
                        principalTable: "Ligas",
                        principalColumn: "IdLiga",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Partidas_Times_IdTimeCasa",
                        column: x => x.IdTimeCasa,
                        principalTable: "Times",
                        principalColumn: "IdTime",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Times_IdTimeFora",
                        column: x => x.IdTimeFora,
                        principalTable: "Times",
                        principalColumn: "IdTime",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstatisticasPartidas",
                columns: table => new
                {
                    IdEstatistica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPartida = table.Column<int>(type: "int", nullable: false),
                    IdJogador = table.Column<int>(type: "int", nullable: false),
                    Pontos = table.Column<int>(type: "int", nullable: false),
                    Rebotes = table.Column<int>(type: "int", nullable: false),
                    Assistencias = table.Column<int>(type: "int", nullable: false),
                    RoubosBola = table.Column<int>(type: "int", nullable: false),
                    Tocos = table.Column<int>(type: "int", nullable: false),
                    Faltas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstatisticasPartidas", x => x.IdEstatistica);
                    table.ForeignKey(
                        name: "FK_EstatisticasPartidas_Jogadores_IdJogador",
                        column: x => x.IdJogador,
                        principalTable: "Jogadores",
                        principalColumn: "IdJogador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstatisticasPartidas_Partidas_IdPartida",
                        column: x => x.IdPartida,
                        principalTable: "Partidas",
                        principalColumn: "IdPartida",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasPartidas_IdJogador",
                table: "EstatisticasPartidas",
                column: "IdJogador");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasPartidas_IdPartida_IdJogador",
                table: "EstatisticasPartidas",
                columns: new[] { "IdPartida", "IdJogador" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_IdTime",
                table: "Jogadores",
                column: "IdTime");

            migrationBuilder.CreateIndex(
                name: "IX_Ligas_Nome_Esporte",
                table: "Ligas",
                columns: new[] { "Nome", "Esporte" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_IdLiga",
                table: "Partidas",
                column: "IdLiga");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_IdTimeCasa",
                table: "Partidas",
                column: "IdTimeCasa");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_IdTimeFora",
                table: "Partidas",
                column: "IdTimeFora");

            migrationBuilder.CreateIndex(
                name: "IX_Times_IdLiga",
                table: "Times",
                column: "IdLiga");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstatisticasPartidas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.DropTable(
                name: "Times");

            migrationBuilder.DropTable(
                name: "Ligas");
        }
    }
}
