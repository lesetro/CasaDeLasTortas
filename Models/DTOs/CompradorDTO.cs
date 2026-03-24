using System;
using System.Collections.Generic;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.DTOs
{
    public class CompradorDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public int TotalCompras { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        
        // Información de la persona
        public string NombrePersona { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class CompradorCreateDTO
    {
        public int PersonaId { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
    }

    public class CompradorUpdateDTO
    {
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
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
        public string NombrePersona { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public int TotalCompras { get; set; }
        public bool Activo { get; set; }
    }

    public class CompradorPerfilDTO
    {
        public int Id { get; set; }
        public string NombrePersona { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string DireccionCompleta { get; set; } = string.Empty;
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public List<PagoHistorialDTO> HistorialCompras { get; set; } = new();
    }

    /// <summary>
    /// DTO para mostrar el historial de pagos/compras del comprador
    /// </summary>
    public class PagoHistorialDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        
        // Monto
        public decimal Monto { get; set; }
        
        // Estado
        public EstadoPago Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        
        // Método de pago
        public MetodoPago? MetodoPago { get; set; }
        public string? MetodoPagoTexto { get; set; }
        
        // Fechas
        public DateTime FechaPago { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        
        // Resumen de productos
        public int CantidadProductos { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string> NombresTortas { get; set; } = new();
        
        // Vendedores involucrados
        public int CantidadVendedores { get; set; }
        public List<string> NombresVendedores { get; set; } = new();
        
        // Propiedades calculadas
        public bool EstaCompletado => Estado == EstadoPago.Verificado || Estado == EstadoPago.Completado;
        public bool EstaPendiente => Estado == EstadoPago.Pendiente || Estado == EstadoPago.EnRevision;
        
        public string EstadoColor => Estado switch
        {
            EstadoPago.Pendiente => "warning",
            EstadoPago.EnRevision => "info",
            EstadoPago.Verificado => "success",
            EstadoPago.Completado => "success",
            EstadoPago.Rechazado => "danger",
            EstadoPago.Cancelado => "secondary",
            EstadoPago.ReembolsoPendiente => "warning",
            EstadoPago.Reembolsado => "dark",
            _ => "secondary"
        };
    }
}