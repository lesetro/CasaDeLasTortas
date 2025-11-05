using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class VendedorViewModel
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; }

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        [Display(Name = "Calificación")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Calificacion { get; set; }

        [Display(Name = "Total de Ventas")]
        public int TotalVentas { get; set; }

        [StringLength(100)]
        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Verificado")]
        public bool Verificado { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        // Información de la persona
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public string? Telefono { get; set; }

        // Estadísticas
        public int TotalTortas { get; set; }
        public int TortasDisponibles { get; set; }
        public decimal IngresosTotales { get; set; }
        public int PedidosPendientes { get; set; }
    }

    public class VendedorCreateViewModel
    {
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; }

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción del Negocio")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }
    }

    public class VendedorEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; }

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción del Negocio")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }

    public class VendedorListViewModel
    {
        public List<VendedorViewModel> Vendedores { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
        public string? FiltroEspecialidad { get; set; }
        public bool? FiltroVerificado { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
    }

    public class VendedorDashboardViewModel
    {
        public VendedorViewModel Vendedor { get; set; }
        
        // Estadísticas del mes
        public decimal IngresosMes { get; set; }
        public int VentasMes { get; set; }
        public int NuevosPedidos { get; set; }
        public decimal PromedioVentaDiaria { get; set; }
        public decimal IngresosTotales { get; set; }
        public int CantidadPedidosPendientes { get; set; }
        public int TortasActivas { get; set; }
        
        
        // Gráficos
        public Dictionary<string, decimal>? VentasUltimos7Dias { get; set; }
        public Dictionary<string, int>? TortasMasVendidas { get; set; }
        
        // Listas recientes
        public List<TortaViewModel>? TortasRecientes { get; set; }
        public List<PagoViewModel>? PagosRecientes { get; set; }
        public List<PagoViewModel>? PedidosPendientesLista { get; set; }
        
    }

    public class VendedorPerfilPublicoViewModel
    {
        public int Id { get; set; }
        public string? NombreComercial { get; set; }
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public string? Horario { get; set; }
        public bool Verificado { get; set; }
        public string? Avatar { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // Catálogo público
        public List<TortaViewModel>? TortasDisponibles { get; set; }
        public int TotalTortas { get; set; }
    }
}