using ClosedXML.Excel;
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
    }
}