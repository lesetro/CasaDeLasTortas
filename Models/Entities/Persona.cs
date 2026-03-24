using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaDeLasTortas.Models.Entities
{
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } 

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        [Display(Name = "DNI")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El DNI debe tener entre 8 y 10 dígitos")]
        public string? Dni { get; set; }

        [StringLength(15)]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(500)]
        [Display(Name = "Foto de Perfil")]
        public string? Avatar { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Comprador"; // "Vendedor", "Comprador", "Admin"

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Propiedades calculadas (no se mapean a la base de datos)
        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [NotMapped]
        [Display(Name = "Edad")]
        public int? Edad => FechaNacimiento.HasValue ? 
            DateTime.Now.Year - FechaNacimiento.Value.Year - 
            (DateTime.Now.DayOfYear < FechaNacimiento.Value.DayOfYear ? 1 : 0) : null;

        // Propiedades de navegación
        public virtual Vendedor? Vendedor { get; set; }
        public virtual Comprador? Comprador { get; set; }

        // Métodos de utilidad
        public void ActualizarUltimoAcceso()
        {
            UltimoAcceso = DateTime.Now;
        }

        public bool VerificarPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void CambiarPassword(string nuevaPassword)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
        }

        public bool EsMayorDeEdad()
        {
            return FechaNacimiento.HasValue && 
                   DateTime.Now.Year - FechaNacimiento.Value.Year >= 18;
        }
    }
}