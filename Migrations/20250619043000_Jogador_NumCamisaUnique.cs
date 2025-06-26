using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class Jogador_NumCamisaUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jogadores_IdTime",
                table: "Jogadores");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_IdTime_NumeroCamisa",
                table: "Jogadores",
                columns: new[] { "IdTime", "NumeroCamisa" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jogadores_IdTime_NumeroCamisa",
                table: "Jogadores");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_IdTime",
                table: "Jogadores",
                column: "IdTime");
        }
    }
}
