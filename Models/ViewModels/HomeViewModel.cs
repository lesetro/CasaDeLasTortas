using System.Collections.Generic;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<TortaCatalogoDTO> Tortas { get; set; }
        public IEnumerable<TortaDestacadaDTO> TortasDestacadas { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
        public FiltrosHomeViewModel Filtros { get; set; }
        public EstadisticasHomeViewModel Estadisticas { get; set; }
    }

    public class TortaCatalogoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string DescripcionCorta { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; }
        public decimal Calificacion { get; set; }
        public string NombreVendedor { get; set; }
        public string ImagenPrincipal { get; set; }
        public bool Disponible { get; set; }
        public bool EsNuevo { get; set; }
        public bool EsPopular { get; set; }
        public bool EsPreferida { get; set; }
        public bool EsFavorito { get; set; }
        public int Stock { get; set; }
    }

    public class TortaDestacadaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public decimal Calificacion { get; set; }
        public string NombreVendedor { get; set; }
        public string ImagenPrincipal { get; set; }
    }

    public class FiltrosHomeViewModel
    {
        public string Busqueda { get; set; }
        public string Categoria { get; set; }
        public string OrdenarPor { get; set; }
        public IEnumerable<string> CategoriasDisponibles { get; set; }
    }

    public class EstadisticasHomeViewModel
    {
        public int TotalTortas { get; set; }
        public int TotalVendedores { get; set; }
        public decimal VentasHoy { get; set; }
        public int TortasNuevas { get; set; }
    }

    // Para el dashboard del comprador
    public class CompradorDashboardViewModel
    {
        public CompradorInfoViewModel Comprador { get; set; }
        public IEnumerable<TortaCatalogoDTO> Tortas { get; set; }
        public IEnumerable<TortaCatalogoDTO> TortasRecomendadas { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
        public FiltrosHomeViewModel Filtros { get; set; }
        public CompradorStatsViewModel Estadisticas { get; set; }
    }

    public class CompradorInfoViewModel
    {
        public string Nombre { get; set; }
        public int TotalCompras { get; set; }
        public decimal GastoMes { get; set; }
        public int PedidosPendientes { get; set; }
    }

    public class CompradorStatsViewModel
    {
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public int PedidosActivos { get; set; }
        public int ComprasMes { get; set; }
    }
}