using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoNET.Carreras.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carreras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    FechaCreada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Kms = table.Column<double>(type: "double precision", nullable: true),
                    CostoInscripcion = table.Column<long>(type: "bigint", nullable: false),
                    CantidadParticipantes = table.Column<int>(type: "integer", nullable: false),
                    CantidadMaximaParticipantes = table.Column<int>(type: "integer", nullable: false),
                    EstadoCarrera = table.Column<int>(type: "integer", nullable: false),
                    ImagenPromocional = table.Column<string>(type: "text", nullable: false),
                    Checkpoints = table.Column<string>(type: "jsonb", nullable: true),
                    RutaGeoJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carreras", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Participantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEquipamientoEntregado = table.Column<bool>(type: "boolean", nullable: false),
                    CarreraId = table.Column<int>(type: "integer", nullable: false),
                    LugarRetiroEquipamientoElegidoId = table.Column<int>(type: "integer", nullable: true),
                    IdLugarRetiroEquipamientoElegido = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participantes_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participantes_LugaresDeEntrega_LugarRetiroEquipamientoElegi~",
                        column: x => x.LugarRetiroEquipamientoElegidoId,
                        principalTable: "LugaresDeEntrega",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LugaresDeEntrega_CarreraId",
                table: "LugaresDeEntrega",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Participantes_CarreraId",
                table: "Participantes",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Participantes_LugarRetiroEquipamientoElegidoId",
                table: "Participantes",
                column: "LugarRetiroEquipamientoElegidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participantes");

            migrationBuilder.DropTable(
                name: "LugaresDeEntrega");

            migrationBuilder.DropTable(
                name: "Carreras");
        }
    }
}
