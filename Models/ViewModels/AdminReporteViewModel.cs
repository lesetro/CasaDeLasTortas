using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.ViewModels
{


    /// <summary>
    /// ViewModel para la página de reportes del Admin
    /// </summary>
    public class AdminReporteViewModel
    {
        // Rango de fechas del reporte
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        
        // Totales generales
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalComisiones { get; set; }
        public decimal PromedioVenta { get; set; }
        
        // Reportes detallados
        public List<ReporteVendedorDTO> VentasPorVendedor { get; set; } = new();
        public List<ReporteProductoDTO> ProductosMasVendidos { get; set; } = new();
        public Dictionary<DateTime, ReporteDiarioDTO> VentasPorDia { get; set; } = new();
        
        // Comparativa con período anterior
        public decimal VariacionIngresos { get; set; }
        public decimal VariacionVentas { get; set; }
        
        // Propiedades calculadas
        public string RangoFechasTexto => $"{FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}";
        public int DiasEnRango => (FechaFin - FechaInicio).Days + 1;
        public decimal PromedioIngresoDiario => DiasEnRango > 0 ? TotalIngresos / DiasEnRango : 0;
        public decimal MargenComision => TotalIngresos > 0 ? (TotalComisiones / TotalIngresos) * 100 : 0;
    }

    /// <summary>
    /// DTO para reporte de ventas por vendedor
    /// </summary>
    public class ReporteVendedorDTO
    {
        public int VendedorId { get; set; }
        public string NombreVendedor { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        
        // Métricas de ventas
        public int CantidadVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public int ProductosVendidos { get; set; }
        public decimal PromedioVenta => CantidadVentas > 0 ? TotalIngresos / CantidadVentas : 0;
        
        // Métricas adicionales
        public decimal Comisiones { get; set; }
        public decimal MontoNeto => TotalIngresos - Comisiones;
        public int ClientesUnicos { get; set; }
        public decimal Calificacion { get; set; }
        
        // Porcentaje del total
        public decimal PorcentajeDelTotal { get; set; }
    }

    /// <summary>
    /// DTO para reporte de productos más vendidos
    /// </summary>
    public class ReporteProductoDTO
    {
        public int TortaId { get; set; }
        public string NombreTorta { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public string? NombreVendedor { get; set; }
        public string? ImagenUrl { get; set; }
        
        // Métricas
        public int CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PrecioPromedio => CantidadVendida > 0 ? TotalIngresos / CantidadVendida : 0;
        
        // Rankings
        public int Ranking { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
        
        // Stock actual
        public int StockActual { get; set; }
        public bool Disponible { get; set; }
    }

    /// <summary>
    /// DTO para reporte diario de ventas
    /// </summary>
    public class ReporteDiarioDTO
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Comisiones { get; set; }
        public int ProductosVendidos { get; set; }
        
        // Métricas calculadas
        public decimal PromedioVenta => CantidadVentas > 0 ? Ingresos / CantidadVentas : 0;
        public decimal MargenComision => Ingresos > 0 ? (Comisiones / Ingresos) * 100 : 0;
        
        // Comparativa con día anterior
        public decimal VariacionIngresos { get; set; }
        public decimal VariacionVentas { get; set; }
        
        // Día de la semana
        public string DiaSemana => Fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
        public bool EsFinDeSemana => Fecha.DayOfWeek == DayOfWeek.Saturday || Fecha.DayOfWeek == DayOfWeek.Sunday;
    }
}