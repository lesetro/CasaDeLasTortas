using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class TortaViewModel
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre de la Torta")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Precio")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; }

        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [StringLength(20)]
        [Display(Name = "Tamaño")]
        public string Tamanio { get; set; } = string.Empty;

        [Display(Name = "Tiempo de Preparación (días)")]
        [Range(0, 30)]
        public int? TiempoPreparacion { get; set; }

        [Display(Name = "Ingredientes")]
        [DataType(DataType.MultilineText)]
        public string? Ingredientes { get; set; }

        [Display(Name = "Personalizable")]
        public bool Personalizable { get; set; }

        [Display(Name = "Veces Vendida")]
        public int VecesVendida { get; set; }

        [Display(Name = "Calificación")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Calificacion { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Última Actualización")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaActualizacion { get; set; }

        // Información del vendedor
        public string NombreVendedor { get; set; } = string.Empty;
        public string? EspecialidadVendedor { get; set; }

        // Imágenes
        public List<ImagenTortaViewModel> Imagenes { get; set; } = new();
        public string? ImagenPrincipal { get; set; }
    }

    public class TortaCreateViewModel
    {
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre de la Torta")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Precio")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock Inicial")]
        public int Stock { get; set; }

        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Tamaño")]
        public string Tamanio { get; set; } = string.Empty;

        [Display(Name = "Tiempo de Preparación (días)")]
        [Range(0, 30)]
        public int? TiempoPreparacion { get; set; }

        [Display(Name = "Ingredientes")]
        [DataType(DataType.MultilineText)]
        public string? Ingredientes { get; set; }

        [Display(Name = "¿Es Personalizable?")]
        public bool Personalizable { get; set; }

        [Display(Name = "Imágenes de la Torta")]
        public List<IFormFile>? Imagenes { get; set; }

        [Display(Name = "Índice de Imagen Principal")]
        public int? ImagenPrincipalIndex { get; set; }
    }

    public class TortaEditViewModel
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre de la Torta")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Precio")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; }

        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [StringLength(20)]
        [Display(Name = "Tamaño")]
        public string Tamanio { get; set; } = string.Empty;

        [Display(Name = "Tiempo de Preparación (días)")]
        [Range(0, 30)]
        public int? TiempoPreparacion { get; set; }

        [Display(Name = "Ingredientes")]
        [DataType(DataType.MultilineText)]
        public string? Ingredientes { get; set; }

        [Display(Name = "¿Es Personalizable?")]
        public bool Personalizable { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }

        // Imágenes existentes
        public List<ImagenTortaViewModel> ImagenesActuales { get; set; } = new();

        // Nuevas imágenes a agregar
        [Display(Name = "Agregar Nuevas Imágenes")]
        public List<IFormFile>? NuevasImagenes { get; set; }

        // IDs de imágenes a eliminar
        public List<int>? ImagenesAEliminar { get; set; }

        // ID de la nueva imagen principal
        public int? NuevaImagenPrincipalId { get; set; }
    }

    public class TortaListViewModel
    {
        public List<TortaViewModel> Tortas { get; set; } = new();
        public PaginacionViewModel Paginacion { get; set; } = new();
        public string? FiltroCategoria { get; set; }
        public string? FiltroVendedor { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public bool? SoloDisponibles { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
        
        // Para los filtros en la vista
        public List<string> CategoriasDisponibles { get; set; } = new();
        public List<VendedorViewModel> VendedoresDisponibles { get; set; } = new();
    }

    public class TortaDetalleViewModel
    {
        public TortaViewModel Torta { get; set; } = new();
        public VendedorViewModel Vendedor { get; set; } = new();
        public bool PuedeEditar { get; set; }
        public bool PuedeComprar { get; set; }
        public bool YaCompro { get; set; }
        
        // Para mostrar tortas relacionadas
        public List<TortaViewModel> TortasRelacionadas { get; set; } = new();
    }
}