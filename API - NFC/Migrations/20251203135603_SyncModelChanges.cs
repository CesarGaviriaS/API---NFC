using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API___NFC.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosNFC_Aprendiz_IdAprendiz",
                table: "RegistrosNFC");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosNFC_Usuario_IdUsuario",
                table: "RegistrosNFC");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_CodigoBarras",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrosNFC",
                table: "RegistrosNFC");

            migrationBuilder.RenameTable(
                name: "RegistrosNFC",
                newName: "RegistroNFC");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosNFC_IdUsuario",
                table: "RegistroNFC",
                newName: "IX_RegistroNFC_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosNFC_IdAprendiz",
                table: "RegistroNFC",
                newName: "IX_RegistroNFC_IdAprendiz");

            migrationBuilder.AlterColumn<string>(
                name: "CodigoBarras",
                table: "Usuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaTokenExpira",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacion",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagenUrl",
                table: "Elemento",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "RegistroNFC",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IdAprendiz",
                table: "RegistroNFC",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistroNFC",
                table: "RegistroNFC",
                column: "IdRegistro");

            migrationBuilder.CreateTable(
                name: "TagAsignado",
                columns: table => new
                {
                    IdTag = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdPersona = table.Column<int>(type: "int", nullable: false),
                    TipoPersona = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAsignado", x => x.IdTag);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CodigoBarras",
                table: "Usuario",
                column: "CodigoBarras",
                unique: true,
                filter: "[CodigoBarras] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroNFC_Aprendiz_IdAprendiz",
                table: "RegistroNFC",
                column: "IdAprendiz",
                principalTable: "Aprendiz",
                principalColumn: "IdAprendiz",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroNFC_Usuario_IdUsuario",
                table: "RegistroNFC",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistroNFC_Aprendiz_IdAprendiz",
                table: "RegistroNFC");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroNFC_Usuario_IdUsuario",
                table: "RegistroNFC");

            migrationBuilder.DropTable(
                name: "TagAsignado");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_CodigoBarras",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistroNFC",
                table: "RegistroNFC");

            migrationBuilder.DropColumn(
                name: "FechaTokenExpira",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacion",
                table: "Usuario");

            migrationBuilder.RenameTable(
                name: "RegistroNFC",
                newName: "RegistrosNFC");

            migrationBuilder.RenameIndex(
                name: "IX_RegistroNFC_IdUsuario",
                table: "RegistrosNFC",
                newName: "IX_RegistrosNFC_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_RegistroNFC_IdAprendiz",
                table: "RegistrosNFC",
                newName: "IX_RegistrosNFC_IdAprendiz");

            migrationBuilder.AlterColumn<string>(
                name: "CodigoBarras",
                table: "Usuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagenUrl",
                table: "Elemento",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "RegistrosNFC",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdAprendiz",
                table: "RegistrosNFC",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrosNFC",
                table: "RegistrosNFC",
                column: "IdRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CodigoBarras",
                table: "Usuario",
                column: "CodigoBarras",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosNFC_Aprendiz_IdAprendiz",
                table: "RegistrosNFC",
                column: "IdAprendiz",
                principalTable: "Aprendiz",
                principalColumn: "IdAprendiz",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosNFC_Usuario_IdUsuario",
                table: "RegistrosNFC",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
