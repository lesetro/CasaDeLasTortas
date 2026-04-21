using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface ICompradorRepository
    {
        // Métodos básicos CRUD
        Task<Comprador?> GetByIdAsync(int id);
        Task<IEnumerable<Comprador>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Comprador comprador);
        void Update(Comprador comprador);
        void Delete(Comprador comprador);
        Task DeleteAsync(Comprador comprador);
        Task UpdateAsync(Comprador comprador);
        
        // Métodos con relaciones
        Task<Comprador?> GetByIdWithPersonaAsync(int id);
        Task<Comprador?> GetByIdWithPagosAsync(int id);
        Task<Comprador?> GetByIdWithDetallesCompletosAsync(int id);
        Task<IEnumerable<Comprador>> GetAllWithPersonaAsync();
        
        // Búsqueda por Persona
        Task<Comprador?> GetByPersonaIdAsync(int idPersona);
        Task<bool> ExistsByPersonaIdAsync(int idPersona);
        
        // Filtros
        Task<IEnumerable<Comprador>> GetByCiudadAsync(string ciudad);
        Task<IEnumerable<Comprador>> SearchAsync(string termino);
        Task<IEnumerable<Comprador>> GetActivosAsync();
        
        // Estadísticas
        Task<int> CountAsync();
        Task<decimal> GetTotalGastadoAsync(int idComprador);
        Task<int> GetTotalComprasAsync(int idComprador);
        
        // Historial
        Task<IEnumerable<Comprador>> GetTopCompradores(int cantidad = 10);
        Task<IEnumerable<Comprador>> GetCompradorReciente(int dias = 30);

        
        Task<bool> ExistsAsync(Expression<Func<Comprador, bool>> predicate);
    }
}