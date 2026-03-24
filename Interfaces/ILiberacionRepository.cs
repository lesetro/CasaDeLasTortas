using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface ILiberacionRepository
    {
        // CRUD
        Task<LiberacionFondos?> GetByIdAsync(int id);
        Task<IEnumerable<LiberacionFondos>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(LiberacionFondos liberacion);
        void Update(LiberacionFondos liberacion);
        Task UpdateAsync(LiberacionFondos liberacion);
        
        // Con relaciones
        Task<LiberacionFondos?> GetByIdWithDetallesAsync(int id);
        Task<IEnumerable<LiberacionFondos>> GetByVendedorIdAsync(int vendedorId);
        Task<IEnumerable<LiberacionFondos>> GetByVentaIdAsync(int ventaId);
        
        // Por estado
        Task<IEnumerable<LiberacionFondos>> GetByEstadoAsync(EstadoLiberacion estado);
        Task<IEnumerable<LiberacionFondos>> GetPendientesAsync();
        Task<IEnumerable<LiberacionFondos>> GetListasParaLiberarAsync();
        
        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountByEstadoAsync(EstadoLiberacion estado);
        Task<decimal> GetMontoTotalLiberadoAsync();
        Task<decimal> GetMontoPendienteLiberacionAsync();
        Task<decimal> GetMontoLiberadoByVendedorAsync(int vendedorId);
        
        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistePendienteParaVentaAsync(int ventaId, int vendedorId);
        
        // Operaciones
        Task<IEnumerable<LiberacionFondos>> CrearLiberacionesParaVentaAsync(int ventaId, decimal porcentajeComision);
        Task MarcarListaParaLiberarAsync(int liberacionId);
        Task RegistrarTransferenciaAsync(int liberacionId, string numeroOperacion, string archivoComprobante, int adminId);
        Task ConfirmarRecepcionAsync(int liberacionId);
    }
}