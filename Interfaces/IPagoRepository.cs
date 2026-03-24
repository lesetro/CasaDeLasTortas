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

        // Con relaciones
        Task<Pago?> GetByIdWithDetallesAsync(int id);
        Task<Pago?> GetByIdWithVentaAsync(int id);
        Task<Pago?> GetByIdWithCompradorAsync(int id);
        Task<IEnumerable<Pago>> GetAllWithDetailsAsync();
        Task<Pago?> GetByIdWithVerificadorAsync(int id);

        // Filtros por relaciones
        Task<IEnumerable<Pago>> GetByCompradorIdAsync(int compradorId);
        Task<IEnumerable<Pago>> GetByCompradorIdWithDetailsAsync(int compradorId);
        Task<IEnumerable<Pago>> GetByVentaIdAsync(int ventaId);
        Task<IEnumerable<Pago>> GetByVendedorAsync(int vendedorId);
        Task<IEnumerable<Pago>> GetByVendedorIdAsync(int vendedorId);

        // Filtros por características
        Task<IEnumerable<Pago>> GetByEstadoAsync(EstadoPago estado);
        Task<IEnumerable<Pago>> GetByMetodoPagoAsync(MetodoPago metodoPago);
        Task<IEnumerable<Pago>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<Pago>> SearchAsync(string termino);
        
        // ✅ NUEVOS: Filtros para Admin
        Task<IEnumerable<Pago>> GetPendientesVerificacionAsync();
        Task<IEnumerable<Pago>> GetEnRevisionAsync();
        Task<IEnumerable<Pago>> GetRechazadosAsync();
        Task<IEnumerable<Pago>> GetReembolsosPendientesAsync();

        // Estadísticas
        Task<int> CountAsync();
        Task<decimal> GetTotalIngresosAsync();
        Task<decimal> GetIngresosByVendedorAsync(int vendedorId);
        Task<decimal> GetIngresosByCompradorAsync(int compradorId);
        Task<decimal> GetIngresosByFechaAsync(DateTime desde, DateTime hasta);
        
        // ✅ NUEVOS: Estadísticas de comisiones
        Task<decimal> GetTotalComisionesAsync();
        Task<decimal> GetComisionesDelMesAsync();
        Task<decimal> GetMontoEnRevisionAsync();

        // Estadísticas por estado
        Task<int> CountByEstadoAsync(EstadoPago estado);
        Task<int> CountPendientesAsync();
        Task<int> CountCompletadosAsync();
        Task<int> CountCanceladosAsync();
        Task<int> CountEnRevisionAsync();
        Task<int> CountRechazadosAsync();

        // Estadísticas por método de pago
        Task<Dictionary<MetodoPago, int>> GetCountByMetodoPagoAsync();
        Task<Dictionary<MetodoPago, decimal>> GetTotalByMetodoPagoAsync();

        // Reportes
        Task<IEnumerable<Pago>> GetPagosPendientesAsync();
        Task<IEnumerable<Pago>> GetPagosCompletadosAsync();
        Task<IEnumerable<Pago>> GetUltimosPagosAsync(int cantidad = 10);
        Task<IEnumerable<Pago>> GetPagosVerificadosHoyAsync();

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<Pago, bool>> predicate);
        Task<bool> TieneComprobanteAsync(int pagoId);
        Task<bool> PuedeSerVerificadoAsync(int pagoId);
        Task<bool> PuedeSubirComprobanteAsync(int pagoId);
        Task<int> GetIntentosRechazadosAsync(int pagoId);

        // Pagos del día
        Task<IEnumerable<Pago>> GetPagosDelDiaAsync(DateTime fecha);
        Task<decimal> GetIngresosDelDiaAsync(DateTime fecha);
        Task<decimal> GetVentasDelDiaAsync();
        Task<decimal> GetVentasDelMesAsync();

        // Dashboard
        Task<Dictionary<DateTime, decimal>> GetIngresosUltimos7DiasAsync();
        Task<IEnumerable<Pago>> GetPagosConEntregaProximaAsync();
        Task<double> GetTiempoPromedioVerificacionAsync();
    }
}