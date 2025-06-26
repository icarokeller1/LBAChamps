using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LBAChamps.Migrations
{
    /// <inheritdoc />
    public partial class Time_AddLogoBinarya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Times");

            migrationBuilder.AddColumn<byte[]>(
                name: "Logo",
                table: "Times",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoMimeType",
                table: "Times",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Times");

            migrationBuilder.DropColumn(
                name: "LogoMimeType",
                table: "Times");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Times",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
