using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNET.Carreras.API.Migrations
{
    /// <inheritdoc />
    public partial class KmsCarrera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Kms",
                table: "Carreras",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kms",
                table: "Carreras");
        }
    }
}
