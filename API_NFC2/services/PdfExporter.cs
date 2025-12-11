using System;
using System.Collections.Generic;
using System.IO;
using API___NFC.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API___NFC.Services.ExportHelpers
{
    public static class PdfExporter
    {
        // Genera un PDF con QuestPDF a partir de la lista de DTOs
        public static byte[] GeneratePdf(IEnumerable<FlujoNfcItemDto> datos)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header()
                        .Text("Reporte de Flujo de Ingreso y Salida")
                        .SemiBold()
                        .FontSize(14)
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(5)
                        .Column(col =>
                        {
                            col.Item()
                                .Text($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                                .FontSize(9)
                                .AlignRight();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // fecha
                                    columns.RelativeColumn(1); // tipo
                                    columns.RelativeColumn(3); // persona
                                    columns.RelativeColumn(2); // documento
                                    columns.RelativeColumn(3); // dispositivos
                                    columns.RelativeColumn(1); // tipo persona
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Fecha / Hora").SemiBold();

                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Tipo").SemiBold();

                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Persona").SemiBold();

                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Documento").SemiBold();

                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Dispositivos").SemiBold();

                                    header.Cell().Element(cell => {
                                        cell.PaddingVertical(6).PaddingHorizontal(8).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text("Tipo Persona").SemiBold();
                                });

                                // Rows
                                foreach (var row in datos)
                                {
                                    var fecha = row == null || row.FechaRegistro == DateTime.MinValue
                                        ? "N/A"
                                        : row.FechaRegistro.ToString("yyyy-MM-dd HH:mm");

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(fecha);

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(row?.TipoRegistro ?? string.Empty);

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(row?.NombreCompleto ?? string.Empty);

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(row?.Documento ?? string.Empty);

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(row?.DispositivosTexto ?? string.Empty);

                                    table.Cell().Element(cell => {
                                        cell.PaddingVertical(4).PaddingHorizontal(6).Border(1).BorderColor(Colors.Grey.Lighten2);
                                        return cell;
                                    }).Text(row?.TipoPersona ?? string.Empty);
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            using (var ms = new MemoryStream())
            {
                doc.GeneratePdf(ms);
                return ms.ToArray();
            }
        }
    }
}