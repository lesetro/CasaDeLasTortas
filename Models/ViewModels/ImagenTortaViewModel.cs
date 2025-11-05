using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class ImagenTortaViewModel
    {
        public int Id { get; set; }
        public int TortaId { get; set; }
        
        [Display(Name = "URL de la Imagen")]
        public string UrlImagen { get; set; }
        
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; }
        
        [Display(Name = "Tamaño")]
        [DisplayFormat(DataFormatString = "{0:N0} bytes")]
        public long? TamanioArchivo { get; set; }
        
        [Display(Name = "Tipo de Archivo")]
        public string? TipoArchivo { get; set; }
        
        [Display(Name = "Es Principal")]
        public bool EsPrincipal { get; set; }
        
        [Display(Name = "Orden")]
        public int Orden { get; set; }
        
        [Display(Name = "Fecha de Subida")]
        [DataType(DataType.DateTime)]
        public DateTime FechaSubida { get; set; }
        
        // Información de la torta
        public string? NombreTorta { get; set; }
    }

    public class ImagenTortaCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una torta")]
        [Display(Name = "Torta")]
        public int TortaId { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar al menos una imagen")]
        [Display(Name = "Imágenes")]
        public List<IFormFile> Imagenes { get; set; }
        
        [Display(Name = "Establecer como Principal")]
        public int? ImagenPrincipalIndex { get; set; }
    }

    public class ImagenTortaEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Es Imagen Principal")]
        public bool EsPrincipal { get; set; }
        
        [Display(Name = "Orden de Visualización")]
        [Range(0, 100)]
        public int Orden { get; set; }
    }

    public class ImagenTortaListViewModel
    {
        public int TortaId { get; set; }
        public string NombreTorta { get; set; }
        public List<ImagenTortaViewModel> Imagenes { get; set; }
        public bool PuedeEditar { get; set; }
    }
}