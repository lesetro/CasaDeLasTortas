using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IPagoRepository
    {
        // CRUD básico
        Task<Pago?> GetByIdAsync(int id);
        Task<IEnumerable<Pago>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Pago pago);
        void Update(Pago pago);
        Task UpdateAsync(Pago pago); 
        void Delete(Pago pago);
        Task DeleteAsync(Pago pago); 

        // Métodos con relaciones
        Task<Pago?> GetByIdWithDetallesAsync(int id);
        Task<Pago?> GetByIdWithTortaAsync(int id);
        Task<Pago?> GetByIdWithCompradorAsync(int id);
        Task<IEnumerable<Pago>> GetAllWithDetallesAsync(int pagina = 1, int registrosPorPagina = 10);
        Task<IEnumerable<Pago>> GetAllWithDetailsAsync(); 

        // Filtros por relaciones
        Task<IEnumerable<Pago>> GetByCompradorIdAsync(int compradorId);
        Task<IEnumerable<Pago>> GetByCompradorIdWithDetailsAsync(int compradorId);
        Task<IEnumerable<Pago>> GetByTortaAsync(int tortaId);
        Task<IEnumerable<Pago>> GetByVendedorAsync(int vendedorId);

        // Filtros por características
        Task<IEnumerable<Pago>> GetByEstadoAsync(EstadoPago estado);
        Task<IEnumerable<Pago>> GetByMetodoPagoAsync(MetodoPago metodoPago);
        Task<IEnumerable<Pago>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<Pago>> SearchAsync(string termino);

        // Estadísticas
        Task<int> CountAsync();
        Task<decimal> GetTotalIngresosAsync();
        Task<decimal> GetIngresosByVendedorAsync(int vendedorId);
        Task<decimal> GetIngresosByCompradorAsync(int compradorId);
        Task<decimal> GetIngresosByFechaAsync(DateTime desde, DateTime hasta);

        // Estadísticas por estado
        Task<int> CountByEstadoAsync(EstadoPago estado);
        Task<int> CountPendientesAsync();
        Task<int> CountCompletadosAsync();
        Task<int> CountCanceladosAsync();

        // Estadísticas por método de pago
        Task<Dictionary<string, int>> GetCountByMetodoPagoAsync();
        Task<Dictionary<string, decimal>> GetTotalByMetodoPagoAsync();


        // Reportes
        Task<IEnumerable<Pago>> GetPagosPendientesAsync();
        Task<IEnumerable<Pago>> GetPagosCompletadosAsync();
        Task<IEnumerable<Pago>> GetUltimosPagosAsync(int cantidad = 10);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<Pago, bool>> predicate);
        Task<bool> TieneComprobanteAsync(int pagoId);

        // Pagos del día
        Task<IEnumerable<Pago>> GetPagosDelDiaAsync(DateTime fecha);
        Task<decimal> GetIngresosDelDiaAsync(DateTime fecha);

        // Métodos específicos para dashboard
        Task<Dictionary<DateTime, decimal>> GetIngresosUltimos7DiasAsync();
        Task<IEnumerable<Pago>> GetPagosConEntregaProximaAsync();
        Task<IEnumerable<Pago>> GetByVendedorIdAsync(int vendedorId);

        // ✅ MÉTODOS NUEVOS PARA EL PAGO CONTROLLER
        Task<decimal> GetVentasDelDiaAsync();
        Task<decimal> GetVentasDelMesAsync();
    }
}