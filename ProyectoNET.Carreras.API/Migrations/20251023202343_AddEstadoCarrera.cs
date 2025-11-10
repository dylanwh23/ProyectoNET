using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoNET.Carreras.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoCarrera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LugarRetiroEquipamientoElegido",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "LugaresRetiroEquipamiento",
                table: "Carreras");

            migrationBuilder.RenameColumn(
                name: "EquipamientoEntregado",
                table: "Participantes",
                newName: "IsEquipamientoEntregado");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Carreras",
                newName: "ImagenPromocional");

            migrationBuilder.AddColumn<int>(
                name: "IdLugarRetiroEquipamientoElegido",
                table: "Participantes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LugarRetiroEquipamientoElegidoId",
                table: "Participantes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstadoCarrera",
                table: "Carreras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LugaresDeEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    CarreraId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LugaresDeEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LugaresDeEntrega_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Participantes_LugarRetiroEquipamientoElegidoId",
                table: "Participantes",
                column: "LugarRetiroEquipamientoElegidoId");

            migrationBuilder.CreateIndex(
                name: "IX_LugaresDeEntrega_CarreraId",
                table: "LugaresDeEntrega",
                column: "CarreraId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes",
                column: "LugarRetiroEquipamientoElegidoId",
                principalTable: "LugaresDeEntrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                table: "Participantes");

            migrationBuilder.DropTable(
                name: "LugaresDeEntrega");

            migrationBuilder.DropIndex(
                name: "IX_Participantes_LugarRetiroEquipamientoElegidoId",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "IdLugarRetiroEquipamientoElegido",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "LugarRetiroEquipamientoElegidoId",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "EstadoCarrera",
                table: "Carreras");

            migrationBuilder.RenameColumn(
                name: "IsEquipamientoEntregado",
                table: "Participantes",
                newName: "EquipamientoEntregado");

            migrationBuilder.RenameColumn(
                name: "ImagenPromocional",
                table: "Carreras",
                newName: "Estado");

            migrationBuilder.AddColumn<string>(
                name: "LugarRetiroEquipamientoElegido",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "LugaresRetiroEquipamiento",
                table: "Carreras",
                type: "text[]",
                nullable: false);
        }
    }
}
