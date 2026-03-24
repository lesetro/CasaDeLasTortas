using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Models.ViewModels
{
    // ViewModel para actualizar perfil
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Teléfono inválido")]
        public string Telefono { get; set; }

        public IFormFile AvatarFile { get; set; }
    }

    
}