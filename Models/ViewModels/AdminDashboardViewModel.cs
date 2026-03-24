using System;
using System.Collections.Generic;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.ViewModels
{
    /// <summary>
    /// ViewModel completo para el Dashboard del Administrador
    /// Incluye todas las métricas y listas de acciones pendientes
    /// </summary>
    public class AdminDashboardViewModel
    {
        // ==================== RESUMEN GENERAL ====================
        
        public ResumenGeneralViewModel Resumen { get; set; } = new();
        
        // ==================== ACCIONES PENDIENTES ====================
        
        /// <summary>
        /// Pagos esperando verificación del admin
        /// </summary>
        public List<PagoPendienteDTO> PagosPendientes { get; set; } = new();
        public int TotalPagosPendientes { get; set; }
        
        /// <summary>
        /// Liberaciones listas para procesar
        /// </summary>
        public List<LiberacionPendienteDTO> LiberacionesPendientes { get; set; } = new();
        public int TotalLiberacionesPendientes { get; set; }
        
        /// <summary>
        /// Disputas abiertas que requieren atención
        /// </summary>
        public List<DisputaResumenDTO> DisputasAbiertas { get; set; } = new();
        public int TotalDisputasAbiertas { get; set; }
        
        /// <summary>
        /// Vendedores pendientes de verificación
        /// </summary>
        public List<VendedorResumenDTO> VendedoresPendientes { get; set; } = new();
        public int TotalVendedoresPendientes { get; set; }
        
        // ==================== ACTIVIDAD RECIENTE ====================
        
        /// <summary>
        /// Últimas ventas realizadas
        /// </summary>
        public List<VentaResumenDTO> VentasRecientes { get; set; } = new();
        
        /// <summary>
        /// Últimos pagos verificados
        /// </summary>
        public List<PagoDTO> PagosRecientes { get; set; } = new();
        
        /// <summary>
        /// Últimas liberaciones procesadas
        /// </summary>
        public List<LiberacionDTO> LiberacionesRecientes { get; set; } = new();
        
        // ==================== ALERTAS ====================
        
        public List<AlertaAdminDTO> Alertas { get; set; } = new();
        
        // ==================== GRÁFICOS (datos) ====================
        
        /// <summary>
        /// Ventas por día (últimos 7 días)
        /// </summary>
        public List<DatoGraficoDTO> VentasPorDia { get; set; } = new();
        
        /// <summary>
        /// Ingresos por día (últimos 7 días)
        /// </summary>
        public List<DatoGraficoDTO> IngresosPorDia { get; set; } = new();
        
        /// <summary>
        /// Distribución de estados de ventas
        /// </summary>
        public Dictionary<string, int> DistribucionEstadosVentas { get; set; } = new();
        
        // ==================== TOP VENDEDORES ====================
        
        public List<TopVendedorDTO> TopVendedores { get; set; } = new();
        
        // ==================== CONFIGURACIÓN ====================
        
        public ConfiguracionPlataformaResumenDTO Configuracion { get; set; } = new();
    }

    /// <summary>
    /// Resumen general de métricas
    /// </summary>
    public class ResumenGeneralViewModel
    {
        // Hoy
        public int VentasHoy { get; set; }
        public decimal IngresosHoy { get; set; }
        public decimal ComisionesHoy { get; set; }
        public int NuevosUsuariosHoy { get; set; }
        
        // Esta semana
        public int VentasSemana { get; set; }
        public decimal IngresosSemana { get; set; }
        public decimal ComisionesSemana { get; set; }
        
        // Este mes
        public int VentasMes { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal ComisionesMes { get; set; }
        
        // Totales históricos
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalComisiones { get; set; }
        
        // Usuarios
        public int TotalUsuarios { get; set; }
        public int TotalCompradores { get; set; }
        public int TotalVendedores { get; set; }
        public int VendedoresActivos { get; set; }
        public int VendedoresVerificados { get; set; }
        
        // Productos
        public int TotalTortas { get; set; }
        public int TortasDisponibles { get; set; }
        
        // Comparación con período anterior (%)
        public decimal CambioVentasVsAyer { get; set; }
        public decimal CambioIngresosVsSemanaAnterior { get; set; }
    }

    /// <summary>
    /// DTO para alertas del admin
    /// </summary>
    public class AlertaAdminDTO
    {
        public string Tipo { get; set; } = string.Empty; // "warning", "danger", "info"
        public string Icono { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? LinkAccion { get; set; }
        public string? TextoAccion { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>
    /// DTO para datos de gráficos
    /// </summary>
    public class DatoGraficoDTO
    {
        public string Etiqueta { get; set; } = string.Empty; // Fecha, categoría, etc.
        public decimal Valor { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// DTO para top vendedores
    /// </summary>
    public class TopVendedorDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal Calificacion { get; set; }
        public bool Verificado { get; set; }
        public int Posicion { get; set; }
    }

    /// <summary>
    /// DTO para resumen de configuración de plataforma
    /// </summary>
    public class ConfiguracionPlataformaResumenDTO
    {
        public string NombrePlataforma { get; set; } = string.Empty;
        public decimal ComisionPorcentaje { get; set; }
        public bool PlataformaActiva { get; set; }
        public bool DatosBancariosConfigurados { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }

    /// <summary>
    /// DTO para resumen de venta (listados)
    /// </summary>
    public class VentaResumenDTO
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string NombreComprador { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public EstadoVenta Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public int CantidadProductos { get; set; }
        public int CantidadVendedores { get; set; }
        public bool TienePagoVerificado { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Pagos del Admin
    /// </summary>
    public class AdminPagosViewModel
    {
        public EstadisticasPagosDTO Estadisticas { get; set; } = new();
        public List<PagoPendienteDTO> PagosPendientes { get; set; } = new();
        public List<PagoDTO> PagosRecientes { get; set; } = new();
        public DatosPagoPlataformaDTO DatosPlataforma { get; set; } = new();
        
        // Filtros aplicados
        public string? FiltroEstado { get; set; }
        public DateTime? FiltroDesde { get; set; }
        public DateTime? FiltroHasta { get; set; }
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Liberaciones del Admin
    /// </summary>
    public class AdminLiberacionesViewModel
    {
        public EstadisticasLiberacionesDTO Estadisticas { get; set; } = new();
        public List<LiberacionPendienteDTO> LiberacionesPendientes { get; set; } = new();
        public List<LiberacionDTO> LiberacionesRecientes { get; set; } = new();
        
        // Filtros
        public string? FiltroEstado { get; set; }
        public int? FiltroVendedorId { get; set; }
        public DateTime? FiltroDesde { get; set; }
        public DateTime? FiltroHasta { get; set; }
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Disputas del Admin
    /// </summary>
    public class AdminDisputasViewModel
    {
        public EstadisticasDisputasDTO Estadisticas { get; set; } = new();
        public List<DisputaResumenDTO> DisputasAbiertas { get; set; } = new();
        public List<DisputaResumenDTO> DisputasRecientes { get; set; } = new();
        
        // Filtros
        public string? FiltroEstado { get; set; }
        public string? FiltroTipo { get; set; }
        public int? FiltroPrioridad { get; set; }
        public DateTime? FiltroDesde { get; set; }
        public DateTime? FiltroHasta { get; set; }
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }

    /// <summary>
    /// ViewModel para la página de Configuración de la Plataforma
    /// </summary>
    public class AdminConfiguracionViewModel
    {
        public ConfiguracionPlataforma Configuracion { get; set; } = new();
        public bool EsNuevaConfiguracion { get; set; }
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
        
        // Listas para dropdowns
        public List<string> BancosDisponibles { get; set; } = new()
        {
            "Banco Nación",
            "Banco Provincia",
            "Banco Galicia",
            "Banco Santander",
            "Banco BBVA",
            "Banco Macro",
            "Banco HSBC",
            "Banco Patagonia",
            "Banco Ciudad",
            "Banco Credicoop",
            "MercadoPago",
            "Ualá",
            "Brubank",
            "Naranja X",
            "Otro"
        };
    }

    /// <summary>
    /// ViewModel para la página de Usuarios del Admin
    /// </summary>
    public class AdminUsuariosViewModel
    {
        public EstadisticasUsuariosDTO Estadisticas { get; set; } = new();
        public List<UsuarioListaDTO> Usuarios { get; set; } = new();
        
        // Filtros
        public string? Busqueda { get; set; }
        public string? FiltroRol { get; set; }
        public bool? FiltroActivo { get; set; }
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int TamanioPagina { get; set; } = 20;
    }

    /// <summary>
    /// DTO para estadísticas de usuarios
    /// </summary>
    public class EstadisticasUsuariosDTO
    {
        public int TotalUsuarios { get; set; }
        public int TotalCompradores { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalAdmins { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int NuevosHoy { get; set; }
        public int NuevosEstaSemana { get; set; }
    }

    /// <summary>
    /// DTO para listar usuarios en la tabla de admin
    /// </summary>
    public class UsuarioListaDTO
    {
        public int PersonaId { get; set; }
        public int? VendedorId { get; set; }
        public int? CompradorId { get; set; }
        
        public string Nombre { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        
        public string Rol { get; set; } = "SinRol";
        public string? NombreComercial { get; set; }
        public bool Activo { get; set; }
        public bool Verificado { get; set; }
        
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}