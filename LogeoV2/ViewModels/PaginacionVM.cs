namespace LogeoV2.ViewModels
{
    public class ResultadoPaginado<T>
    {
        public List<T> Items { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }
}