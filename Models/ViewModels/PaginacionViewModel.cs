namespace CasaDeLasTortas.Models.ViewModels
{
    public class PaginacionViewModel
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalItems { get; set; }
        
        // ✅ PROPIEDAD QUE FALTABA
        public string? Busqueda { get; set; }
        
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
        
        public int PrimerItem => ((PaginaActual - 1) * TamanioPagina) + 1;
        public int UltimoItem => Math.Min(PaginaActual * TamanioPagina, TotalItems);
        
        // ✅ PROPIEDADES ADICIONALES QUE SE USAN EN _PaginacionPartial.cshtml
        public int ItemsDesde => PrimerItem;
        public int ItemsHasta => UltimoItem;
        
        // Para generar los números de página en la vista
        public List<int> NumerosPagina
        {
            get
            {
                var paginas = new List<int>();
                var inicio = Math.Max(1, PaginaActual - 2);
                var fin = Math.Min(TotalPaginas, PaginaActual + 2);
                
                for (int i = inicio; i <= fin; i++)
                {
                    paginas.Add(i);
                }
                
                return paginas;
            }
        }

        public PaginacionViewModel()
        {
            TamanioPagina = 10; // Valor por defecto
        }

        public PaginacionViewModel(int totalItems, int paginaActual, int tamanioPagina)
        {
            TotalItems = totalItems;
            PaginaActual = paginaActual;
            TamanioPagina = tamanioPagina;
            TotalPaginas = (int)Math.Ceiling(totalItems / (double)tamanioPagina);
        }
    }
}