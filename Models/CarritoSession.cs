using System;
using System.Collections.Generic;

namespace CasaDeLasTortas.Models
{
    /// <summary>
    /// Modelo para guardar el carrito en Session/Cookies (NO en BD)
    /// </summary>
    [Serializable]
    public class CarritoSession
    {
        public List<CarritoItemSession> Items { get; set; } = new List<CarritoItemSession>();
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        
        // Propiedades calculadas
        public int TotalItems => Items.Sum(i => i.Cantidad);
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal DescuentoTotal => Items.Sum(i => i.Descuento);
        public decimal Total => Subtotal - DescuentoTotal;
    }

    [Serializable]
    public class CarritoItemSession
    {
        public int TortaId { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; } = 0;
        public string? NotasPersonalizacion { get; set; }
        
        // Datos del vendedor (para agrupar después)
        public int VendedorId { get; set; }
        public string NombreVendedor { get; set; }
        
        // Imagen principal para mostrar
        public string? ImagenPrincipal { get; set; }
        
        // Propiedades calculadas
        public decimal Subtotal => PrecioUnitario * Cantidad;
        public decimal Total => Subtotal - Descuento;
    }
}