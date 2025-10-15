using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API___NFC.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoEstadoSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComandoAgente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comando = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandoAgente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionario",
                columns: table => new
                {
                    IdFuncionario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsNatural = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionario", x => x.IdFuncionario);
                });

            migrationBuilder.CreateTable(
                name: "Programa",
                columns: table => new
                {
                    IdPrograma = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePrograma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NivelFormacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programa", x => x.IdPrograma);
                });

            migrationBuilder.CreateTable(
                name: "TareaEscritura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatosParaEscribir = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareaEscritura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoElemento",
                columns: table => new
                {
                    IdTipoElemento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoElemento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoElemento", x => x.IdTipoElemento);
                });

            migrationBuilder.CreateTable(
                name: "TipoProceso",
                columns: table => new
                {
                    IdTipoProceso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoProceso", x => x.IdTipoProceso);
                });

            migrationBuilder.CreateTable(
                name: "Ficha",
                columns: table => new
                {
                    IdFicha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdPrograma = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ficha", x => x.IdFicha);
                    table.ForeignKey(
                        name: "FK_Ficha_Programa_IdPrograma",
                        column: x => x.IdPrograma,
                        principalTable: "Programa",
                        principalColumn: "IdPrograma");
                });

            migrationBuilder.CreateTable(
                name: "Aprendiz",
                columns: table => new
                {
                    IdAprendiz = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdFicha = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aprendiz", x => x.IdAprendiz);
                    table.ForeignKey(
                        name: "FK_Aprendiz_Ficha_IdFicha",
                        column: x => x.IdFicha,
                        principalTable: "Ficha",
                        principalColumn: "IdFicha");
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFuncionario = table.Column<int>(type: "int", nullable: true),
                    IdAprendiz = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Aprendiz_IdAprendiz",
                        column: x => x.IdAprendiz,
                        principalTable: "Aprendiz",
                        principalColumn: "IdAprendiz");
                    table.ForeignKey(
                        name: "FK_Usuario_Funcionario_IdFuncionario",
                        column: x => x.IdFuncionario,
                        principalTable: "Funcionario",
                        principalColumn: "IdFuncionario");
                });

            migrationBuilder.CreateTable(
                name: "Elemento",
                columns: table => new
                {
                    IdElemento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoElemento = table.Column<int>(type: "int", nullable: true),
                    elemento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Serial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaracteristicasTecnicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaracteristicasFisicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdPropietario = table.Column<int>(type: "int", nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TieneNFCTag = table.Column<bool>(type: "bit", nullable: true),
                    imageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elemento", x => x.IdElemento);
                    table.ForeignKey(
                        name: "FK_Elemento_TipoElemento_IdTipoElemento",
                        column: x => x.IdTipoElemento,
                        principalTable: "TipoElemento",
                        principalColumn: "IdTipoElemento");
                    table.ForeignKey(
                        name: "FK_Elemento_Usuario_IdPropietario",
                        column: x => x.IdPropietario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "Proceso",
                columns: table => new
                {
                    IdProceso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoProceso = table.Column<int>(type: "int", nullable: true),
                    TimeStampActual = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdElemento = table.Column<int>(type: "int", nullable: true),
                    RequiereOtroProceso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdPortador = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proceso", x => x.IdProceso);
                    table.ForeignKey(
                        name: "FK_Proceso_Elemento_IdElemento",
                        column: x => x.IdElemento,
                        principalTable: "Elemento",
                        principalColumn: "IdElemento");
                    table.ForeignKey(
                        name: "FK_Proceso_TipoProceso_IdTipoProceso",
                        column: x => x.IdTipoProceso,
                        principalTable: "TipoProceso",
                        principalColumn: "IdTipoProceso");
                    table.ForeignKey(
                        name: "FK_Proceso_Usuario_IdPortador",
                        column: x => x.IdPortador,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aprendiz_IdFicha",
                table: "Aprendiz",
                column: "IdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_Elemento_IdPropietario",
                table: "Elemento",
                column: "IdPropietario");

            migrationBuilder.CreateIndex(
                name: "IX_Elemento_IdTipoElemento",
                table: "Elemento",
                column: "IdTipoElemento");

            migrationBuilder.CreateIndex(
                name: "IX_Ficha_IdPrograma",
                table: "Ficha",
                column: "IdPrograma");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdElemento",
                table: "Proceso",
                column: "IdElemento");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdPortador",
                table: "Proceso",
                column: "IdPortador");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdTipoProceso",
                table: "Proceso",
                column: "IdTipoProceso");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdAprendiz",
                table: "Usuario",
                column: "IdAprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdFuncionario",
                table: "Usuario",
                column: "IdFuncionario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComandoAgente");

            migrationBuilder.DropTable(
                name: "Proceso");

            migrationBuilder.DropTable(
                name: "TareaEscritura");

            migrationBuilder.DropTable(
                name: "Elemento");

            migrationBuilder.DropTable(
                name: "TipoProceso");

            migrationBuilder.DropTable(
                name: "TipoElemento");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Aprendiz");

            migrationBuilder.DropTable(
                name: "Funcionario");

            migrationBuilder.DropTable(
                name: "Ficha");

            migrationBuilder.DropTable(
                name: "Programa");
        }
    }
}
