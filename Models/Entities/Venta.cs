using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Estados de una venta en el flujo de compra
    /// </summary>
    public enum EstadoVenta
    {
        [Display(Name = "Pendiente de Pago")]
        Pendiente,

        [Display(Name = "Pago en Revisión")]
        PagoEnRevision,

        [Display(Name = "Pagada")]
        Pagada,

        [Display(Name = "En Preparación")]
        EnPreparacion,

        [Display(Name = "Lista para Retiro")]
        ListaParaRetiro,

        [Display(Name = "Enviada")]
        Enviada,

        [Display(Name = "Entregada")]
        Entregada,

        [Display(Name = "Cancelada")]
        Cancelada,

        [Display(Name = "En Disputa")]
        EnDisputa,

        [Display(Name = "Reembolsada")]
        Reembolsada
    }

    /// <summary>
    /// Representa una orden de compra completa
    /// Puede incluir tortas de múltiples vendedores
    /// </summary>
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        // ==================== RELACIONES ====================

        [Required]
        [ForeignKey("Comprador")]
        public int CompradorId { get; set; }

        // ==================== IDENTIFICACIÓN ====================

        [Required]
        [StringLength(20)]
        [Display(Name = "Número de Orden")]
        public string NumeroOrden { get; set; } = string.Empty; // Ej: ORD-20250308-001

        // ==================== FECHAS ====================

        [Required]
        [Display(Name = "Fecha de Venta")]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Display(Name = "Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        [Display(Name = "Fecha de Entrega Estimada")]
        public DateTime? FechaEntregaEstimada { get; set; }

        [Display(Name = "Fecha de Entrega Real")]
        public DateTime? FechaEntregaReal { get; set; }

        // ==================== ESTADO ====================

        [Required]
        [Display(Name = "Estado")]
        public EstadoVenta Estado { get; set; } = EstadoVenta.Pendiente;

        // ==================== MONTOS (ORIGINAL) ====================

        /// <summary>
        /// Suma de todos los productos sin descuentos
        /// </summary>
        [Required]
        [Display(Name = "Subtotal")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total de descuentos aplicados
        /// </summary>
        [Display(Name = "Descuento Total")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DescuentoTotal { get; set; } = 0;

        /// <summary>
        /// Monto total que paga el comprador (Subtotal - Descuentos)
        /// </summary>
        [Required]
        [Display(Name = "Total")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Total { get; set; }

        // ==================== COMISIONES Y DISTRIBUCIÓN (NUEVO) ====================

        /// <summary>
        /// Porcentaje de comisión aplicado (ej: 10.00 = 10%)
        /// </summary>
        [Display(Name = "Porcentaje Comisión")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal PorcentajeComision { get; set; } = 10.00m; // 10% por defecto

        /// <summary>
        /// Monto total de comisión para la plataforma
        /// </summary>
        [Display(Name = "Comisión Plataforma")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ComisionPlataforma { get; set; } = 0;

        /// <summary>
        /// Monto total a distribuir entre vendedores (Total - ComisionPlataforma)
        /// </summary>
        [Display(Name = "Monto para Vendedores")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoVendedores { get; set; } = 0;

        /// <summary>
        /// Indica si los fondos ya fueron liberados a los vendedores
        /// </summary>
        [Display(Name = "Fondos Liberados")]
        public bool FondosLiberados { get; set; } = false;

        /// <summary>
        /// Fecha en que se liberaron los fondos
        /// </summary>
        [Display(Name = "Fecha Liberación")]
        public DateTime? FechaLiberacion { get; set; }

        // ==================== DIRECCIÓN DE ENTREGA ====================

        [Required]
        [StringLength(200)]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }

        [StringLength(50)]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [StringLength(10)]
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        // ==================== NOTAS ====================

        [StringLength(500)]
        [Display(Name = "Notas del Cliente")]
        [DataType(DataType.MultilineText)]
        public string? NotasCliente { get; set; }

        [StringLength(500)]
        [Display(Name = "Notas Internas")]
        [DataType(DataType.MultilineText)]
        public string? NotasInternas { get; set; }

        // ==================== FACTURACIÓN ====================

        [Display(Name = "Requiere Factura")]
        public bool RequiereFactura { get; set; } = false;

        [StringLength(13)]
        [Display(Name = "CUIT Cliente")]
        public string? CUITCliente { get; set; }

        [StringLength(100)]
        [Display(Name = "Razón Social")]
        public string? RazonSocial { get; set; }

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================

        public virtual Comprador Comprador { get; set; } = null!;
        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Disputa> Disputas { get; set; } = new List<Disputa>();

        // ==================== PROPIEDADES CALCULADAS ====================

        /// <summary>
        /// Obtiene el pago principal (el más reciente válido)
        /// </summary>
        [NotMapped]
        public Pago? PagoPrincipal => Pagos?
            .Where(p => p.Estado != EstadoPago.Cancelado && p.Estado != EstadoPago.Rechazado)
            .OrderByDescending(p => p.FechaPago)
            .FirstOrDefault();

        /// <summary>
        /// Indica si la venta está pagada (verificado por admin)
        /// </summary>
        [NotMapped]
        public bool EstaPagada => PagoPrincipal?.Estado == EstadoPago.Verificado ||
                                   PagoPrincipal?.Estado == EstadoPago.Completado;

        /// <summary>
        /// Indica si la venta puede ser cancelada
        /// </summary>
        [NotMapped]
        public bool PuedeCancelarse => Estado == EstadoVenta.Pendiente || 
                                        Estado == EstadoVenta.PagoEnRevision;

        /// <summary>
        /// Obtiene la cantidad total de items
        /// </summary>
        [NotMapped]
        public int TotalItems => Detalles?.Sum(d => d.Cantidad) ?? 0;

        /// <summary>
        /// Obtiene los IDs únicos de vendedores en esta venta
        /// </summary>
        [NotMapped]
        public IEnumerable<int> VendedoresIds => Detalles?.Select(d => d.VendedorId).Distinct() ?? Enumerable.Empty<int>();

        /// <summary>
        /// Cantidad de vendedores diferentes en esta venta
        /// </summary>
        [NotMapped]
        public int CantidadVendedores => VendedoresIds.Count();

        // ==================== MÉTODOS ====================

        /// <summary>
        /// Calcula y establece las comisiones basadas en el porcentaje configurado
        /// </summary>
        public void CalcularComisiones()
        {
            ComisionPlataforma = Math.Round(Total * (PorcentajeComision / 100), 2);
            MontoVendedores = Total - ComisionPlataforma;
        }

        /// <summary>
        /// Calcula cuánto corresponde a cada vendedor (proporcional a sus productos)
        /// </summary>
        public Dictionary<int, decimal> CalcularMontosPorVendedor()
        {
            var montos = new Dictionary<int, decimal>();
            
            if (Detalles == null || !Detalles.Any()) return montos;

            foreach (var vendedorId in VendedoresIds)
            {
                var subtotalVendedor = Detalles
                    .Where(d => d.VendedorId == vendedorId)
                    .Sum(d => d.Subtotal);
                
                // Proporción de este vendedor sobre el total
                var proporcion = subtotalVendedor / Subtotal;
                
                // Monto que le corresponde (menos su parte de comisión)
                var montoVendedor = Math.Round(subtotalVendedor * (1 - PorcentajeComision / 100), 2);
                
                montos[vendedorId] = montoVendedor;
            }

            return montos;
        }
    }
}