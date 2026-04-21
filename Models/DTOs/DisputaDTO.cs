using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar información de una disputa
    /// </summary>
    public class DisputaDTO
    {
        public int Id { get; set; }
        public string NumeroDisputa { get; set; } = string.Empty;
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        
        // Tipo y estado
        public TipoDisputa Tipo { get; set; }
        public string TipoTexto { get; set; } = string.Empty;
        public EstadoDisputa Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        public ResolucionDisputa Resolucion { get; set; }
        public string? ResolucionTexto { get; set; }
        
        // Contenido
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public List<string> Evidencia { get; set; } = new();
        
        // Montos
        public decimal? MontoInvolucrado { get; set; }
        public decimal? MontoResolucion { get; set; }
        
        // Personas
        public int IniciadorId { get; set; }
        public string NombreIniciador { get; set; } = string.Empty;
        public string EmailIniciador { get; set; } = string.Empty;
        public int? AdminAsignadoId { get; set; }
        public string? NombreAdminAsignado { get; set; }
        
        // Prioridad
        public int Prioridad { get; set; }
        public string PrioridadTexto { get; set; } = string.Empty;
        
        // Fechas
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaLimite { get; set; }
        public int DiasAbierta { get; set; }
        
        // Resolución
        public string? DetalleResolucion { get; set; }
        
        // Flags
        public bool EstaAbierta { get; set; }
        public bool EstaResuelta { get; set; }
        
        // Mensajes
        public List<MensajeDisputaDTO> Mensajes { get; set; } = new();
        public int CantidadMensajes { get; set; }
    }

    /// <summary>
    /// DTO para un mensaje dentro de una disputa
    /// </summary>
    public class MensajeDisputaDTO
    {
        public int Id { get; set; }
        public int DisputaId { get; set; }
        public int AutorId { get; set; }
        public string NombreAutor { get; set; } = string.Empty;
        public string? AvatarAutor { get; set; }
        public string RolAutor { get; set; } = string.Empty; // "Comprador", "Vendedor", "Admin"
        public string Contenido { get; set; } = string.Empty;
        public List<string> Adjuntos { get; set; } = new();
        public DateTime Fecha { get; set; }
        public bool EsInterno { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva disputa
    /// </summary>
    public class CrearDisputaDTO
    {
        [Required(ErrorMessage = "El ID de la venta es requerido")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El tipo de disputa es requerido")]
        public TipoDisputa Tipo { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "El título debe tener entre 10 y 200 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 2000 caracteres")]
        [Display(Name = "Descripción del problema")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Monto involucrado")]
        public decimal? MontoInvolucrado { get; set; }

        /// <summary>
        /// URLs de imágenes de evidencia
        /// </summary>
        public List<string>? Evidencia { get; set; }
    }

    /// <summary>
    /// DTO para agregar un mensaje a la disputa
    /// </summary>
    public class AgregarMensajeDisputaDTO
    {
        [Required]
        public int DisputaId { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(2000, MinimumLength = 1)]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// URLs de archivos adjuntos
        /// </summary>
        public List<string>? Adjuntos { get; set; }

        /// <summary>
        /// Si es true, solo admins pueden ver el mensaje
        /// </summary>
        public bool EsInterno { get; set; } = false;
    }

    /// <summary>
    /// DTO para resolver una disputa
    /// </summary>
    public class ResolverDisputaDTO
    {
        [Required]
        public int DisputaId { get; set; }

        [Required(ErrorMessage = "La resolución es requerida")]
        public ResolucionDisputa Resolucion { get; set; }

        /// <summary>
        /// A favor de quién se resuelve
        /// </summary>
        [Required]
        public string AFavorDe { get; set; } = string.Empty; // "Comprador", "Vendedor", "Acuerdo"

        [Required(ErrorMessage = "El detalle de resolución es requerido")]
        [StringLength(2000, MinimumLength = 10)]
        [Display(Name = "Detalle de la resolución")]
        public string DetalleResolucion { get; set; } = string.Empty;

        /// <summary>
        /// Monto a reembolsar (si aplica)
        /// </summary>
        [Display(Name = "Monto de resolución")]
        public decimal? MontoResolucion { get; set; }
    }

    /// <summary>
    /// DTO para asignar admin a una disputa
    /// </summary>
    public class AsignarAdminDisputaDTO
    {
        [Required]
        public int DisputaId { get; set; }

        [Required]
        public int AdminId { get; set; }
    }

    /// <summary>
    /// DTO para listar disputas (Admin)
    /// </summary>
    public class DisputaResumenDTO
    {
        public int Id { get; set; }
        public string NumeroDisputa { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public string TipoTexto { get; set; } = string.Empty;
        public string EstadoTexto { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string NombreIniciador { get; set; } = string.Empty;
        public string? NombreAdminAsignado { get; set; }
        public int Prioridad { get; set; }
        public string PrioridadTexto { get; set; } = string.Empty;
        public decimal? MontoInvolucrado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int DiasAbierta { get; set; }
        public int CantidadMensajes { get; set; }
        public bool EstaAbierta { get; set; }
    }

    /// <summary>
    /// DTO para crear disputa enviando el tipo como string (para compatibilidad con clientes JSON)
    /// </summary>
    public class CrearDisputaRequestDTO
    {
        [Required]
        public int VentaId { get; set; }

        /// <summary>
        /// Nombre del enum TipoDisputa como string (ej: "ProductoNoRecibido") o número.
        /// </summary>
        [Required]
        public string TipoStr { get; set; } = string.Empty;

        public string? Titulo { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        public string? MensajeInicial { get; set; }

        /// <summary>Prioridad como entero: 1=Baja … 5=Urgente</summary>
        public int? Prioridad { get; set; }

        public decimal? MontoInvolucrado { get; set; }
    }

    /// <summary>
    /// DTO para agregar un mensaje simple a una disputa (sin adjuntos)
    /// </summary>
    public class AgregarMensajeDTO
    {
        [Required]
        [StringLength(2000)]
        public string Contenido { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambiar el estado de una disputa
    /// </summary>
    public class CambiarEstadoDisputaDTO
    {
        [Required]
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para estadísticas de disputas
    /// </summary>
    public class EstadisticasDisputasDTO
    {
        // Contadores
        public int TotalDisputas { get; set; }
        public int Abiertas { get; set; }
        public int EnInvestigacion { get; set; }
        public int EsperandoRespuesta { get; set; }
        public int ResueltasHoy { get; set; }
        public int ResueltasMes { get; set; }
        
        // Por resolución
        public int ResueltasFavorComprador { get; set; }
        public int ResueltasFavorVendedor { get; set; }
        public int ResueltasAcuerdo { get; set; }
        
        // Montos
        public decimal MontoTotalInvolucrado { get; set; }
        public decimal MontoTotalReembolsado { get; set; }
        
        // Tiempos promedio
        public double DiasPromedioResolucion { get; set; }
        
        // Por tipo (los más comunes)
        public Dictionary<string, int> PorTipo { get; set; } = new();
    }
}