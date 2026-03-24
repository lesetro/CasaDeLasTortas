using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public class Vendedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Persona")]
        public int PersonaId { get; set; }

        // ==================== DATOS COMERCIALES ====================
        
        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100, ErrorMessage = "El nombre comercial no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        [Range(0, 5)]
        [Display(Name = "Calificación")]
        [Column(TypeName = "decimal(3, 2)")]
        public decimal Calificacion { get; set; } = 0;

        [Display(Name = "Total de Ventas")]
        public int TotalVentas { get; set; } = 0;

        [StringLength(100)]
        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        // ==================== DATOS DE PAGO (NUEVOS) ====================
        
        /// <summary>
        /// Alias de CBU o MercadoPago para recibir pagos
        /// Ejemplo: "pasteleria.carlos.mp"
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Alias (CBU/MercadoPago)")]
        public string? AliasCBU { get; set; }

        /// <summary>
        /// Clave Bancaria Uniforme (22 dígitos) o CVU para billeteras virtuales
        /// </summary>
        [StringLength(22)]
        [Display(Name = "CBU/CVU")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe tener exactamente 22 dígitos")]
        public string? CBU { get; set; }

        /// <summary>
        /// Nombre del banco o billetera virtual
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Banco/Billetera")]
        public string? Banco { get; set; }

        /// <summary>
        /// Nombre del titular de la cuenta bancaria
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Titular de la Cuenta")]
        public string? TitularCuenta { get; set; }

        /// <summary>
        /// CUIT del vendedor para facturación (formato: XX-XXXXXXXX-X)
        /// </summary>
        [StringLength(13)]
        [Display(Name = "CUIT")]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT inválido (XX-XXXXXXXX-X)")]
        public string? CUIT { get; set; }

        /// <summary>
        /// URL de la imagen del código QR de MercadoPago
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Imagen QR de Pago")]
        public string? ImagenQR { get; set; }

        /// <summary>
        /// Indica si el vendedor tiene completos sus datos de pago
        /// </summary>
        [Display(Name = "Datos de Pago Completos")]
        public bool DatosPagoCompletos { get; set; } = false;

        // ==================== ESTADOS Y FECHAS ====================

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Verificado")]
        public bool Verificado { get; set; } = false;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha en que completó sus datos de pago
        /// </summary>
        [Display(Name = "Fecha Datos de Pago")]
        public DateTime? FechaDatosPago { get; set; }

        // ==================== ESTADÍSTICAS DE COBROS ====================

        /// <summary>
        /// Total acumulado que ha recibido de la plataforma
        /// </summary>
        [Display(Name = "Total Cobrado")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalCobrado { get; set; } = 0;

        /// <summary>
        /// Total en comisiones pagadas a la plataforma
        /// </summary>
        [Display(Name = "Total Comisiones")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalComisiones { get; set; } = 0;

        /// <summary>
        /// Monto pendiente de cobrar (liberaciones pendientes)
        /// </summary>
        [Display(Name = "Pendiente de Cobro")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal PendienteCobro { get; set; } = 0;

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================
        
        public virtual Persona Persona { get; set; } = null!;
        public virtual ICollection<Torta> Tortas { get; set; } = new List<Torta>();
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public virtual ICollection<Pago> PagosConfirmados { get; set; } = new List<Pago>();
        public virtual ICollection<LiberacionFondos> Liberaciones { get; set; } = new List<LiberacionFondos>();

        // ==================== MÉTODOS DE VALIDACIÓN ====================

        /// <summary>
        /// Verifica si el vendedor tiene todos los datos necesarios para recibir pagos
        /// </summary>
        public bool TieneDatosPagoValidos()
        {
            return !string.IsNullOrWhiteSpace(AliasCBU) &&
                   !string.IsNullOrWhiteSpace(CBU) &&
                   !string.IsNullOrWhiteSpace(TitularCuenta) &&
                   CBU.Length == 22;
        }

        /// <summary>
        /// Actualiza el estado de DatosPagoCompletos basado en los campos
        /// </summary>
        public void ActualizarEstadoDatosPago()
        {
            var completos = TieneDatosPagoValidos();
            if (completos && !DatosPagoCompletos)
            {
                DatosPagoCompletos = true;
                FechaDatosPago = DateTime.Now;
            }
            else if (!completos)
            {
                DatosPagoCompletos = false;
            }
        }
    }
}