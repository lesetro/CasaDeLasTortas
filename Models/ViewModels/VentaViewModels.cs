using CasaDeLasTortas.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CasaDeLasTortas.Models.ViewModels
{
    
    // =============================================
    // VIEWMODELS PARA MIS COMPRAS (COMPRADOR)
    // =============================================

    public class MisComprasViewModel
    {
        public List<VentaResumenViewModel> Ventas { get; set; } = new();
        public PaginacionViewModel Paginacion { get; set; } = new();
    }

    public class VentaResumenViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Orden")]
        public string NumeroOrden { get; set; } = string.Empty;
        
        [Display(Name = "Fecha de Venta")]
        [DataType(DataType.DateTime)]
        public DateTime FechaVenta { get; set; }
        
        [Display(Name = "Estado")]
        public EstadoVenta Estado { get; set; }
        
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
        
        [Display(Name = "Total Items")]
        public int TotalItems { get; set; }
        
        [Display(Name = "Imagen Principal")]
        public string? ImagenPrincipal { get; set; }
        
        // Propiedades calculadas
        public string EstadoDisplay => Estado.ToString();
        
        // ✅ CORREGIDO: ListaParaEnvio → ListaParaRetiro
        public string EstadoColor => Estado switch
        {
            EstadoVenta.Pendiente => "warning",
            EstadoVenta.PagoEnRevision => "info",
            EstadoVenta.Pagada => "info",
            EstadoVenta.EnPreparacion => "primary",
            EstadoVenta.ListaParaRetiro => "secondary",  // ✅ CORREGIDO
            EstadoVenta.Enviada => "info",
            EstadoVenta.Entregada => "success",
            EstadoVenta.Cancelada => "danger",
            EstadoVenta.EnDisputa => "warning",
            EstadoVenta.Reembolsada => "dark",
            _ => "light"
        };
    }

    public class VentaDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public EstadoVenta Estado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public string DireccionEntrega { get; set; } = string.Empty;
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? NotasCliente { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }
        public DateTime? FechaEntregaReal { get; set; }
        public bool RequiereFactura { get; set; }
        
        // ✅ CORREGIDO: Campo correcto para Argentina
        public string? CUITCliente { get; set; }
        public string? RazonSocial { get; set; }

        // Datos del comprador
        public int CompradorId { get; set; }
        public string NombreComprador { get; set; } = string.Empty;
        public string EmailComprador { get; set; } = string.Empty;
        public string TelefonoComprador { get; set; } = string.Empty;

        // ✅ NUEVO: Campos de comisiones (usados en VentaController.Details)
        public decimal PorcentajeComision { get; set; }
        public decimal ComisionPlataforma { get; set; }
        public decimal MontoVendedores { get; set; }
        public bool FondosLiberados { get; set; }

        // ✅ NUEVO: Datos del pago principal (usados en VentaController.Details)
        public int? PagoId { get; set; }
        public EstadoPago EstadoPago { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal MontoPagado { get; set; }

        // Detalles de productos
        public List<DetalleVentaViewModel> Detalles { get; set; } = new();

        // Pagos (múltiples)
        public List<PagoResumenViewModel> Pagos { get; set; } = new();

        // Permisos
        public bool PuedeCancelar { get; set; }
        public bool PuedePagar { get; set; }
        public bool PuedeVerFactura { get; set; }

        // Propiedades calculadas
        public string EstadoDisplay => Estado.ToString();
        
        // ✅ CORREGIDO: ListaParaEnvio → ListaParaRetiro
        public string EstadoColor => Estado switch
        {
            EstadoVenta.Pendiente => "warning",
            EstadoVenta.PagoEnRevision => "info",
            EstadoVenta.Pagada => "info",
            EstadoVenta.EnPreparacion => "primary",
            EstadoVenta.ListaParaRetiro => "secondary",  // ✅ CORREGIDO
            EstadoVenta.Enviada => "info",
            EstadoVenta.Entregada => "success",
            EstadoVenta.Cancelada => "danger",
            EstadoVenta.EnDisputa => "warning",
            EstadoVenta.Reembolsada => "dark",
            _ => "light"
        };
        
        public bool PagosCompletados => Pagos.Any(p => p.Estado == EstadoPago.Completado);
        public decimal TotalPagado => Pagos.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto);
        public decimal SaldoPendiente => Total - TotalPagado;
    }

    // =============================================
    // VIEWMODELS PARA DETALLE DE VENTA
    // =============================================

    public class DetalleVentaViewModel
    {
        public int Id { get; set; }
        public int TortaId { get; set; }
        
        [Display(Name = "Producto")]
        public string NombreTorta { get; set; } = string.Empty;
        
        public int VendedorId { get; set; }
        
        [Display(Name = "Vendedor")]
        public string NombreVendedor { get; set; } = string.Empty;
        
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }
        
        [Display(Name = "Precio Unitario")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }
        
        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        public decimal Descuento { get; set; }
        
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }
        
        [Display(Name = "Estado")]
        public EstadoDetalleVenta Estado { get; set; }
        
        [Display(Name = "Notas de Personalización")]
        [DataType(DataType.MultilineText)]
        public string? NotasPersonalizacion { get; set; }
        
        [Display(Name = "Fecha Estimada")]
        [DataType(DataType.Date)]
        public DateTime? FechaEstimadaPreparacion { get; set; }
        
        [Display(Name = "Fecha Real")]
        [DataType(DataType.Date)]
        public DateTime? FechaRealPreparacion { get; set; }
        
        [Display(Name = "Imagen")]
        public string? ImagenTorta { get; set; }

        public string EstadoDisplay => Estado.ToString();
        public string EstadoColor => Estado switch
        {
            EstadoDetalleVenta.Pendiente => "warning",
            EstadoDetalleVenta.Confirmado => "info",
            EstadoDetalleVenta.EnPreparacion => "primary",
            EstadoDetalleVenta.Listo => "secondary",
            EstadoDetalleVenta.Entregado => "success",
            EstadoDetalleVenta.Cancelado => "danger",
            _ => "light"
        };
    }

    public class PagoResumenViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }
        
        [Display(Name = "Fecha")]
        [DataType(DataType.DateTime)]
        public DateTime FechaPago { get; set; }
        
        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; }
        
        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }
        
        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }
        
        [Display(Name = "Comprobante")]
        public string? ArchivoComprobante { get; set; }
        
        public string EstadoDisplay => Estado.ToString();
        public string MetodoDisplay => MetodoPago?.ToString() ?? "No especificado";
        
        public string EstadoColor => Estado switch
        {
            EstadoPago.Pendiente => "warning",
            EstadoPago.EnRevision => "info",
            EstadoPago.Verificado => "primary",
            EstadoPago.Completado => "success",
            EstadoPago.Rechazado => "danger",
            EstadoPago.Cancelado => "secondary",
            EstadoPago.ReembolsoPendiente => "warning",
            EstadoPago.Reembolsado => "dark",
            _ => "light"
        };
    }

    // =============================================
    // VIEWMODELS PARA MIS PEDIDOS (VENDEDOR)
    // =============================================

    public class MisPedidosViewModel
    {
        public List<PedidoVendedorViewModel> Pedidos { get; set; } = new();
        public PaginacionViewModel Paginacion { get; set; } = new();
        public string? FiltroEstado { get; set; }
        
        // Estadísticas rápidas
        public int PendientesCount { get; set; }
        public int EnPreparacionCount { get; set; }
        public int ListosCount { get; set; }
        public int EntregadosCount { get; set; }
        public int CanceladosCount { get; set; }
        
        // Total de pedidos
        public int TotalPedidos => PendientesCount + EnPreparacionCount + ListosCount + EntregadosCount + CanceladosCount;
    }

    public class PedidoVendedorViewModel
    {
        public int DetalleId { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public int TortaId { get; set; }
        public string NombreTorta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public EstadoDetalleVenta Estado { get; set; }
        public string? NotasPersonalizacion { get; set; }
        public string? ImagenTorta { get; set; }
        public string DireccionEntrega { get; set; } = string.Empty;
        public string NombreComprador { get; set; } = string.Empty;
        public string TelefonoComprador { get; set; } = string.Empty;
        public DateTime? FechaEntregaEstimada { get; set; }

        public string EstadoDisplay => Estado.ToString();
        public string EstadoColor => Estado switch
        {
            EstadoDetalleVenta.Pendiente => "warning",
            EstadoDetalleVenta.Confirmado => "info",
            EstadoDetalleVenta.EnPreparacion => "primary",
            EstadoDetalleVenta.Listo => "secondary",
            EstadoDetalleVenta.Entregado => "success",
            EstadoDetalleVenta.Cancelado => "danger",
            _ => "light"
        };
        
        // Propiedades adicionales útiles
        public bool EsUrgente => FechaEntregaEstimada.HasValue && FechaEntregaEstimada.Value <= DateTime.Now.AddHours(24);
        public bool TienePersonalizacion => !string.IsNullOrEmpty(NotasPersonalizacion);
    }
}