using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Configuración global de la plataforma
    /// Incluye datos bancarios para recibir pagos, porcentajes de comisión, etc.
    /// Solo debe existir un registro (singleton en BD)
    /// </summary>
    public class ConfiguracionPlataforma
    {
        [Key]
        public int Id { get; set; }

        // ==================== DATOS DE LA PLATAFORMA ====================

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre de la Plataforma")]
        public string NombrePlataforma { get; set; } = "Casa de las Tortas";

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        [Display(Name = "Logo URL")]
        public string? LogoUrl { get; set; }

        // ==================== DATOS BANCARIOS DE LA PLATAFORMA ====================

        /// <summary>
        /// Alias de CBU/MercadoPago de la plataforma para recibir pagos
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Alias (CBU/MercadoPago)")]
        public string AliasCBU { get; set; } = string.Empty;

        /// <summary>
        /// CBU de la cuenta de la plataforma (22 dígitos)
        /// </summary>
        [Required]
        [StringLength(22)]
        [Display(Name = "CBU")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe tener exactamente 22 dígitos")]
        public string CBU { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del banco de la plataforma
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "Banco")]
        public string Banco { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del titular de la cuenta
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "Titular de la Cuenta")]
        public string TitularCuenta { get; set; } = string.Empty;

        /// <summary>
        /// CUIT de la plataforma
        /// </summary>
        [Required]
        [StringLength(13)]
        [Display(Name = "CUIT")]
        public string CUIT { get; set; } = string.Empty;

        /// <summary>
        /// Imagen del QR de MercadoPago de la plataforma
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Imagen QR")]
        public string? ImagenQR { get; set; }

        // ==================== COMISIONES ====================

        /// <summary>
        /// Porcentaje de comisión estándar (ej: 10.00 = 10%)
        /// </summary>
        [Required]
        [Range(0, 100)]
        [Display(Name = "Comisión Estándar (%)")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal ComisionPorcentaje { get; set; } = 10.00m;

        /// <summary>
        /// Comisión mínima en pesos (si el porcentaje da menos)
        /// </summary>
        [Display(Name = "Comisión Mínima ($)")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ComisionMinima { get; set; } = 0;

        /// <summary>
        /// Comisión máxima en pesos (tope)
        /// </summary>
        [Display(Name = "Comisión Máxima ($)")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ComisionMaxima { get; set; }

        // ==================== POLÍTICAS ====================

        /// <summary>
        /// Días después de la entrega para liberar fondos automáticamente
        /// </summary>
        [Range(0, 30)]
        [Display(Name = "Días para Liberar Fondos")]
        public int DiasParaLiberarFondos { get; set; } = 3;

        /// <summary>
        /// Días máximos para que el comprador suba comprobante
        /// </summary>
        [Range(1, 30)]
        [Display(Name = "Días Límite para Pagar")]
        public int DiasLimitePago { get; set; } = 3;

        /// <summary>
        /// Máximo de intentos de pago rechazado antes de cancelar
        /// </summary>
        [Range(1, 10)]
        [Display(Name = "Máx. Intentos de Pago")]
        public int MaxIntentosRechazados { get; set; } = 3;

        /// <summary>
        /// Días para resolver una disputa
        /// </summary>
        [Range(1, 60)]
        [Display(Name = "Días Límite Disputa")]
        public int DiasLimiteDisputa { get; set; } = 15;

        // ==================== NOTIFICACIONES ====================

        [StringLength(100)]
        [Display(Name = "Email de Notificaciones")]
        [EmailAddress]
        public string? EmailNotificaciones { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono de Contacto")]
        public string? TelefonoContacto { get; set; }

        // ==================== TEXTOS ====================

        /// <summary>
        /// Instrucciones que se muestran al comprador para pagar
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Instrucciones de Pago")]
        [DataType(DataType.MultilineText)]
        public string? InstruccionesPago { get; set; }

        /// <summary>
        /// Términos y condiciones (resumen)
        /// </summary>
        [StringLength(5000)]
        [Display(Name = "Términos y Condiciones")]
        [DataType(DataType.MultilineText)]
        public string? TerminosCondiciones { get; set; }

        // ==================== ESTADO ====================

        [Display(Name = "Plataforma Activa")]
        public bool PlataformaActiva { get; set; } = true;

        /// <summary>
        /// Mensaje a mostrar si la plataforma está en mantenimiento
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Mensaje de Mantenimiento")]
        public string? MensajeMantenimiento { get; set; }

        // ==================== AUDITORÍA ====================

        [Display(Name = "Última Actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [Display(Name = "Actualizado Por")]
        public int? ActualizadoPorId { get; set; }

        // ==================== MÉTODOS ====================

        /// <summary>
        /// Calcula la comisión para un monto dado
        /// </summary>
        public decimal CalcularComision(decimal monto)
        {
            var comision = Math.Round(monto * (ComisionPorcentaje / 100), 2);
            
            // Aplicar mínimo
            if (comision < ComisionMinima)
                comision = ComisionMinima;
            
            // Aplicar máximo
            if (ComisionMaxima.HasValue && comision > ComisionMaxima.Value)
                comision = ComisionMaxima.Value;
            
            return comision;
        }

        /// <summary>
        /// Crea una configuración por defecto para inicializar la BD
        /// </summary>
        public static ConfiguracionPlataforma CrearDefault()
        {
            return new ConfiguracionPlataforma
            {
                NombrePlataforma = "Casa de las Tortas",
                Descripcion = "Marketplace de tortas artesanales",
                AliasCBU = "casadelastortas.pagos",
                CBU = "0000000000000000000000", // Placeholder - Admin debe configurar
                Banco = "Banco Ejemplo",
                TitularCuenta = "Casa de las Tortas S.A.",
                CUIT = "30-12345678-9",
                ComisionPorcentaje = 10.00m,
                DiasParaLiberarFondos = 3,
                DiasLimitePago = 3,
                MaxIntentosRechazados = 3,
                DiasLimiteDisputa = 15,
                PlataformaActiva = true,
                InstruccionesPago = @"Para completar tu compra:
1. Realizá una transferencia al alias o CBU indicado
2. Subí el comprobante de pago
3. Esperá la confirmación (máximo 24hs hábiles)
4. ¡Listo! Tu pedido comenzará a prepararse",
                FechaActualizacion = DateTime.Now
            };
        }
    }
}