using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class Noticia_FK_Liga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Noticias_Ligas_LigaIdLiga",
                table: "Noticias");

            migrationBuilder.DropIndex(
                name: "IX_Noticias_LigaIdLiga",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "LigaIdLiga",
                table: "Noticias");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_IdLiga",
                table: "Noticias",
                column: "IdLiga");

            migrationBuilder.AddForeignKey(
                name: "FK_Noticias_Ligas_IdLiga",
                table: "Noticias",
                column: "IdLiga",
                principalTable: "Ligas",
                principalColumn: "IdLiga",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Noticias_Ligas_IdLiga",
                table: "Noticias");

            migrationBuilder.DropIndex(
                name: "IX_Noticias_IdLiga",
                table: "Noticias");

            migrationBuilder.AddColumn<int>(
                name: "LigaIdLiga",
                table: "Noticias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_LigaIdLiga",
                table: "Noticias",
                column: "LigaIdLiga");

            migrationBuilder.AddForeignKey(
                name: "FK_Noticias_Ligas_LigaIdLiga",
                table: "Noticias",
                column: "LigaIdLiga",
                principalTable: "Ligas",
                principalColumn: "IdLiga");
        }
    }
}
