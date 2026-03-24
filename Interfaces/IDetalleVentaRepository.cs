using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IDetalleVentaRepository
    {
        // CRUD Básico
        Task<DetalleVenta?> GetByIdAsync(int id);
        Task<IEnumerable<DetalleVenta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 20);
        Task AddAsync(DetalleVenta detalle);
        void Update(DetalleVenta detalle);
        Task UpdateAsync(DetalleVenta detalle);
        void Delete(DetalleVenta detalle);
        Task DeleteAsync(DetalleVenta detalle);
        Task AddRangeAsync(IEnumerable<DetalleVenta> detalles);

        // Métodos con relaciones
        Task<DetalleVenta?> GetByIdWithTodoAsync(int id); // Con Venta, Torta, Vendedor
        Task<IEnumerable<DetalleVenta>> GetByVentaIdAsync(int ventaId);
        Task<IEnumerable<DetalleVenta>> GetByVentaIdWithDetailsAsync(int ventaId);

        // Filtros por Vendedor
        Task<IEnumerable<DetalleVenta>> GetByVendedorIdAsync(int vendedorId);
        Task<IEnumerable<DetalleVenta>> GetByVendedorIdPendientesAsync(int vendedorId);
        Task<IEnumerable<DetalleVenta>> GetByVendedorIdWithVentaAsync(int vendedorId);

        // Filtros por Torta
        Task<IEnumerable<DetalleVenta>> GetByTortaIdAsync(int tortaId);
        Task<int> GetTotalVendidoByTortaAsync(int tortaId);

        // Filtros por Estado
        Task<IEnumerable<DetalleVenta>> GetByEstadoAsync(EstadoDetalleVenta estado);
        Task<IEnumerable<DetalleVenta>> GetPendientesByVendedorAsync(int vendedorId);

        // Estadísticas
        Task<int> CountAsync();
        Task<decimal> GetTotalVentasByVendedorAsync(int vendedorId);
        Task<Dictionary<int, int>> GetVentasPorTortaAsync(int vendedorId, int limite = 10);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<DetalleVenta, bool>> predicate);
        Task<bool> TieneDetallesPendientesAsync(int ventaId);
    }
}