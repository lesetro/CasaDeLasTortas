using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IVendedorRepository
    {
        // Métodos básicos CRUD
        Task<Vendedor?> GetByIdAsync(int id);
        Task<IEnumerable<Vendedor>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Vendedor vendedor);
        void Update(Vendedor vendedor);
        void Delete(Vendedor vendedor);
        Task UpdateAsync(Vendedor vendedor);
        Task DeleteAsync(Vendedor vendedor);
        
        
        // Métodos con relaciones
        Task<Vendedor?> GetByIdWithPersonaAsync(int id);
        Task<Vendedor?> GetByIdWithTortasAsync(int id);
        Task<Vendedor?> GetByIdWithDetallesCompletosAsync(int id);
        Task<IEnumerable<Vendedor>> GetAllWithPersonaAsync();
        
        // Búsqueda por Persona
        Task<Vendedor?> GetByPersonaIdAsync(int idPersona);
        Task<bool> ExistsByPersonaIdAsync(int idPersona);
        Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync();
        
        // Filtros
        Task<IEnumerable<Vendedor>> GetByEspecialidadAsync(string especialidad);
        Task<IEnumerable<Vendedor>> GetActivosAsync();
        Task<IEnumerable<Vendedor>> GetVerificadosAsync();
        Task<IEnumerable<Vendedor>> SearchAsync(string termino);
        
        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountActivosAsync();
        Task<decimal> GetCalificacionPromedioAsync(int idVendedor);
        Task<int> GetTotalVentasAsync(int idVendedor);
        
        // Ordenamiento
        Task<IEnumerable<Vendedor>> GetTopByCalificacionAsync(int cantidad = 10);
        Task<IEnumerable<Vendedor>> GetTopByVentasAsync(int cantidad = 10);

        // MÉTODO NUEVO AGREGADO
        Task<bool> ExistsAsync(Expression<Func<Vendedor, bool>> predicate);
        Task<int> GetCountAsync();
    }
}