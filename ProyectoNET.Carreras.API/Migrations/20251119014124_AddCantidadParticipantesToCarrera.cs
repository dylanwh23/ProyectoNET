using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNET.Carreras.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCantidadParticipantesToCarrera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadParticipantes",
                table: "Carreras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadParticipantes",
                table: "Carreras");
        }
    }
}
