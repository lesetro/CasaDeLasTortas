using System;
using System.Collections.Generic;

namespace CasaDeLasTortas.Models.DTOs
{
    public class VendedorDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string NombreComercial { get; set; }
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public string? Horario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Verificado { get; set; }
        public bool Activo { get; set; }
        
        // Información de la persona
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        
        // Estadísticas
        public int TotalTortas { get; set; }
        public decimal IngresosTotales { get; set; }
    }

    public class VendedorCreateDTO
    {
        public int PersonaId { get; set; }
        public string NombreComercial { get; set; }
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public string? Horario { get; set; }
    }

    public class VendedorUpdateDTO
    {
        public string NombreComercial { get; set; }
        public string? Especialidad { get; set; }
        public string? Descripcion { get; set; }
        public string? Horario { get; set; }
        public bool Activo { get; set; }
    }

    public class VendedorListDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; }
        public string? Especialidad { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public bool Verificado { get; set; }
        public string? Avatar { get; set; }
    }

    public class VendedorDashboardDTO
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; }
        public decimal Calificacion { get; set; }
        public int TotalVentas { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal IngresosMes { get; set; }
        public int VentasMes { get; set; }
        public int TortasActivas { get; set; }
        public int PedidosPendientes { get; set; }
        public List<TortaListDTO> TortasRecientes { get; set; }
        public List<PagoListDTO> UltimosPagos { get; set; }
    }
}