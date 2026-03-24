using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public class Torta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vendedor")]
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre de la Torta")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999999.99")]
        [Display(Name = "Precio")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [StringLength(20)]
        [Display(Name = "Tamaño")]
        public string Tamanio { get; set; } = "Mediana";

        [Display(Name = "Tiempo de Preparación (días)")]
        [Range(0, 30, ErrorMessage = "El tiempo de preparación debe estar entre 0 y 30 días")]
        public int? TiempoPreparacion { get; set; }

        [Display(Name = "Ingredientes")]
        public string? Ingredientes { get; set; }

        [Display(Name = "Personalizable")]
        public bool Personalizable { get; set; } = false;

        [Display(Name = "Veces Vendida")]
        public int VecesVendida { get; set; } = 0;

        [Range(0, 5)]
        [Display(Name = "Calificación")]
        [Column(TypeName = "decimal(3, 2)")]
        public decimal Calificacion { get; set; } = 0;

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación
        public virtual Vendedor Vendedor { get; set; }
        public virtual ICollection<ImagenTorta> Imagenes { get; set; } = new List<ImagenTorta>();
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}