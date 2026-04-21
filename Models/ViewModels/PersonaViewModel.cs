using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CasaDeLasTortas.Models.Entities;
using System;
using System.Collections.Generic;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class PersonaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no es válido")]
        [StringLength(15)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(20)]
        [Display(Name = "DNI")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El DNI debe tener entre 8 y 10 dígitos")]
        public string? Dni { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? Avatar { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.Date)]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Último Acceso")]
        [DataType(DataType.DateTime)]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Cuenta Activa")]
        public bool Activo { get; set; }

        // Propiedades calculadas para la vista
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [Display(Name = "Edad")]
        public int? Edad => FechaNacimiento.HasValue ? 
            DateTime.Now.Year - FechaNacimiento.Value.Year - 
            (DateTime.Now.DayOfYear < FechaNacimiento.Value.DayOfYear ? 1 : 0) : null;

        [Display(Name = "Es Mayor de Edad")]
        public bool EsMayorDeEdad => FechaNacimiento.HasValue && 
            DateTime.Now.Year - FechaNacimiento.Value.Year >= 18;

        // Para mostrar información adicional
        public VendedorViewModel? Vendedor { get; set; }
        public CompradorViewModel? Comprador { get; set; }
    }

    public class PersonaCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Comprador";

        [Phone]
        [StringLength(15)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(20)]
        [Display(Name = "DNI")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El DNI debe tener entre 8 y 10 dígitos")]
        public string? Dni { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Foto de Perfil")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "Acepto los términos y condiciones")]
        [Required(ErrorMessage = "Debe aceptar los términos y condiciones")]
        public bool AceptaTerminos { get; set; }
    }

    //  PersonaEditViewModel
    public class PersonaEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(15)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(20)]
        [Display(Name = "DNI")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El DNI debe tener entre 8 y 10 dígitos")]
        public string? Dni { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Nueva Foto de Perfil")]
        public IFormFile? NuevoAvatar { get; set; }

        [Display(Name = "Foto Actual")]
        public string? AvatarActual { get; set; }

        [Display(Name = "Cuenta Activa")]
        public bool Activo { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class PersonaListViewModel
    {
        public List<PersonaViewModel> Personas { get; set; } = new List<PersonaViewModel>();
        public PaginacionViewModel Paginacion { get; set; } = new PaginacionViewModel();
        public string? FiltroRol { get; set; }
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
    }

    public class PersonaPerfilViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Dni { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Avatar { get; set; }
        public string Rol { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        
        // Propiedades calculadas
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public int? Edad => FechaNacimiento.HasValue ? 
            DateTime.Now.Year - FechaNacimiento.Value.Year - 
            (DateTime.Now.DayOfYear < FechaNacimiento.Value.DayOfYear ? 1 : 0) : null;
        public bool EsMayorDeEdad => Edad.HasValue && Edad >= 18;
    }
}