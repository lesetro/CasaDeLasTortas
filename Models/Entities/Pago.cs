using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Estados del pago en el flujo de compra
    /// </summary>
    public enum EstadoPago
    {
        [Display(Name = "Pendiente de Pago")]
        Pendiente,              // Orden creada, esperando que comprador pague

        [Display(Name = "En Revisión")]
        EnRevision,             // Comprador subió comprobante, Admin debe verificar

        [Display(Name = "Pago Verificado")]
        Verificado,             // Admin confirmó que el dinero llegó a la plataforma

        [Display(Name = "Completado")]
        Completado,             // Proceso completo (entregado y fondos liberados)

        [Display(Name = "Rechazado")]
        Rechazado,              // Admin rechazó el comprobante (inválido/falso)

        [Display(Name = "Cancelado")]
        Cancelado,              // Orden cancelada

        [Display(Name = "Reembolso Pendiente")]
        ReembolsoPendiente,     // Se debe devolver dinero al comprador

        [Display(Name = "Reembolsado")]
        Reembolsado             // Dinero devuelto al comprador
    }

    /// <summary>
    /// Métodos de pago disponibles
    /// </summary>
    public enum MetodoPago
    {
        [Display(Name = "Transferencia Bancaria")]
        Transferencia,

        [Display(Name = "MercadoPago")]
        MercadoPago,

        [Display(Name = "Efectivo (en local)")]
        Efectivo,

        [Display(Name = "Tarjeta de Crédito")]
        TarjetaCredito,

        [Display(Name = "Tarjeta de Débito")]
        TarjetaDebito
    }

    /// <summary>
    /// Representa un pago realizado por un comprador a la plataforma
    /// El dinero va a la cuenta de la plataforma, luego se libera a los vendedores
    /// </summary>
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        // ==================== RELACIONES ====================

        [Required]
        [ForeignKey("Venta")]
        public int VentaId { get; set; }

        [Required]
        [ForeignKey("Comprador")]
        public int CompradorId { get; set; }

        // ==================== MONTOS ====================

        /// <summary>
        /// Monto total pagado por el comprador
        /// </summary>
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 99999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto Total")]
        [Column(TypeName = "decimal(12, 2)")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        /// <summary>
        /// Comisión total para la plataforma (suma de comisiones de todos los vendedores)
        /// </summary>
        [Display(Name = "Comisión Plataforma")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ComisionPlataforma { get; set; } = 0;

        /// <summary>
        /// Monto que se distribuirá entre los vendedores
        /// </summary>
        [Display(Name = "Monto para Vendedores")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoVendedores { get; set; } = 0;

        // ==================== ESTADO Y FECHAS ====================

        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Estado del Pago")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }

        [Display(Name = "Fecha de Verificación")]
        public DateTime? FechaVerificacion { get; set; }

        [Display(Name = "Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        // ==================== COMPROBANTE DEL COMPRADOR ====================

        /// <summary>
        /// Ruta del archivo del comprobante de pago subido por el comprador
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Comprobante de Pago")]
        public string? ArchivoComprobante { get; set; }

        /// <summary>
        /// Número de transacción/operación del comprobante
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Número de Transacción")]
        public string? NumeroTransaccion { get; set; }

        /// <summary>
        /// Fecha en que el comprador subió el comprobante
        /// </summary>
        [Display(Name = "Fecha Comprobante")]
        public DateTime? FechaComprobante { get; set; }

        // ==================== VERIFICACIÓN POR ADMIN ====================

        /// <summary>
        /// ID del administrador que verificó el pago
        /// </summary>
        [Display(Name = "Verificado Por")]
        public int? VerificadoPorId { get; set; }

        /// <summary>
        /// Observaciones del admin al verificar
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones Admin")]
        public string? ObservacionesAdmin { get; set; }

        // ==================== RECHAZO ====================

        /// <summary>
        /// Motivo por el cual se rechazó el comprobante
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Motivo de Rechazo")]
        public string? MotivoRechazo { get; set; }

        /// <summary>
        /// Fecha del rechazo
        /// </summary>
        [Display(Name = "Fecha de Rechazo")]
        public DateTime? FechaRechazo { get; set; }

        /// <summary>
        /// Cantidad de veces que se ha rechazado (para limitar intentos)
        /// </summary>
        [Display(Name = "Intentos de Pago")]
        public int IntentosRechazados { get; set; } = 0;

        // ==================== REEMBOLSO ====================

        /// <summary>
        /// Archivo del comprobante de reembolso (cuando la plataforma devuelve dinero)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Comprobante de Reembolso")]
        public string? ArchivoReembolso { get; set; }

        /// <summary>
        /// Número de transacción del reembolso
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Transacción Reembolso")]
        public string? NumeroTransaccionReembolso { get; set; }

        /// <summary>
        /// Fecha en que se realizó el reembolso
        /// </summary>
        [Display(Name = "Fecha de Reembolso")]
        public DateTime? FechaReembolso { get; set; }

        /// <summary>
        /// Motivo del reembolso
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Motivo de Reembolso")]
        public string? MotivoReembolso { get; set; }

        // ==================== OBSERVACIONES GENERALES ====================

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        [Display(Name = "Notificación Enviada")]
        public bool NotificacionEnviada { get; set; } = false;

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================

        public virtual Venta Venta { get; set; } = null!;
        public virtual Comprador Comprador { get; set; } = null!;

        [ForeignKey("VerificadoPorId")]
        public virtual Persona? VerificadoPor { get; set; }

        // ==================== PROPIEDADES CALCULADAS ====================

        /// <summary>
        /// Indica si el pago puede ser verificado (tiene comprobante y está en revisión)
        /// </summary>
        [NotMapped]
        public bool PuedeSerVerificado => 
            Estado == EstadoPago.EnRevision && 
            !string.IsNullOrEmpty(ArchivoComprobante);

        /// <summary>
        /// Indica si el comprador puede subir un nuevo comprobante
        /// </summary>
        [NotMapped]
        public bool PuedeSubirComprobante => 
            Estado == EstadoPago.Pendiente || 
            (Estado == EstadoPago.Rechazado && IntentosRechazados < 3);

        /// <summary>
        /// Indica si el pago requiere reembolso
        /// </summary>
        [NotMapped]
        public bool RequiereReembolso => Estado == EstadoPago.ReembolsoPendiente;
    }
}