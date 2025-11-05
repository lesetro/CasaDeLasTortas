using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public class ImagenTorta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Torta")]
        public int TortaId { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es requerida")]
        [StringLength(500)]
        [Display(Name = "URL de la Imagen")]
        public string UrlImagen { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; }

        [Display(Name = "Tamaño del Archivo (bytes)")]
        public long? TamanioArchivo { get; set; }

        [StringLength(50)]
        [Display(Name = "Tipo de Archivo")]
        public string? TipoArchivo { get; set; }

        [Display(Name = "Es Imagen Principal")]
        public bool EsPrincipal { get; set; } = false;

        [Display(Name = "Orden")]
        public int Orden { get; set; } = 0;

        [Display(Name = "Fecha de Subida")]
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Propiedad de navegación
        public virtual Torta Torta { get; set; }
    }
}