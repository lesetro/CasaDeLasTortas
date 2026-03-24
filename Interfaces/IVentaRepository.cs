using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IVentaRepository
    {
        // CRUD Básico
        Task<Venta?> GetByIdAsync(int id);
        Task<IEnumerable<Venta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Venta venta);
        void Update(Venta venta);
        Task UpdateAsync(Venta venta);
        void Delete(Venta venta);
        Task DeleteAsync(Venta venta);

        // Métodos con relaciones
        Task<Venta?> GetByIdWithDetallesAsync(int id);
        Task<Venta?> GetByIdWithCompradorAsync(int id);
        Task<Venta?> GetByIdWithPagosAsync(int id);
        Task<Venta?> GetByIdWithTodoAsync(int id); // Comprador + Detalles + Pagos
        Task<IEnumerable<Venta>> GetAllWithDetailsAsync();

        // Filtros por Comprador
        Task<IEnumerable<Venta>> GetByCompradorIdAsync(int compradorId);
        Task<IEnumerable<Venta>> GetByCompradorIdWithDetailsAsync(int compradorId);

        // Filtros por Estado
        Task<IEnumerable<Venta>> GetByEstadoAsync(EstadoVenta estado);
        Task<IEnumerable<Venta>> GetPendientesAsync();
        Task<IEnumerable<Venta>> GetEnProcesoAsync();
        Task<IEnumerable<Venta>> GetCompletadasAsync();
        Task<IEnumerable<Venta>> GetCanceladasAsync();

        // Filtros por Vendedor (a través de DetalleVenta)
        Task<IEnumerable<Venta>> GetByVendedorIdAsync(int vendedorId);
        Task<IEnumerable<Venta>> GetByVendedorIdWithDetailsAsync(int vendedorId);

        // Filtros por fecha
        Task<IEnumerable<Venta>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<Venta>> GetVentasDelDiaAsync(DateTime fecha);
        Task<IEnumerable<Venta>> GetVentasDelMesAsync(int año, int mes);

        // Búsqueda
        Task<IEnumerable<Venta>> SearchAsync(string termino); // Por número de orden, comprador, etc.

        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountByCompradorAsync(int compradorId);
        Task<int> CountByVendedorAsync(int vendedorId);
        Task<decimal> GetTotalIngresosAsync();
        Task<decimal> GetTotalIngresosByVendedorAsync(int vendedorId);
        Task<decimal> GetTotalIngresosByCompradorAsync(int compradorId);
        Task<decimal> GetTotalIngresosByFechaRangoAsync(DateTime desde, DateTime hasta);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<Venta, bool>> predicate);

        // Generar número de orden
        Task<string> GenerarNumeroOrdenAsync();
    }
}