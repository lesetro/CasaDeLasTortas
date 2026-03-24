using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public class Comprador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Persona")]
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [StringLength(50)]
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }

        [StringLength(50)]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [StringLength(10)]
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [Display(Name = "Total de Compras")]
        public int TotalCompras { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(500)]
        [Display(Name = "Preferencias")]
        [DataType(DataType.MultilineText)]
        public string? Preferencias { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual Persona Persona { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}