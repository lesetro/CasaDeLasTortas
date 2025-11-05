using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El {0} no puede exceder los {1} caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Tipo de Cuenta")]
        public string Rol { get; set; } // "Vendedor" o "Comprador"

        // ✅ CAMBIADO: Avatar → AvatarFile
        [Display(Name = "Foto de Perfil")]
        public IFormFile? AvatarFile { get; set; }

        // Campos adicionales para Vendedor
        [Display(Name = "Nombre Comercial")]
        public string? NombreComercial { get; set; }

        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [Display(Name = "Descripción del Negocio")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        // Campos adicionales para Comprador
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }

        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Acepto los términos y condiciones")]
        [Required(ErrorMessage = "Debe aceptar los términos y condiciones")]
        public bool AceptaTerminos { get; set; }
    }
}