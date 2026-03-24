using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.ViewModels
{
    // ══════════════════════════════════════════════════════════════════════
    // VENDEDOR VIEW MODEL
    // ✅ AGREGADA: Esta clase faltaba y causaba errores en otros ViewModels
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel básico para mostrar información de un vendedor
    /// Usado en listas, tarjetas, y referencias desde otros ViewModels
    /// </summary>
    public class VendedorViewModel
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }

        // Datos de persona
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Avatar")]
        public string? Avatar { get; set; }

        // Datos comerciales
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Horario")]
        public string? Horario { get; set; }

        // Métricas
        [Display(Name = "Calificación")]
        public decimal Calificacion { get; set; }

        [Display(Name = "Total Ventas")]
        public int TotalVentas { get; set; }

        // Estados
        [Display(Name = "Verificado")]
        public bool Verificado { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Datos de Pago Completos")]
        public bool DatosPagoCompletos { get; set; }

        // Fechas
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; }

        // Estadísticas adicionales
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }

        // URL de imagen principal (si tiene)
        public string? ImagenPrincipal { get; set; }

        /// <summary>
        /// Mapea desde la entidad Vendedor
        /// </summary>
        public static VendedorViewModel FromEntity(Vendedor vendedor)
        {
            return new VendedorViewModel
            {
                Id = vendedor.Id,
                PersonaId = vendedor.PersonaId,
                Nombre = vendedor.Persona?.Nombre ?? "N/A",
                Email = vendedor.Persona?.Email ?? "N/A",
                Telefono = vendedor.Persona?.Telefono,
                Avatar = vendedor.Persona?.Avatar,
                NombreComercial = vendedor.NombreComercial,
                Especialidad = vendedor.Especialidad,
                Descripcion = vendedor.Descripcion,
                Horario = vendedor.Horario,
                Calificacion = vendedor.Calificacion,
                TotalVentas = vendedor.TotalVentas,
                Verificado = vendedor.Verificado,
                Activo = vendedor.Activo,
                DatosPagoCompletos = vendedor.DatosPagoCompletos,
                FechaCreacion = vendedor.FechaCreacion,
                TotalProductos = vendedor.Tortas?.Count ?? 0,
                ProductosActivos = vendedor.Tortas?.Count(t => t.Disponible && t.Stock > 0) ?? 0
            };
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // VENDEDOR PERFIL VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para mostrar el perfil completo del vendedor
    /// </summary>
    public class VendedorPerfilViewModel
    {
        // ==================== DATOS BÁSICOS ====================
        
        public int Id { get; set; }
        public int PersonaId { get; set; }
        
        // Datos de persona
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Avatar { get; set; }
        
        // Datos comerciales
        public string NombreComercial { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public string? Horario { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public bool Verificado { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // ==================== DATOS DE PAGO ====================
        
        public DatosPagoVendedorViewModel DatosPago { get; set; } = new();
        
        // ==================== ESTADÍSTICAS ====================
        
        public EstadisticasVendedorViewModel Estadisticas { get; set; } = new();
        
        // ==================== PRODUCTOS ====================
        
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // DATOS PAGO VENDEDOR VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para los datos de pago del vendedor
    /// </summary>
    public class DatosPagoVendedorViewModel
    {
        [Display(Name = "Alias (CBU/MercadoPago)")]
        [StringLength(50)]
        public string? AliasCBU { get; set; }

        [Display(Name = "CBU/CVU")]
        [StringLength(22)]
        public string? CBU { get; set; }
        
        /// <summary>
        /// CBU con caracteres ocultos para mostrar (ej: ****...7890)
        /// </summary>
        public string? CBUOculto { get; set; }

        [Display(Name = "Banco/Billetera")]
        [StringLength(100)]
        public string? Banco { get; set; }

        [Display(Name = "Titular de la Cuenta")]
        [StringLength(100)]
        public string? TitularCuenta { get; set; }

        [Display(Name = "CUIT")]
        [StringLength(13)]
        public string? CUIT { get; set; }

        [Display(Name = "Imagen QR de Pago")]
        [StringLength(500)]
        public string? ImagenQR { get; set; }

        public bool DatosCompletos { get; set; }
        public DateTime? FechaDatosPago { get; set; }
        
        /// <summary>
        /// Mensaje de validación si faltan datos
        /// </summary>
        public string? MensajeValidacion { get; set; }
        
        /// <summary>
        /// Lista de campos faltantes
        /// </summary>
        public List<string> CamposFaltantes { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDITAR DATOS PAGO VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para editar datos de pago
    /// </summary>
    public class EditarDatosPagoViewModel
    {
        public int VendedorId { get; set; }

        [Required(ErrorMessage = "El alias es requerido para recibir pagos")]
        [StringLength(50, ErrorMessage = "El alias no puede exceder 50 caracteres")]
        [Display(Name = "Alias (CBU/MercadoPago)")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El alias solo puede contener letras, números, puntos, guiones y guiones bajos")]
        public string AliasCBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CBU/CVU es requerido")]
        [StringLength(22, MinimumLength = 22, ErrorMessage = "El CBU debe tener exactamente 22 dígitos")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe contener solo números")]
        [Display(Name = "CBU/CVU (22 dígitos)")]
        public string CBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El banco es requerido")]
        [StringLength(100)]
        [Display(Name = "Banco/Billetera")]
        public string Banco { get; set; } = string.Empty;

        [Required(ErrorMessage = "El titular de la cuenta es requerido")]
        [StringLength(100)]
        [Display(Name = "Titular de la Cuenta")]
        public string TitularCuenta { get; set; } = string.Empty;

        [StringLength(13)]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT: XX-XXXXXXXX-X")]
        [Display(Name = "CUIT (opcional)")]
        public string? CUIT { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Debe ser una URL válida")]
        [Display(Name = "URL de imagen QR (opcional)")]
        public string? ImagenQR { get; set; }
        
        // Para subir archivo QR
        public bool SubirNuevoQR { get; set; }
        
        // Lista de bancos disponibles
        public List<string> BancosDisponibles { get; set; } = new()
        {
            "Banco Nación",
            "Banco Provincia",
            "Banco Galicia",
            "Banco Santander",
            "Banco BBVA",
            "Banco Macro",
            "Banco HSBC",
            "Banco Patagonia",
            "Banco Ciudad",
            "Banco Credicoop",
            "MercadoPago",
            "Ualá",
            "Brubank",
            "Naranja X",
            "Otro"
        };
    }

    // ══════════════════════════════════════════════════════════════════════
    // ESTADISTICAS VENDEDOR VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para estadísticas financieras del vendedor
    /// </summary>
    public class EstadisticasVendedorViewModel
    {
        // Cobros
        public decimal TotalCobrado { get; set; }
        public decimal TotalComisionesPagadas { get; set; }
        public decimal PendienteCobro { get; set; }
        
        // Ventas
        public int VentasTotales { get; set; }
        public int VentasEsteMes { get; set; }
        public decimal IngresosBrutosMes { get; set; }
        public decimal IngresosNetosMes { get; set; }
        
        // Productos
        public int ProductosActivos { get; set; }
        public int ProductosAgotados { get; set; }
        
        // Liberaciones
        public int LiberacionesPendientes { get; set; }
        public decimal MontoLiberacionesPendientes { get; set; }
        
        // Última actividad
        public DateTime? UltimaVenta { get; set; }
        public DateTime? UltimaLiberacion { get; set; }
        
        // Calificación
        public decimal CalificacionPromedio { get; set; }
        public int TotalResenas { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // REGISTRO VENDEDOR VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para el registro de vendedor
    /// </summary>
    public class RegistroVendedorViewModel
    {
        // ==================== DATOS PERSONALES ====================
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        // ==================== DATOS COMERCIALES ====================

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre de tu Negocio")]
        public string NombreComercial { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Especialidad (ej: Tortas de Chocolate, Pastelería Francesa)")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción de tu negocio")]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        [Display(Name = "Horario de Atención (ej: Lun-Vie 9:00-18:00)")]
        public string? Horario { get; set; }

        // ==================== DATOS DE PAGO (OPCIONALES EN REGISTRO) ====================

        [StringLength(50)]
        [Display(Name = "Alias (CBU/MercadoPago)")]
        public string? AliasCBU { get; set; }

        [StringLength(22)]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe tener exactamente 22 dígitos")]
        [Display(Name = "CBU/CVU")]
        public string? CBU { get; set; }

        [StringLength(100)]
        [Display(Name = "Banco")]
        public string? Banco { get; set; }

        [StringLength(100)]
        [Display(Name = "Titular de la Cuenta")]
        public string? TitularCuenta { get; set; }

        [StringLength(13)]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT: XX-XXXXXXXX-X")]
        [Display(Name = "CUIT")]
        public string? CUIT { get; set; }

        // ==================== TÉRMINOS ====================

        [Required(ErrorMessage = "Debe aceptar los términos y condiciones")]
        [Display(Name = "Acepto los términos y condiciones")]
        public bool AceptaTerminos { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // MIS LIBERACIONES VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para la lista de liberaciones del vendedor
    /// </summary>
    public class MisLiberacionesViewModel
    {
        public int VendedorId { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        
        // Resumen
        public decimal TotalCobrado { get; set; }
        public decimal PendienteCobro { get; set; }
        public int LiberacionesPendientes { get; set; }
        
        // Lista de liberaciones
        public List<LiberacionDTO> Liberaciones { get; set; } = new();
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        
        // Filtros
        public string? FiltroEstado { get; set; }
        public DateTime? FiltroDesde { get; set; }
        public DateTime? FiltroHasta { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // VENDEDOR LISTA VIEW MODEL (para listados administrativos)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para listados de vendedores con paginación y filtros
    /// </summary>
    public class VendedorListaViewModel
    {
        public List<VendedorViewModel> Vendedores { get; set; } = new();
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosPorPagina { get; set; } = 10;
        
        // Filtros
        public string? Busqueda { get; set; }
        public bool? SoloVerificados { get; set; }
        public bool? SoloActivos { get; set; }
        public string? OrdenarPor { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // VENDEDOR PERFIL PUBLICO VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para mostrar el perfil público de un vendedor (visible para compradores)
    /// </summary>
    public class VendedorPerfilPublicoViewModel
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public string? Horario { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public bool Verificado { get; set; }
        public string? Avatar { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Productos del vendedor
        public int TotalProductos { get; set; }
        public int ProductosDisponibles { get; set; }
        public List<TortaResumenViewModel> Tortas { get; set; } = new();

        // Estadísticas públicas
        public int TotalResenas { get; set; }
        public decimal CalificacionPromedio { get; set; }
    }

    /// <summary>
    /// ViewModel resumido de torta para listas
    /// </summary>
    public class TortaResumenViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? PrecioOferta { get; set; }
        public string? ImagenPrincipal { get; set; }
        public bool Disponible { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDITAR VENDEDOR VIEW MODEL
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel para editar los datos del vendedor
    /// </summary>
    public class EditarVendedorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        // Datos de contacto (de Persona)
        [Phone]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }
    }
}
