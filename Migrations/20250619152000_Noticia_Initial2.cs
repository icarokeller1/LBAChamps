using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class Noticia_Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Noticias",
                columns: table => new
                {
                    IdNoticia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Autor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IdLiga = table.Column<int>(type: "int", nullable: true),
                    LigaIdLiga = table.Column<int>(type: "int", nullable: true),
                    Imagem = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ImagemMimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkInstagram = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataPublicacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticias", x => x.IdNoticia);
                    table.ForeignKey(
                        name: "FK_Noticias_Ligas_LigaIdLiga",
                        column: x => x.LigaIdLiga,
                        principalTable: "Ligas",
                        principalColumn: "IdLiga");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_DataPublicacao",
                table: "Noticias",
                column: "DataPublicacao");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_LigaIdLiga",
                table: "Noticias",
                column: "LigaIdLiga");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Noticias");
        }
    }
}
