using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Models.ViewModels
{
    // =============================================
    // VIEWMODELS PARA PROCESAR PAGO
    // =============================================

    public class ProcesarPagoViewModel
    {
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public List<MetodoPago> MetodosDisponibles { get; set; } = new();
        
        // Datos del comprador (solo lectura)
        public int CompradorId { get; set; }
        public string CompradorNombre { get; set; } = string.Empty;
        public string CompradorEmail { get; set; } = string.Empty;
    }

    public class RealizarPagoViewModel
    {
        [Required]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "Seleccione un método de pago")]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; }

        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        [Display(Name = "Comprobante de Pago")]
        public IFormFile? ArchivoComprobante { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }
    }

    public class ConfirmacionPagoViewModel
    {
        public int PagoId { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    // =============================================
    // VIEWMODEL PARA CONFIRMAR PAGO (VENDEDOR)
    // =============================================

    public class ConfirmarPagoViewModel
    {
        [Required]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El nombre de la torta es requerido")]
        [Display(Name = "Torta")]
        public string NombreTorta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Display(Name = "Cantidad")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        [Display(Name = "Comprobante de Pago")]
        public IFormFile? ArchivoComprobante { get; set; }

        [Display(Name = "Notas de Confirmación")]
        [DataType(DataType.MultilineText)]
        public string? NotasConfirmacion { get; set; }
    }

    // =============================================
    // VIEWMODEL PARA LISTADO DE PAGOS
    // =============================================

    public class PagoListViewModel
    {
        public List<PagoItemViewModel> Pagos { get; set; } = new();
        public PaginacionViewModel Paginacion { get; set; } = new();
        
        // Filtros
        public EstadoPago? FiltroEstado { get; set; }
        public MetodoPago? FiltroMetodoPago { get; set; }
        public int? FiltroVendedorId { get; set; }
        public int? FiltroCompradorId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
        
        // Estadísticas
        public decimal TotalMonto { get; set; }
        public int TotalPagos { get; set; }
        public Dictionary<EstadoPago, int>? PagosPorEstado { get; set; }
    }

    public class PagoItemViewModel
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string NombreComprador { get; set; } = string.Empty;
        public string NombreTorta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public EstadoPago Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public bool TieneComprobante { get; set; }
        public string? ImagenTorta { get; set; }

        public string EstadoDisplay => Estado.ToString();
        public string EstadoColor => Estado switch
        {
            EstadoPago.Pendiente => "warning",
            EstadoPago.Completado => "success",
            EstadoPago.Cancelado => "danger",
            _ => "secondary"
        };
    }

    // =============================================
    // VIEWMODEL PARA CANCELAR PAGO
    // =============================================

    public class CancelarPagoViewModel
    {
        [Required]
        public int PagoId { get; set; }

        [Required]
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public string NombreTorta { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Comprador")]
        public string NombreComprador { get; set; } = string.Empty;

        [Display(Name = "Motivo de Cancelación")]
        public string? MotivoCancelacion { get; set; }
    }

    public class ProcesarCancelacionViewModel
    {
        [Required]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un motivo de cancelación")]
        [Display(Name = "Motivo de Cancelación")]
        public string MotivoCancelacion { get; set; } = string.Empty;

        [Display(Name = "Comentarios adicionales")]
        [DataType(DataType.MultilineText)]
        public string? Comentarios { get; set; }

        [Display(Name = "Notificar al comprador")]
        public bool NotificarComprador { get; set; } = true;
    }

    // =============================================
    // VIEWMODEL PARA DETALLE DE PAGO
    // =============================================

    public class PagoDetalleViewModel
    {
        public PagoViewModel Pago { get; set; } = new();
        public VentaResumenViewModel Venta { get; set; } = new();
        public CompradorViewModel Comprador { get; set; } = new();
        public bool PuedeEditar { get; set; }
        public bool PuedeCancelar { get; set; }
        public bool PuedeConfirmar { get; set; }
        public bool PuedeSubirComprobante { get; set; }
        public List<DetalleVentaViewModel>? DetallesVenta { get; set; }
    }

    public class PagoViewModel
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int CompradorId { get; set; }
        public int? VendedorId { get; set; }
        
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }
        
        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.DateTime)]
        public DateTime FechaPago { get; set; }
        
        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; }
        
        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }
        
        [Display(Name = "Comprobante")]
        public string? ArchivoComprobante { get; set; }
        
        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }
        
        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }
        
        [Display(Name = "Fecha de Confirmación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaConfirmacion { get; set; }
        
        [Display(Name = "Confirmado Por")]
        public int? ConfirmadoPorVendedorId { get; set; }

        public string? NombreComprador { get; set; }
        public string? EmailComprador { get; set; }
        public string? TelefonoComprador { get; set; }
        public string? NumeroOrden { get; set; }
    }
}