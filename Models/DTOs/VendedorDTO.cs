using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar información de un vendedor
    /// </summary>
    public class VendedorDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        
        // Datos comerciales
        public string NombreComercial { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public string? Horario { get; set; }
        
        // Datos de la persona
        public string NombrePersona { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Avatar { get; set; }
        
        // Estados
        public bool Verificado { get; set; }
        public bool Activo { get; set; }
        public bool DatosPagoCompletos { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // Estadísticas de cobros
        public decimal TotalCobrado { get; set; }
        public decimal TotalComisiones { get; set; }
        public decimal PendienteCobro { get; set; }
        
        // Datos de pago (visibles solo para el propio vendedor o admin)
        public DatosPagoVendedorDTO? DatosPago { get; set; }
    }

    /// <summary>
    /// DTO para listar vendedores (usado en filtros de tortas, listas, etc.)
    /// </summary>
    public class VendedorListDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public bool Verificado { get; set; }
        public bool Activo { get; set; }
        public string? Avatar { get; set; }
        
        // Para mostrar cantidad de productos
        public int TotalProductos { get; set; }
        public int ProductosDisponibles { get; set; }
    }

    /// <summary>
    /// DTO para los datos de pago del vendedor
    /// </summary>
    public class DatosPagoVendedorDTO
    {
        public string? AliasCBU { get; set; }
        public string? CBU { get; set; }
        public string? CBUOculto { get; set; } // Muestra solo últimos 4 dígitos
        public string? Banco { get; set; }
        public string? TitularCuenta { get; set; }
        public string? CUIT { get; set; }
        public string? ImagenQR { get; set; }
        public bool DatosCompletos { get; set; }
        public DateTime? FechaDatosPago { get; set; }
    }

    /// <summary>
    /// DTO para crear/actualizar datos de pago del vendedor
    /// </summary>
    public class ActualizarDatosPagoDTO
    {
        [Required(ErrorMessage = "El alias es requerido")]
        [StringLength(50, ErrorMessage = "El alias no puede exceder 50 caracteres")]
        [Display(Name = "Alias (CBU/MercadoPago)")]
        public string AliasCBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CBU es requerido")]
        [StringLength(22, MinimumLength = 22, ErrorMessage = "El CBU debe tener exactamente 22 dígitos")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe contener solo números")]
        [Display(Name = "CBU/CVU")]
        public string CBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El banco es requerido")]
        [StringLength(100)]
        [Display(Name = "Banco/Billetera")]
        public string Banco { get; set; } = string.Empty;

        [Required(ErrorMessage = "El titular es requerido")]
        [StringLength(100)]
        [Display(Name = "Titular de la Cuenta")]
        public string TitularCuenta { get; set; } = string.Empty;

        [StringLength(13)]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT inválido")]
        [Display(Name = "CUIT (opcional)")]
        public string? CUIT { get; set; }

        [StringLength(500)]
        [Display(Name = "Imagen QR (URL)")]
        public string? ImagenQR { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo vendedor
    /// </summary>
    public class CrearVendedorDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        public string NombreComercial { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? Horario { get; set; }
    }

    /// <summary>
    /// DTO para mostrar vendedor en el checkout (datos de pago para el comprador)
    /// </summary>
    public class VendedorCheckoutDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public decimal Calificacion { get; set; }
        public bool Verificado { get; set; }
        
        // Monto que corresponde a este vendedor en la compra
        public decimal MontoProductos { get; set; }
        public decimal Comision { get; set; }
        public decimal MontoNeto { get; set; }
    }

    /// <summary>
    /// DTO para resumen de vendedor en el panel admin
    /// </summary>
    public class VendedorResumenDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string NombrePersona { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalVentas { get; set; }
        public decimal Calificacion { get; set; }
        public bool Verificado { get; set; }
        public bool Activo { get; set; }
        public bool DatosPagoCompletos { get; set; }
        public decimal PendienteCobro { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}