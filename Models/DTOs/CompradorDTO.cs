using System;
using System.Collections.Generic;

namespace CasaDeLasTortas.Models.DTOs
{
    public class CompradorDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public int TotalCompras { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        
        // Información de la persona
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
    }

    public class CompradorCreateDTO
    {
        public int PersonaId { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
    }

    public class CompradorUpdateDTO
    {
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
        public bool Activo { get; set; }
    }

    public class CompradorListDTO
    {
        public int Id { get; set; }
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string Ciudad { get; set; }
        public int TotalCompras { get; set; }
        public bool Activo { get; set; }
    }

    public class CompradorPerfilDTO
    {
        public int Id { get; set; }
        public string NombrePersona { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string DireccionCompleta { get; set; }
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public List<PagoHistorialDTO> HistorialCompras { get; set; }
    }
}