using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public enum EstadoPago
    {
        [Display(Name = "Pendiente")]
        Pendiente,
        [Display(Name = "Completado")]
        Completado,
        [Display(Name = "Cancelado")]
        Cancelado
    }

    public enum MetodoPago
    {
        [Display(Name = "Transferencia Bancaria")]
        Transferencia,
        [Display(Name = "Efectivo")]
        Efectivo,
        [Display(Name = "Tarjeta de Crédito")]
        TarjetaCredito,
        [Display(Name = "Tarjeta de Débito")]
        TarjetaDebito,
        [Display(Name = "MercadoPago")]
        MercadoPago
    }

    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Torta")]
        public int TortaId { get; set; }

        [Required]
        [ForeignKey("Comprador")]
        public int CompradorId { get; set; }

        [Required]
        [ForeignKey("Vendedor")]
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999999.99")]
        [Display(Name = "Monto Total")]
        [Column(TypeName = "decimal(10, 2)")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Required]
        [Display(Name = "Precio Unitario")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Descuento")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Descuento { get; set; } = 0;

        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Estado del Pago")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }

        [StringLength(500)]
        [Display(Name = "Archivo del Comprobante")]
        public string? ArchivoComprobante { get; set; }

        [StringLength(100)]
        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección de Entrega")]
        public string? DireccionEntrega { get; set; }

        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        [Display(Name = "Notificación Enviada")]
        public bool NotificacionEnviada { get; set; } = false;

        // Propiedades de navegación
        public virtual Torta Torta { get; set; }
        public virtual Comprador Comprador { get; set; }
        public virtual Vendedor Vendedor { get; set; }
    }
}