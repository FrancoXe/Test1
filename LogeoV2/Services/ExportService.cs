using ClosedXML.Excel;
using LogeoV2.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace LogeoV2.Services
{
    public class ExportService : IExportService
    {
        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportarExcel(string titulo, List<string> encabezados, List<List<string>> filas)
        {
            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add(titulo);

            for (int c = 0; c < encabezados.Count; c++)
            {
                hoja.Cell(1, c + 1).Value = encabezados[c];
                hoja.Cell(1, c + 1).Style.Font.Bold = true;
            }

            for (int f = 0; f < filas.Count; f++)
            {
                for (int c = 0; c < filas[f].Count; c++)
                {
                    hoja.Cell(f + 2, c + 1).Value = filas[f][c];
                }
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportarPdf(string titulo, List<string> encabezados, List<List<string>> filas)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.Header().Text(titulo).FontSize(16).Bold();

                    page.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            foreach (var _ in encabezados)
                                columnas.RelativeColumn();
                        });

                        tabla.Header(header =>
                        {
                            foreach (var enc in encabezados)
                            {
                                header.Cell().Border(1).Padding(4).Text(enc).Bold();
                            }
                        });

                        foreach (var fila in filas)
                        {
                            foreach (var valor in fila)
                            {
                                tabla.Cell().Border(1).Padding(4).Text(valor);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado el ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] ExportarCsv(List<string> encabezados, List<List<string>> filas)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", encabezados.Select(EscaparCsv)));

            foreach (var fila in filas)
            {
                sb.AppendLine(string.Join(",", fila.Select(EscaparCsv)));
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private static string EscaparCsv(string valor)
        {
            if (valor.Contains(',') || valor.Contains('"') || valor.Contains('\n'))
            {
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            }
            return valor;
        }
        public byte[] ExportarReclamoDetalle(Reclamo reclamo)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Municipalidad de Unquillo").FontSize(18).Bold();
                        col.Item().Text("Informe de Reclamo").FontSize(14).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"N° de Reclamo: {reclamo.IdReclamo}").Bold();
                            row.RelativeItem().AlignRight().Text($"Fecha: {reclamo.FechaCreacion:dd/MM/yyyy HH:mm}");
                        });

                        col.Item().PaddingTop(10).Text("Datos del Vecino").Bold().FontSize(13);
                        col.Item().Text($"Nombre: {reclamo.Usuario?.Nombre} {reclamo.Usuario?.Apellido}");
                        col.Item().Text($"DNI: {reclamo.DNI}");
                        col.Item().Text($"Correo: {reclamo.Usuario?.Correo}");

                        col.Item().PaddingTop(10).Text("Detalle del Reclamo").Bold().FontSize(13);
                        col.Item().Text($"Categoría: {reclamo.Categoria?.Nombre}");
                        col.Item().Text($"Subcategoría: {reclamo.Subcategoria?.Nombre}");
                        col.Item().Text($"Barrio: {reclamo.Barrio?.Nombre}");
                        col.Item().Text($"Dirección: {reclamo.Direccion}");
                        col.Item().Text("Descripción:").Bold();
                        col.Item().Background(Colors.Grey.Lighten4).Padding(8).Text(reclamo.Descripcion);

                        col.Item().PaddingTop(10).Text("Estado y Seguimiento").Bold().FontSize(13);
                        col.Item().Text($"Estado actual: {reclamo.Estado}").FontColor(Colors.Blue.Darken1).Bold();
                        col.Item().Text($"Departamento asignado: {reclamo.DepartamentoAsignado?.Nombre ?? "Sin asignar"}");
                        if (reclamo.FechaActualizacion.HasValue)
                        {
                            col.Item().Text($"Última actualización: {reclamo.FechaActualizacion:dd/MM/yyyy HH:mm}");
                        }

                        if (!string.IsNullOrEmpty(reclamo.RutaArchivo))
                        {
                            col.Item().PaddingTop(10).Text("Este reclamo incluye un archivo adjunto (no incorporado en este informe).")
                                .FontSize(9).FontColor(Colors.Grey.Darken1).Italic();
                        }
                    });

                    page.Footer().AlignCenter().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Text(x =>
                        {
                            x.Span("Documento generado el ");
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            x.Span(" · Sistema de Gestión de Reclamos - Municipalidad de Unquillo").FontSize(8);
                        });
                    });
                });
            });

            return documento.GeneratePdf();
        }
    }
}