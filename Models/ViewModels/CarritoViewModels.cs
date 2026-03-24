using CasaDeLasTortas.Models;
using CasaDeLasTortas.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CasaDeLasTortas.Models.ViewModels
{
    public class CarritoViewModel
    {
        public List<CarritoItemSession> Items { get; set; } = new();
        public List<CarritoPorVendedorViewModel> ItemsPorVendedor { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public bool TieneItems => Items.Any();
    }

    public class CarritoPorVendedorViewModel
    {
        public int VendedorId { get; set; }
        public string NombreVendedor { get; set; } = string.Empty;
        public List<CarritoItemSession> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
    }

    public class CheckoutViewModel
    {
        // Datos del carrito
        public CarritoSession Carrito { get; set; } = new();
        
        // Datos de entrega
        [Required(ErrorMessage = "La dirección es requerida")]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; } = string.Empty;
        
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }
        
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }
        
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }
        
        [Display(Name = "Teléfono de Contacto")]
        [Phone]
        public string? TelefonoContacto { get; set; }
        
        [Display(Name = "Email de Contacto")]
        [EmailAddress]
        public string? EmailContacto { get; set; }
        
        [Display(Name = "Notas adicionales")]
        [DataType(DataType.MultilineText)]
        public string? Notas { get; set; }
        
        // Método de pago
        [Required(ErrorMessage = "Seleccione un método de pago")]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; }
        
        // Opciones
        [Display(Name = "Usar dirección del perfil")]
        public bool UsarDireccionPerfil { get; set; } = true;
        
        [Display(Name = "Requiere factura")]
        public bool RequiereFactura { get; set; }
        
        [Display(Name = "RUC/Cédula")]
        public string? RucOCedula { get; set; }
        
        [Display(Name = "Razón Social")]
        public string? RazonSocial { get; set; }
        
        // Propiedad calculada
        public string DireccionEntregaCompleta => 
            $"{DireccionEntrega}" + 
            (!string.IsNullOrEmpty(Ciudad) ? $", {Ciudad}" : "") +
            (!string.IsNullOrEmpty(Provincia) ? $", {Provincia}" : "") +
            (!string.IsNullOrEmpty(CodigoPostal) ? $" - {CodigoPostal}" : "");
    }
}