using System;
using System.Collections.Generic;
using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.DTOs
{
    // DTO Principal
    public class PagoDTO
    {
        public int Id { get; set; }
        public int TortaId { get; set; }
        public int CompradorId { get; set; }
        public int VendedorId { get; set; }
        public decimal Monto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public string? ArchivoComprobante { get; set; }
        public string? NumeroTransaccion { get; set; }
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool NotificacionEnviada { get; set; }
        
        // Información relacionada
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
        public string NombreVendedor { get; set; }
        public string? ImagenTorta { get; set; }
    }

    // DTO para Crear Pago
    public class PagoCreateDTO
    {
        public int TortaId { get; set; }
        public int CompradorId { get; set; }
        public int Cantidad { get; set; }
        public decimal? Descuento { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string? NumeroTransaccion { get; set; }
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public IFormFile? ArchivoComprobante { get; set; }
    }

    // DTO para Actualizar Pago
    public class PagoUpdateDTO
    {
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public string? NumeroTransaccion { get; set; }
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public IFormFile? ArchivoComprobante { get; set; }
    }

    // DTO para Listado de Pagos
    public class PagoListDTO
    {
        public int Id { get; set; }
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
        public string NombreVendedor { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public bool TieneComprobante { get; set; }
        public string? ImagenTorta { get; set; }
    }

    // DTO para Detalle de Pago
    public class PagoDetalleDTO
    {
        public int Id { get; set; }
        
        // Información de la Torta
        public TortaListDTO Torta { get; set; }
        
        // Información del Comprador
        public CompradorListDTO Comprador { get; set; }
        
        // Información del Vendedor
        public VendedorListDTO Vendedor { get; set; }
        
        // Detalles del Pago
        public decimal Monto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public string? ArchivoComprobante { get; set; }
        public string? NumeroTransaccion { get; set; }
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool NotificacionEnviada { get; set; }
        
        // Información adicional
        public bool PuedeActualizar { get; set; }
        public bool PuedeCancelar { get; set; }
        public List<string> HistorialEstados { get; set; }
    }

    // DTO para Historial de Compras
    public class PagoHistorialDTO
    {
        public int Id { get; set; }
        public DateTime FechaPago { get; set; }
        public string NombreTorta { get; set; }
        public string NombreVendedor { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public EstadoPago Estado { get; set; }
        public string EstadoTexto { get; set; }
        public string? ImagenTorta { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public bool PuedeCalificar { get; set; }
    }

    // DTO para Dashboard del Vendedor
    public class PagoVendedorDTO
    {
        public int Id { get; set; }
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
        public string EmailComprador { get; set; }
        public string TelefonoComprador { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public bool TieneComprobante { get; set; }
        public string? ArchivoComprobante { get; set; }
    }

    // DTO para Dashboard del Comprador
    public class PagoCompradorDTO
    {
        public int Id { get; set; }
        public string NombreTorta { get; set; }
        public string NombreVendedor { get; set; }
        public string TelefonoVendedor { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? ImagenTorta { get; set; }
        public bool PuedeSubirComprobante { get; set; }
        public bool PuedeCancelar { get; set; }
    }

    // DTO para Estadísticas
    public class PagoEstadisticasDTO
    {
        // Totales generales
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal PromedioVenta { get; set; }
        
        // Por estado
        public int VentasPendientes { get; set; }
        public int VentasCompletadas { get; set; }
        public int VentasCanceladas { get; set; }
        
        // Montos por estado
        public decimal MontoPendiente { get; set; }
        public decimal MontoCompletado { get; set; }
        public decimal MontoCancelado { get; set; }
        
        // Estadísticas por período
        public Dictionary<string, decimal> VentasPorMes { get; set; }
        public Dictionary<string, int> CantidadPorMes { get; set; }
        public Dictionary<string, decimal> VentasPorDia { get; set; }
        
        // Por categoría
        public Dictionary<string, int> VentasPorCategoria { get; set; }
        public Dictionary<string, decimal> MontosPorCategoria { get; set; }
        
        // Top productos
        public List<TortaMasVendidaDTO> TortasMasVendidas { get; set; }
        
        // Top compradores
        public List<CompradorFrecuenteDTO> CompradoresFrecuentes { get; set; }
    }

    // DTOs auxiliares para estadísticas
    public class TortaMasVendidaDTO
    {
        public int TortaId { get; set; }
        public string NombreTorta { get; set; }
        public int CantidadVendida { get; set; }
        public decimal MontoTotal { get; set; }
        public string? ImagenTorta { get; set; }
    }

    public class CompradorFrecuenteDTO
    {
        public int CompradorId { get; set; }
        public string NombreComprador { get; set; }
        public int CantidadCompras { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime UltimaCompra { get; set; }
    }

    // DTO para confirmar pago
    public class ConfirmarPagoDTO
    {
        public int PagoId { get; set; }
        public string NumeroTransaccion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public IFormFile? ArchivoComprobante { get; set; }
        public string? NotasConfirmacion { get; set; }
    }

    // DTO para cancelar pago
    public class CancelarPagoDTO
    {
        public int PagoId { get; set; }
        public string MotivoCancelacion { get; set; }
        public bool NotificarComprador { get; set; }
    }

    // DTO para subir comprobante
    public class SubirComprobanteDTO
    {
        public int PagoId { get; set; }
        public IFormFile ArchivoComprobante { get; set; }
        public string? NumeroTransaccion { get; set; }
    }

    // DTO para filtros de búsqueda
    public class PagoFiltrosDTO
    {
        public int? VendedorId { get; set; }
        public int? CompradorId { get; set; }
        public int? TortaId { get; set; }
        public EstadoPago? Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public decimal? MontoMinimo { get; set; }
        public decimal? MontoMaximo { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
        public bool OrdenDescendente { get; set; }
    }

    // DTO para resumen de pago
    public class PagoResumenDTO
    {
        public int TotalPagos { get; set; }
        public decimal MontoTotal { get; set; }
        public int PagosPendientes { get; set; }
        public int PagosCompletados { get; set; }
        public int PagosCancelados { get; set; }
        public decimal PromedioMonto { get; set; }
        public DateTime? UltimoPago { get; set; }
    }

    // DTO para notificación de pago
    public class PagoNotificacionDTO
    {
        public int PagoId { get; set; }
        public string TipoNotificacion { get; set; } // "Nuevo", "Actualizado", "Cancelado", "Completado"
        public string Mensaje { get; set; }
        public int DestinatarioId { get; set; }
        public string RolDestinatario { get; set; } // "Vendedor" o "Comprador"
        public DateTime FechaNotificacion { get; set; }
    }
}