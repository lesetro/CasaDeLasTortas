using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public enum EstadoDetalleVenta
    {
        [Display(Name = "Pendiente")]
        Pendiente,
        [Display(Name = "Confirmado")]
        Confirmado,
        [Display(Name = "En Preparación")]
        EnPreparacion,
        [Display(Name = "Listo")]
        Listo,
        [Display(Name = "Entregado")]
        Entregado,
        [Display(Name = "Cancelado")]
        Cancelado
    }

    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Venta")]
        public int VentaId { get; set; }

        [Required]
        [ForeignKey("Torta")]
        public int TortaId { get; set; }

        [Required]
        [ForeignKey("Vendedor")]
        public int VendedorId { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Display(Name = "Precio Unitario")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Descuento")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Descuento { get; set; } = 0;

        [Required]
        [Display(Name = "Subtotal")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; } // Cantidad * PrecioUnitario - Descuento

        [Required]
        [Display(Name = "Estado del Producto")]
        public EstadoDetalleVenta Estado { get; set; } = EstadoDetalleVenta.Pendiente;

        [StringLength(500)]
        [Display(Name = "Notas de Personalización")]
        [DataType(DataType.MultilineText)]
        public string? NotasPersonalizacion { get; set; }

        [Display(Name = "Fecha Estimada de Preparación")]
        public DateTime? FechaEstimadaPreparacion { get; set; }

        [Display(Name = "Fecha Real de Preparación")]
        public DateTime? FechaRealPreparacion { get; set; }

        // Propiedades de navegación
        public virtual Venta Venta { get; set; }
        public virtual Torta Torta { get; set; }
        public virtual Vendedor Vendedor { get; set; }
    }
}