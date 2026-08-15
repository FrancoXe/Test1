namespace LogeoV2.Services
{
    public interface IExportService
    {
        byte[] ExportarExcel(string titulo, List<string> encabezados, List<List<string>> filas);
        byte[] ExportarPdf(string titulo, List<string> encabezados, List<List<string>> filas);
        byte[] ExportarCsv(List<string> encabezados, List<List<string>> filas);
    }
}