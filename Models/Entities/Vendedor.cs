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

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100, ErrorMessage = "El nombre comercial no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; }

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

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Verificado")]
        public bool Verificado { get; set; } = false;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual Persona Persona { get; set; }
        public virtual ICollection<Torta> Tortas { get; set; } = new List<Torta>();
        public virtual ICollection<Pago> PagosRecibidos { get; set; } = new List<Pago>();
    }
}