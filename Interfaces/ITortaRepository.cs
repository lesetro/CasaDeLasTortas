using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface ITortaRepository
    {
        // Métodos básicos CRUD
        Task<Torta?> GetByIdAsync(int id);
        Task<IEnumerable<Torta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Torta torta);
        void Update(Torta torta);
        Task UpdateAsync(Torta torta); 
        void Delete(Torta torta);
        
        // Métodos con relaciones
        Task<Torta?> GetByIdWithVendedorAsync(int id);
        Task<Torta?> GetByIdWithImagenesAsync(int id);
        Task<Torta?> GetByIdWithDetallesCompletosAsync(int id);
        Task<IEnumerable<Torta>> GetAllWithVendedorAsync();
        Task<IEnumerable<Torta>> GetAllWithImagenesAsync();
        
        // Búsqueda por Vendedor
        Task<IEnumerable<Torta>> GetByVendedorIdAsync(int idVendedor);
        Task<int> CountByVendedorIdAsync(int idVendedor);
        
        // Filtros
        Task<IEnumerable<Torta>> GetByCategoriaAsync(string categoria);
        Task<IEnumerable<Torta>> GetByPrecioRangeAsync(decimal precioMin, decimal precioMax);
        Task<IEnumerable<Torta>> GetDisponiblesAsync();
        Task<IEnumerable<Torta>> SearchAsync(string termino);
        Task<Torta?> GetByIdWithVentasAsync(int id);
        

        
        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountDisponiblesAsync();
        Task<decimal> GetPrecioPromedioAsync();
        Task<Torta?> GetByIdWithPagosAsync(int id);
        Task<IEnumerable<Torta>> GetByPrecioRangoAsync(decimal min, decimal max);

        Task<IEnumerable<TortaListDTO>> GetAllWithVendedorAsync(int pagina = 1, int registrosPorPagina = 10);
        
        // Ordenamiento
        Task<IEnumerable<Torta>> GetTopVendidasAsync(int cantidad = 10);
        Task<IEnumerable<Torta>> GetTopCalificadasAsync(int cantidad = 10);
        Task<IEnumerable<Torta>> GetRecientesAsync(int cantidad = 10);

        // Método adicional
        Task<bool> ExistsAsync(Expression<Func<Torta, bool>> predicate);

        // MÉTODOS NUEVOS PARA EL HOME CONTROLLER
        Task<IEnumerable<Torta>> GetTortasDisponiblesConDetallesAsync();
        Task<IEnumerable<string>> GetCategoriasAsync();
        Task<IEnumerable<Torta>> GetTortasDestacadasAsync(int cantidad);
        Task<IEnumerable<Torta>> GetTortasPorCategoriasAsync(List<string?> categorias, int cantidad);
        
        Task<IEnumerable<Torta>> GetTortasNuevasHoyAsync();

        // MÉTODOS NUEVOS PARA EL PAGO CONTROLLER
        Task<IEnumerable<Torta>> GetTortasDisponiblesAsync();
        Task<Torta?> GetByIdWithDetallesAsync(int id);
    }
}