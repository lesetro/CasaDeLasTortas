using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class PagoViewModel
    {
        public int Id { get; set; }
        public int TortaId { get; set; }
        public int CompradorId { get; set; }
        public int VendedorId { get; set; }

        [Display(Name = "Monto Total")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Display(Name = "Precio Unitario")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }

        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        public decimal Descuento { get; set; }

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

        [Display(Name = "Dirección de Entrega")]
        public string? DireccionEntrega { get; set; }

        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }

        // Información relacionada
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
        public string NombreVendedor { get; set; }
        public string? ImagenTorta { get; set; }
        public string? TelefonoComprador { get; set; }
        public string? EmailComprador { get; set; }
    }

    public class PagoCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una torta")]
        [Display(Name = "Torta")]
        public int TortaId { get; set; }

        public int CompradorId { get; set; }
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        [Range(0, 999999.99)]
        public decimal Descuento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; }

        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La dirección de entrega es requerida")]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; }

        [Display(Name = "Fecha de Entrega Deseada")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Comprobante de Pago")]
        public IFormFile? ArchivoComprobante { get; set; }

        // Para mostrar en la vista
        public TortaViewModel? Torta { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class PagoEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Estado del Pago")]
        public EstadoPago Estado { get; set; }

        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }

        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        [Display(Name = "Dirección de Entrega")]
        public string? DireccionEntrega { get; set; }

        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Nuevo Comprobante")]
        public IFormFile? NuevoComprobante { get; set; }

        [Display(Name = "Comprobante Actual")]
        public string? ComprobanteActual { get; set; }

        // Información para mostrar (solo lectura)
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaPago { get; set; }
    }

    public class PagoListViewModel
    {
        public List<PagoViewModel> Pagos { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
        
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
        public Dictionary<EstadoPago, int> PagosPorEstado { get; set; }
    }

    public class PagoDetalleViewModel
    {
        public PagoViewModel Pago { get; set; }
        public TortaViewModel Torta { get; set; }
        public CompradorViewModel Comprador { get; set; }
        public VendedorViewModel Vendedor { get; set; }
        
        // Permisos
        public bool PuedeEditar { get; set; }
        public bool PuedeCancelar { get; set; }
        public bool PuedeConfirmar { get; set; }
        public bool PuedeSubirComprobante { get; set; }
    }

    public class ConfirmarPagoViewModel
    {
        [Required]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "El número de transacción es requerido")]
        [Display(Name = "Número de Transacción")]
        public string NumeroTransaccion { get; set; }

        [Display(Name = "Comprobante de Pago")]
        public IFormFile? ArchivoComprobante { get; set; }

        [Display(Name = "Notas de Confirmación")]
        [DataType(DataType.MultilineText)]
        public string? NotasConfirmacion { get; set; }

        // Info para mostrar
        public decimal Monto { get; set; }
        public string NombreTorta { get; set; }
        public int Cantidad { get; set; }
    }

    public class CancelarPagoViewModel
    {
        [Required]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "Debe proporcionar un motivo de cancelación")]
        [Display(Name = "Motivo de Cancelación")]
        [DataType(DataType.MultilineText)]
        public string MotivoCancelacion { get; set; }

        [Display(Name = "Notificar al Comprador")]
        public bool NotificarComprador { get; set; } = true;

        // Info para mostrar
        public decimal Monto { get; set; }
        public string NombreTorta { get; set; }
        public string NombreComprador { get; set; }
    }
}