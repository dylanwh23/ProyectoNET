using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNET.Usuarios.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaRegistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");
        }
    }
}
