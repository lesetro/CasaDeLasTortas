using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IPersonaRepository
    {
        // Métodos básicos CRUD
        Task<Persona?> GetByIdAsync(int id);
        Task<IEnumerable<Persona>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Persona persona);
        void Update(Persona persona);
        void Delete(Persona persona);
        Task DeleteAsync(Persona persona);
        
        // Búsqueda y consultas
        Task<Persona?> GetByEmailAsync(string email);
        Task<IEnumerable<Persona>> SearchAsync(string termino);
        Task<IEnumerable<Persona>> GetByRolAsync(string rol);
        
        // Validaciones
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<Persona, bool>> predicate);
        
        // Estadísticas y conteos
        Task<int> CountAsync();
        Task<int> CountByRolAsync(string rol);
        
        // Filtros avanzados
        Task<IEnumerable<Persona>> GetActivasAsync();
        Task<IEnumerable<Persona>> GetInactivasAsync();
        Task<IEnumerable<Persona>> GetByFechaRegistroAsync(DateTime desde, DateTime hasta);

        // Método adicional para update asíncrono
        Task UpdateAsync(Persona persona);
        Task<IEnumerable<Persona>> GetPersonasSinCompradorAsync();
        Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync();
        Task<bool> ExistsByEmailAsync(string email);
        
    }
}