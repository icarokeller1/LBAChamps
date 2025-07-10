using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class removeesporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ligas_Nome_Esporte",
                table: "Ligas");

            migrationBuilder.DropColumn(
                name: "Esporte",
                table: "Ligas");

            migrationBuilder.CreateIndex(
                name: "IX_Ligas_Nome",
                table: "Ligas",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ligas_Nome",
                table: "Ligas");

            migrationBuilder.AddColumn<string>(
                name: "Esporte",
                table: "Ligas",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ligas_Nome_Esporte",
                table: "Ligas",
                columns: new[] { "Nome", "Esporte" },
                unique: true);
        }
    }
}
