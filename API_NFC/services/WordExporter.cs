using System;
using System.Collections.Generic;
using System.IO;
using API___NFC.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace API___NFC.Services.ExportHelpers
{
    public static class WordExporter
    {
        // Genera DOCX simple usando Open XML SDK; usamos alias 'Word' para evitar ambigüedad con otros tipos 'Table' o 'Text'.
        public static byte[] GenerateWord(IEnumerable<FlujoNfcItemDto> datos)
        {
            using (var ms = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Word.Document();
                    var body = new Word.Body();

                    // Título centrado
                    var pTitle = new Word.Paragraph(new Word.Run(new Word.Text("Reporte de Flujo de Ingreso y Salida")));
                    pTitle.ParagraphProperties = new Word.ParagraphProperties(new Word.Justification { Val = Word.JustificationValues.Center });
                    body.AppendChild(pTitle);

                    // Fecha generación
                    body.AppendChild(new Word.Paragraph(new Word.Run(new Word.Text($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"))));

                    // Tabla
                    var table = new Word.Table();

                    // Bordes de tabla
                    var tblProps = new Word.TableProperties(
                        new Word.TableBorders(
                            new Word.TopBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 },
                            new Word.BottomBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 },
                            new Word.LeftBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 },
                            new Word.RightBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 },
                            new Word.InsideHorizontalBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 },
                            new Word.InsideVerticalBorder { Val = new EnumValue<Word.BorderValues>(Word.BorderValues.Single), Size = 4 }
                        ));
                    table.AppendChild(tblProps);

                    // Encabezado
                    var headerRow = new Word.TableRow();
                    string[] headers = { "Fecha / Hora", "Tipo", "Persona", "Documento", "Dispositivos", "Tipo Persona" };
                    foreach (var h in headers)
                    {
                        var tc = new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(h))));
                        tc.Append(new Word.TableCellProperties(new Word.TableCellWidth { Type = Word.TableWidthUnitValues.Auto }));
                        headerRow.Append(tc);
                    }
                    table.Append(headerRow);

                    // Filas de datos
                    foreach (var row in datos)
                    {
                        var tr = new Word.TableRow();

                        string fecha = row == null || row.FechaRegistro == DateTime.MinValue
                            ? "N/A"
                            : row.FechaRegistro.ToString("yyyy-MM-dd HH:mm");

                        string[] cells = {
                            fecha,
                            row?.TipoRegistro ?? string.Empty,
                            row?.NombreCompleto ?? string.Empty,
                            row?.Documento ?? string.Empty,
                            row?.DispositivosTexto ?? string.Empty,
                            row?.TipoPersona ?? string.Empty
                        };

                        foreach (var c in cells)
                        {
                            var tc = new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(c))));
                            tr.Append(tc);
                        }

                        table.Append(tr);
                    }

                    body.Append(table);
                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }

                return ms.ToArray();
            }
        }
    }
}