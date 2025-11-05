using System;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.DTOs
{
    public class ImagenTortaDTO
    {
        public int Id { get; set; }
        public int TortaId { get; set; }
        public string UrlImagen { get; set; }
        public string NombreArchivo { get; set; }
        public long? TamanioArchivo { get; set; }
        public string? TipoArchivo { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    public class ImagenTortaCreateDTO
    {
        public int TortaId { get; set; }
        public IFormFile ArchivoImagen { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
    }

    public class ImagenTortaUpdateDTO
    {
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
    }

    public class ImagenTortaUploadDTO
    {
        public int TortaId { get; set; }
        public List<IFormFile> Imagenes { get; set; }
        public int? ImagenPrincipalIndex { get; set; } // Índice de cuál imagen será la principal
    }

    public class ImagenTortaResponseDTO
    {
        public int Id { get; set; }
        public string UrlImagen { get; set; }
        public string NombreArchivo { get; set; }
        public bool EsPrincipal { get; set; }
        public bool UploadExitoso { get; set; }
        public string? MensajeError { get; set; }
    }
     public class ImagenTortaReordenarDTO
    {
        public int Id { get; set; }
        public int Orden { get; set; }
    }
}