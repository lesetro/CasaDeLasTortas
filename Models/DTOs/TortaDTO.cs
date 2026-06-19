using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Models.DTOs
{
    public class TortaDTO
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string Tamanio { get; set; }
        public int? TiempoPreparacion { get; set; }
        public string? Ingredientes { get; set; }
        public bool Personalizable { get; set; }
        public int VecesVendida { get; set; }
        public decimal Calificacion { get; set; }
        public bool Disponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        // Información del vendedor
        public string NombreVendedor { get; set; }
        public string EspecialidadVendedor { get; set; }
        
        // Imágenes
        public List<ImagenTortaDTO> Imagenes { get; set; }
        public string? ImagenPrincipal { get; set; }
    }

    public class TortaCreateDTO
    {
        [Required(ErrorMessage = "El vendedor es obligatorio")]
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "La descripción debe tener al menos 5 caracteres")]
        public string Descripcion { get; set; }

        [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public string? Categoria { get; set; }

        [Required(ErrorMessage = "El tamaño es obligatorio")]
        public string Tamanio { get; set; }

        public int? TiempoPreparacion { get; set; }
        public string? Ingredientes { get; set; }
        public bool Personalizable { get; set; }
    }

    public class TortaUpdateDTO
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string Tamanio { get; set; }
        public int? TiempoPreparacion { get; set; }
        public string? Ingredientes { get; set; }
        public bool Personalizable { get; set; }
        public bool Disponible { get; set; }
    }

    public class TortaListDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public decimal Calificacion { get; set; }
        public bool Disponible { get; set; }
        public string NombreVendedor { get; set; }
        public string? ImagenPrincipal { get; set; }
    }

    public class TortaDetalleDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string Tamanio { get; set; }
        public int? TiempoPreparacion { get; set; }
        public string? Ingredientes { get; set; }
        public bool Personalizable { get; set; }
        public int VecesVendida { get; set; }
        public decimal Calificacion { get; set; }
        public bool Disponible { get; set; }
        
        // Vendedor
        public VendedorDTO Vendedor { get; set; }
        
        // Imágenes
        public List<ImagenTortaDTO> Imagenes { get; set; }
        
        // Estadísticas
        public int TotalComentarios { get; set; }
        public bool PuedeCalificar { get; set; }
    }

    public class TortaCatalogoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string DescripcionCorta { get; set; }
        public decimal Precio { get; set; }
        public string? Categoria { get; set; }
        public decimal Calificacion { get; set; }
        public string NombreVendedor { get; set; }
        public string? ImagenPrincipal { get; set; }
        public bool Disponible { get; set; }
        public bool EsNuevo { get; set; } // Si fue creado en los últimos 7 días
        public bool EsPopular { get; set; } // Si tiene más de X ventas
    }
}