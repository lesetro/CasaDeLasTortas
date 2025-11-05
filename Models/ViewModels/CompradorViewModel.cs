using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class CompradorViewModel
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        [Phone]
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
        public int TotalCompras { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(500)]
        [Display(Name = "Preferencias")]
        [DataType(DataType.MultilineText)]
        public string? Preferencias { get; set; }

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        // Información de la persona
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }

        // Estadísticas
        public decimal TotalGastado { get; set; }
        public int PedidosActivos { get; set; }
        public DateTime? UltimaCompra { get; set; }
    }

    public class CompradorCreateViewModel
    {
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        [Phone]
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

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(500)]
        [Display(Name = "Preferencias")]
        [DataType(DataType.MultilineText)]
        public string? Preferencias { get; set; }
    }

    public class CompradorEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20)]
        [Phone]
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

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(500)]
        [Display(Name = "Preferencias")]
        [DataType(DataType.MultilineText)]
        public string? Preferencias { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }

    public class CompradorListViewModel
    {
        public List<CompradorViewModel> Compradores { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
        public string? FiltroCiudad { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
    }

    public class CompradorPerfilViewModel
    {
        public CompradorViewModel Comprador { get; set; }
        
        // Historial de compras
        public List<PagoViewModel> HistorialCompras { get; set; }
        
        // Pedidos activos
        public List<PagoViewModel> PedidosActivos { get; set; }
        
        // Estadísticas
        public decimal TotalGastadoMes { get; set; }
        public int ComprasMes { get; set; }
        public Dictionary<string, int> ComprasPorCategoria { get; set; }
        public List<VendedorViewModel> VendedoresFavoritos { get; set; }
    }
}