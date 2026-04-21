using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Interfaces
{
    public interface IDisputaRepository
    {
        // CRUD
        Task<Disputa?> GetByIdAsync(int id);
        Task<IEnumerable<Disputa>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Disputa disputa);
        void Update(Disputa disputa);
        Task UpdateAsync(Disputa disputa);
        
        // Con relaciones
        Task<Disputa?> GetByIdWithMensajesAsync(int id);
        Task<IEnumerable<Disputa>> GetAllWithDetallesAsync();
        Task<Disputa?> GetByNumeroDisputaAsync(string numeroDisputa);
        Task<IEnumerable<Disputa>> GetByVentaIdAsync(int ventaId);
        Task<IEnumerable<Disputa>> GetByIniciadorIdAsync(int personaId);
        
        // Por estado
        Task<IEnumerable<Disputa>> GetByEstadoAsync(EstadoDisputa estado);
        Task<IEnumerable<Disputa>> GetAbiertasAsync();
        Task<IEnumerable<Disputa>> GetSinAsignarAsync();
        
        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountAbiertasAsync();
        Task<decimal> GetMontoTotalInvolucradoAsync();
        
        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExisteDisputaAbiertaParaVentaAsync(int ventaId);
        
        // Mensajes
        Task<IEnumerable<MensajeDisputa>> GetMensajesByDisputaIdAsync(int disputaId);
        Task AddMensajeAsync(MensajeDisputa mensaje);
        
        // Operaciones
        Task<string> GenerarNumeroDisputaAsync();
        Task AsignarAdminAsync(int disputaId, int adminId);
        Task CambiarEstadoAsync(int disputaId, EstadoDisputa nuevoEstado);
        Task ResolverAsync(int disputaId, ResolucionDisputa resolucion, string detalle, decimal? montoResolucion = null);
    }
}