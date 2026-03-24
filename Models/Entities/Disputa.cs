using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Tipos de disputa
    /// </summary>
    public enum TipoDisputa
    {
        [Display(Name = "Producto no recibido")]
        ProductoNoRecibido,

        [Display(Name = "Producto dañado")]
        ProductoDaniado,

        [Display(Name = "Producto diferente al pedido")]
        ProductoDiferente,

        [Display(Name = "Pago no reconocido")]
        PagoNoReconocido,

        [Display(Name = "Solicitud de reembolso")]
        SolicitudReembolso,

        [Display(Name = "Problema con el vendedor")]
        ProblemaVendedor,

        [Display(Name = "Otro")]
        Otro
    }

    /// <summary>
    /// Estados de una disputa
    /// </summary>
    public enum EstadoDisputa
    {
        [Display(Name = "Abierta")]
        Abierta,                // Recién creada, pendiente de revisión

        [Display(Name = "En Investigación")]
        EnInvestigacion,        // Admin está investigando

        [Display(Name = "Pendiente de Respuesta del Vendedor")]
        EsperandoVendedor,      // Se requiere respuesta del vendedor

        [Display(Name = "Pendiente de Respuesta del Comprador")]
        EsperandoComprador,     // Se requiere respuesta del comprador

        [Display(Name = "Resuelta a favor del Comprador")]
        ResueltaFavorComprador,

        [Display(Name = "Resuelta a favor del Vendedor")]
        ResueltaFavorVendedor,

        [Display(Name = "Resuelta con acuerdo")]
        ResueltaAcuerdo,        // Ambas partes llegaron a un acuerdo

        [Display(Name = "Cerrada sin resolución")]
        Cerrada,

        [Display(Name = "Cancelada")]
        Cancelada
    }

    /// <summary>
    /// Resoluciones posibles de una disputa
    /// </summary>
    public enum ResolucionDisputa
    {
        [Display(Name = "Sin resolución")]
        SinResolucion,

        [Display(Name = "Reembolso total")]
        ReembolsoTotal,

        [Display(Name = "Reembolso parcial")]
        ReembolsoParcial,

        [Display(Name = "Reenvío de producto")]
        ReenvioProducto,

        [Display(Name = "Sin acción (a favor del vendedor)")]
        SinAccion,

        [Display(Name = "Crédito en plataforma")]
        Credito,

        [Display(Name = "Otro acuerdo")]
        OtroAcuerdo
    }

    /// <summary>
    /// Representa una disputa o reclamo entre comprador y vendedor
    /// El administrador actúa como mediador
    /// </summary>
    public class Disputa
    {
        [Key]
        public int Id { get; set; }

        // ==================== RELACIONES ====================

        [Required]
        [ForeignKey("Venta")]
        public int VentaId { get; set; }

        /// <summary>
        /// Quien inició la disputa (generalmente el comprador)
        /// </summary>
        [Required]
        [ForeignKey("Iniciador")]
        public int IniciadorId { get; set; }

        /// <summary>
        /// Administrador asignado a la disputa
        /// </summary>
        [ForeignKey("AdminAsignado")]
        public int? AdminAsignadoId { get; set; }

        // ==================== IDENTIFICACIÓN ====================

        /// <summary>
        /// Número único de la disputa (ej: DISP-20250320-001)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Número de Disputa")]
        public string NumeroDisputa { get; set; } = string.Empty;

        // ==================== CLASIFICACIÓN ====================

        [Required]
        [Display(Name = "Tipo de Disputa")]
        public TipoDisputa Tipo { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public EstadoDisputa Estado { get; set; } = EstadoDisputa.Abierta;

        [Display(Name = "Resolución")]
        public ResolucionDisputa Resolucion { get; set; } = ResolucionDisputa.SinResolucion;

        // ==================== DESCRIPCIÓN ====================

        /// <summary>
        /// Título o resumen breve del problema
        /// </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del problema
        /// </summary>
        [Required]
        [StringLength(2000)]
        [Display(Name = "Descripción del Problema")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; } = string.Empty;

        // ==================== EVIDENCIA ====================

        /// <summary>
        /// URLs de imágenes de evidencia (separadas por |)
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Evidencia (imágenes)")]
        public string? Evidencia { get; set; }

        // ==================== MONTOS ====================

        /// <summary>
        /// Monto involucrado en la disputa
        /// </summary>
        [Display(Name = "Monto Involucrado")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal? MontoInvolucrado { get; set; }

        /// <summary>
        /// Monto acordado en la resolución (si aplica)
        /// </summary>
        [Display(Name = "Monto Resolución")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal? MontoResolucion { get; set; }

        // ==================== FECHAS ====================

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        [Display(Name = "Fecha de Resolución")]
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// Fecha límite para responder (si está esperando respuesta)
        /// </summary>
        [Display(Name = "Fecha Límite")]
        public DateTime? FechaLimite { get; set; }

        // ==================== RESOLUCIÓN ====================

        /// <summary>
        /// Detalle de cómo se resolvió la disputa
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Detalle de Resolución")]
        [DataType(DataType.MultilineText)]
        public string? DetalleResolucion { get; set; }

        // ==================== PRIORIDAD ====================

        /// <summary>
        /// Prioridad de 1 (baja) a 5 (urgente)
        /// </summary>
        [Range(1, 5)]
        [Display(Name = "Prioridad")]
        public int Prioridad { get; set; } = 3;

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================

        public virtual Venta Venta { get; set; } = null!;
        public virtual Persona Iniciador { get; set; } = null!;
        public virtual Persona? AdminAsignado { get; set; }
        public virtual ICollection<MensajeDisputa> Mensajes { get; set; } = new List<MensajeDisputa>();

        // ==================== PROPIEDADES CALCULADAS ====================

        [NotMapped]
        public bool EstaAbierta => Estado == EstadoDisputa.Abierta || 
                                    Estado == EstadoDisputa.EnInvestigacion ||
                                    Estado == EstadoDisputa.EsperandoVendedor ||
                                    Estado == EstadoDisputa.EsperandoComprador;

        [NotMapped]
        public bool EstaResuelta => Estado == EstadoDisputa.ResueltaFavorComprador ||
                                     Estado == EstadoDisputa.ResueltaFavorVendedor ||
                                     Estado == EstadoDisputa.ResueltaAcuerdo;

        [NotMapped]
        public int DiasAbierta => (DateTime.Now - FechaCreacion).Days;

        [NotMapped]
        public string PrioridadTexto => Prioridad switch
        {
            1 => "Baja",
            2 => "Normal",
            3 => "Media",
            4 => "Alta",
            5 => "Urgente",
            _ => "Normal"
        };

        /// <summary>
        /// Lista de URLs de evidencia
        /// </summary>
        [NotMapped]
        public IEnumerable<string> ListaEvidencia => 
            string.IsNullOrEmpty(Evidencia) 
                ? Enumerable.Empty<string>() 
                : Evidencia.Split('|', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Mensaje dentro de una disputa (comunicación entre las partes)
    /// </summary>
    public class MensajeDisputa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Disputa")]
        public int DisputaId { get; set; }

        [Required]
        [ForeignKey("Autor")]
        public int AutorId { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Mensaje")]
        [DataType(DataType.MultilineText)]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Archivos adjuntos (URLs separadas por |)
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Adjuntos")]
        public string? Adjuntos { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>
        /// Indica si es un mensaje interno (solo visible para admins)
        /// </summary>
        [Display(Name = "Mensaje Interno")]
        public bool EsInterno { get; set; } = false;

        // Navegación
        public virtual Disputa Disputa { get; set; } = null!;
        public virtual Persona Autor { get; set; } = null!;

        [NotMapped]
        public IEnumerable<string> ListaAdjuntos => 
            string.IsNullOrEmpty(Adjuntos) 
                ? Enumerable.Empty<string>() 
                : Adjuntos.Split('|', StringSplitOptions.RemoveEmptyEntries);
    }
}