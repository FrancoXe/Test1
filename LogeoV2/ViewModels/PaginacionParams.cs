namespace LogeoV2.ViewModels
{
    public class PaginacionParams
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string AccionUrl { get; set; } = string.Empty;
        public Dictionary<string, string?> ParametrosExtra { get; set; } = new();

        public string ConstruirUrl(int pagina)
        {
            var query = ParametrosExtra
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}")
                .ToList();

            query.Add($"pagina={pagina}");
            return $"{AccionUrl}?{string.Join("&", query)}";
        }
    }
}