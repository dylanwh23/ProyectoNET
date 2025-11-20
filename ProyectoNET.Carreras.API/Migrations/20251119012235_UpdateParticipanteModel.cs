using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNET.Carreras.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParticipanteModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes");

            migrationBuilder.Sql("ALTER TABLE \"Participantes\" DROP COLUMN IF EXISTS \"Apellido\";");
            migrationBuilder.Sql("ALTER TABLE \"Participantes\" DROP COLUMN IF EXISTS \"Email\";");
            migrationBuilder.Sql("ALTER TABLE \"Participantes\" DROP COLUMN IF EXISTS \"Nombre\";");

            migrationBuilder.AlterColumn<int>(
                name: "LugarRetiroEquipamientoElegidoId",
                table: "Participantes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "IdLugarRetiroEquipamientoElegido",
                table: "Participantes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Participantes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CantidadParticipantes",
                table: "Carreras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes",
                column: "LugarRetiroEquipamientoElegidoId",
                principalTable: "LugaresDeEntrega",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "CantidadParticipantes",
                table: "Carreras");

            migrationBuilder.AlterColumn<int>(
                name: "LugarRetiroEquipamientoElegidoId",
                table: "Participantes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdLugarRetiroEquipamientoElegido",
                table: "Participantes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes",
                column: "LugarRetiroEquipamientoElegidoId",
                principalTable: "LugaresDeEntrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
