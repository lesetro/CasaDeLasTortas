using CasaDeLasTortas.Models;
using CasaDeLasTortas.Models.Entities;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Interfaces
{
    public interface ICarritoService
    {
        // Gestión del carrito en session/cookie
        CarritoSession ObtenerCarrito();
        void GuardarCarrito(CarritoSession carrito);
        void VaciarCarrito();
        
        // Operaciones con items
        Task<bool> AgregarItem(int tortaId, int cantidad, string? notas = null);
        Task<bool> QuitarItem(int tortaId);
        Task<bool> ActualizarCantidad(int tortaId, int cantidad);
        Task<bool> ActualizarNotas(int tortaId, string? notas);
        
        // Utilidades
        int GetTotalItems();
        Task<CarritoSession> ObtenerCarritoConDetalles();
        
        // Fusión de carritos (cuando usuario se loguea)
        Task<CarritoSession> FusionarCarritos(CarritoSession carritoAnonimo, int compradorId);
        
        // Conversión a venta
        Task<Venta?> ConvertirAVenta(int compradorId, string direccionEntrega, string? notas = null);
    }
}