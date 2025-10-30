using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API___NFC.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordRecoveryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Programa",
                columns: table => new
                {
                    IdPrograma = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePrograma = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NivelFormacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programa", x => x.IdPrograma);
                    table.CheckConstraint("CHK_Programa_NivelFormacion", "(NivelFormacion='Operario' OR NivelFormacion='Especialización' OR NivelFormacion='Tecnólogo' OR NivelFormacion='Técnico')");
                });

            migrationBuilder.CreateTable(
                name: "TipoElemento",
                columns: table => new
                {
                    IdTipoElemento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiereNFC = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
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
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoProceso", x => x.IdTipoProceso);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoBarras = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.CheckConstraint("CHK_Usuario_Rol", "(Rol='Administrador' OR Rol='Guardia' OR Rol='Funcionario')");
                    table.CheckConstraint("CHK_Usuario_TipoDocumento", "(TipoDocumento='PA' OR TipoDocumento='CE' OR TipoDocumento='TI' OR TipoDocumento='CC')");
                });

            migrationBuilder.CreateTable(
                name: "Ficha",
                columns: table => new
                {
                    IdFicha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPrograma = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ficha", x => x.IdFicha);
                    table.ForeignKey(
                        name: "FK_Ficha_Programa_IdPrograma",
                        column: x => x.IdPrograma,
                        principalTable: "Programa",
                        principalColumn: "IdPrograma",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Elemento",
                columns: table => new
                {
                    IdElemento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoElemento = table.Column<int>(type: "int", nullable: false),
                    IdPropietario = table.Column<int>(type: "int", nullable: false),
                    TipoPropietario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Serial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoNFC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elemento", x => x.IdElemento);
                    table.CheckConstraint("CHK_Elemento_TipoPropietario", "(TipoPropietario='Usuario' OR TipoPropietario='Aprendiz')");
                    table.ForeignKey(
                        name: "FK_Elemento_TipoElemento_IdTipoElemento",
                        column: x => x.IdTipoElemento,
                        principalTable: "TipoElemento",
                        principalColumn: "IdTipoElemento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aprendiz",
                columns: table => new
                {
                    IdAprendiz = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoBarras = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdFicha = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aprendiz", x => x.IdAprendiz);
                    table.CheckConstraint("CHK_Aprendiz_TipoDocumento", "(TipoDocumento='PA' OR TipoDocumento='CE' OR TipoDocumento='TI' OR TipoDocumento='CC')");
                    table.ForeignKey(
                        name: "FK_Aprendiz_Ficha_IdFicha",
                        column: x => x.IdFicha,
                        principalTable: "Ficha",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proceso",
                columns: table => new
                {
                    IdProceso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoProceso = table.Column<int>(type: "int", nullable: false),
                    TipoPersona = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdGuardia = table.Column<int>(type: "int", nullable: false),
                    TimeStampEntradaSalida = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    RequiereOtrosProcesos = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IdProceso_Relacionado = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SincronizadoBD = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IdAprendiz = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proceso", x => x.IdProceso);
                    table.CheckConstraint("CHK_Proceso_TipoPersona", "(TipoPersona='Usuario' OR TipoPersona='Aprendiz')");
                    table.ForeignKey(
                        name: "FK_Proceso_Aprendiz_IdAprendiz",
                        column: x => x.IdAprendiz,
                        principalTable: "Aprendiz",
                        principalColumn: "IdAprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proceso_TipoProceso_IdTipoProceso",
                        column: x => x.IdTipoProceso,
                        principalTable: "TipoProceso",
                        principalColumn: "IdTipoProceso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proceso_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosNFC",
                columns: table => new
                {
                    IdRegistro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAprendiz = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    TipoRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosNFC", x => x.IdRegistro);
                    table.ForeignKey(
                        name: "FK_RegistrosNFC_Aprendiz_IdAprendiz",
                        column: x => x.IdAprendiz,
                        principalTable: "Aprendiz",
                        principalColumn: "IdAprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosNFC_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ElementoProceso",
                columns: table => new
                {
                    IdElementoProceso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdElemento = table.Column<int>(type: "int", nullable: false),
                    IdProceso = table.Column<int>(type: "int", nullable: false),
                    Validado = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementoProceso", x => x.IdElementoProceso);
                    table.ForeignKey(
                        name: "FK_ElementoProceso_Elemento_IdElemento",
                        column: x => x.IdElemento,
                        principalTable: "Elemento",
                        principalColumn: "IdElemento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElementoProceso_Proceso_IdProceso",
                        column: x => x.IdProceso,
                        principalTable: "Proceso",
                        principalColumn: "IdProceso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aprendiz_CodigoBarras",
                table: "Aprendiz",
                column: "CodigoBarras",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aprendiz_Correo",
                table: "Aprendiz",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aprendiz_IdFicha",
                table: "Aprendiz",
                column: "IdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_Aprendiz_NumeroDocumento",
                table: "Aprendiz",
                column: "NumeroDocumento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elemento_CodigoNFC",
                table: "Elemento",
                column: "CodigoNFC",
                unique: true,
                filter: "[CodigoNFC] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Elemento_IdTipoElemento",
                table: "Elemento",
                column: "IdTipoElemento");

            migrationBuilder.CreateIndex(
                name: "IX_Elemento_Serial",
                table: "Elemento",
                column: "Serial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElementoProceso_IdElemento",
                table: "ElementoProceso",
                column: "IdElemento");

            migrationBuilder.CreateIndex(
                name: "IX_ElementoProceso_IdProceso",
                table: "ElementoProceso",
                column: "IdProceso");

            migrationBuilder.CreateIndex(
                name: "IX_Ficha_Codigo",
                table: "Ficha",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ficha_IdPrograma",
                table: "Ficha",
                column: "IdPrograma");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdAprendiz",
                table: "Proceso",
                column: "IdAprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdTipoProceso",
                table: "Proceso",
                column: "IdTipoProceso");

            migrationBuilder.CreateIndex(
                name: "IX_Proceso_IdUsuario",
                table: "Proceso",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Programa_Codigo",
                table: "Programa",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosNFC_IdAprendiz",
                table: "RegistrosNFC",
                column: "IdAprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosNFC_IdUsuario",
                table: "RegistrosNFC",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_TipoElemento_Tipo",
                table: "TipoElemento",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoProceso_Tipo",
                table: "TipoProceso",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CodigoBarras",
                table: "Usuario",
                column: "CodigoBarras",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_NumeroDocumento",
                table: "Usuario",
                column: "NumeroDocumento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElementoProceso");

            migrationBuilder.DropTable(
                name: "RegistrosNFC");

            migrationBuilder.DropTable(
                name: "Elemento");

            migrationBuilder.DropTable(
                name: "Proceso");

            migrationBuilder.DropTable(
                name: "TipoElemento");

            migrationBuilder.DropTable(
                name: "Aprendiz");

            migrationBuilder.DropTable(
                name: "TipoProceso");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Ficha");

            migrationBuilder.DropTable(
                name: "Programa");
        }
    }
}
