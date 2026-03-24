using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Estados de la liberación de fondos a un vendedor
    /// </summary>
    public enum EstadoLiberacion
    {
        [Display(Name = "Pendiente")]
        Pendiente,              // Esperando que se confirme la entrega

        [Display(Name = "Listo para Liberar")]
        ListoParaLiberar,       // Entrega confirmada, Admin puede transferir

        [Display(Name = "En Proceso")]
        EnProceso,              // Admin inició la transferencia

        [Display(Name = "Transferido")]
        Transferido,            // Admin transfirió el dinero

        [Display(Name = "Confirmado")]
        Confirmado,             // Vendedor confirmó que recibió el dinero

        [Display(Name = "Cancelado")]
        Cancelado               // Cancelado (por disputa, reembolso, etc.)
    }

    /// <summary>
    /// Representa la liberación de fondos de la plataforma hacia un vendedor
    /// Cada venta con múltiples vendedores genera múltiples registros de liberación
    /// </summary>
    public class LiberacionFondos
    {
        [Key]
        public int Id { get; set; }

        // ==================== RELACIONES ====================

        [Required]
        [ForeignKey("Venta")]
        public int VentaId { get; set; }

        [Required]
        [ForeignKey("Vendedor")]
        public int VendedorId { get; set; }

        /// <summary>
        /// Administrador que procesó la liberación
        /// </summary>
        [ForeignKey("ProcesadoPor")]
        public int? ProcesadoPorId { get; set; }

        // ==================== MONTOS ====================

        /// <summary>
        /// Monto bruto de los productos del vendedor en esta venta
        /// </summary>
        [Required]
        [Display(Name = "Monto Bruto")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoBruto { get; set; }

        /// <summary>
        /// Comisión descontada por la plataforma
        /// </summary>
        [Required]
        [Display(Name = "Comisión")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Comision { get; set; }

        /// <summary>
        /// Monto neto a recibir por el vendedor (MontoBruto - Comision)
        /// </summary>
        [Required]
        [Display(Name = "Monto Neto")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoNeto { get; set; }

        // ==================== ESTADO ====================

        [Required]
        [Display(Name = "Estado")]
        public EstadoLiberacion Estado { get; set; } = EstadoLiberacion.Pendiente;

        // ==================== FECHAS ====================

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Lista para Liberar")]
        public DateTime? FechaListoParaLiberar { get; set; }

        [Display(Name = "Fecha de Transferencia")]
        public DateTime? FechaTransferencia { get; set; }

        [Display(Name = "Fecha de Confirmación")]
        public DateTime? FechaConfirmacion { get; set; }

        // ==================== DATOS DE TRANSFERENCIA ====================

        /// <summary>
        /// Número de operación/transacción de la transferencia
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Número de Operación")]
        public string? NumeroOperacion { get; set; }

        /// <summary>
        /// Comprobante de la transferencia (imagen)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Comprobante de Transferencia")]
        public string? ArchivoComprobante { get; set; }

        // ==================== DATOS DEL VENDEDOR (AL MOMENTO DE LA LIBERACIÓN) ====================

        /// <summary>
        /// Alias al que se transfirió (snapshot)
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Alias Destino")]
        public string? AliasDestino { get; set; }

        /// <summary>
        /// CBU al que se transfirió (snapshot)
        /// </summary>
        [StringLength(22)]
        [Display(Name = "CBU Destino")]
        public string? CBUDestino { get; set; }

        /// <summary>
        /// Titular de la cuenta destino (snapshot)
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Titular Destino")]
        public string? TitularDestino { get; set; }

        // ==================== OBSERVACIONES ====================

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================

        public virtual Venta Venta { get; set; } = null!;
        public virtual Vendedor Vendedor { get; set; } = null!;
        public virtual Persona? ProcesadoPor { get; set; }

        // ==================== PROPIEDADES CALCULADAS ====================

        /// <summary>
        /// Indica si el admin puede procesar esta liberación
        /// </summary>
        [NotMapped]
        public bool PuedeProcesar => Estado == EstadoLiberacion.ListoParaLiberar;

        /// <summary>
        /// Indica si el vendedor puede confirmar la recepción
        /// </summary>
        [NotMapped]
        public bool PuedeConfirmar => Estado == EstadoLiberacion.Transferido;

        /// <summary>
        /// Porcentaje de comisión aplicado
        /// </summary>
        [NotMapped]
        public decimal PorcentajeComision => MontoBruto > 0 ? Math.Round((Comision / MontoBruto) * 100, 2) : 0;

        // ==================== MÉTODOS ====================

        /// <summary>
        /// Crea un registro de liberación para un vendedor específico de una venta
        /// </summary>
        public static LiberacionFondos Crear(Venta venta, int vendedorId, decimal montoBruto, decimal porcentajeComision)
        {
            var comision = Math.Round(montoBruto * (porcentajeComision / 100), 2);
            
            return new LiberacionFondos
            {
                VentaId = venta.Id,
                VendedorId = vendedorId,
                MontoBruto = montoBruto,
                Comision = comision,
                MontoNeto = montoBruto - comision,
                Estado = EstadoLiberacion.Pendiente,
                FechaCreacion = DateTime.Now
            };
        }

        /// <summary>
        /// Marca como listo para liberar (cuando se confirma la entrega)
        /// </summary>
        public void MarcarListoParaLiberar()
        {
            Estado = EstadoLiberacion.ListoParaLiberar;
            FechaListoParaLiberar = DateTime.Now;
        }

        /// <summary>
        /// Registra la transferencia realizada por el admin
        /// </summary>
        public void RegistrarTransferencia(int adminId, string numeroOperacion, string? archivoComprobante, Vendedor vendedor)
        {
            Estado = EstadoLiberacion.Transferido;
            FechaTransferencia = DateTime.Now;
            ProcesadoPorId = adminId;
            NumeroOperacion = numeroOperacion;
            ArchivoComprobante = archivoComprobante;
            
            // Guardar snapshot de datos del vendedor
            AliasDestino = vendedor.AliasCBU;
            CBUDestino = vendedor.CBU;
            TitularDestino = vendedor.TitularCuenta;
        }

        /// <summary>
        /// Confirma que el vendedor recibió el dinero
        /// </summary>
        public void ConfirmarRecepcion()
        {
            Estado = EstadoLiberacion.Confirmado;
            FechaConfirmacion = DateTime.Now;
        }
    }
}