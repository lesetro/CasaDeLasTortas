using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IImagenTortaRepository
    {
        // ==================== CRUD BÁSICO ====================
        Task<ImagenTorta?> GetByIdAsync(int id);
        Task<IEnumerable<ImagenTorta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 20);
        Task AddAsync(ImagenTorta imagen);
        void Update(ImagenTorta imagen);
        void Delete(ImagenTorta imagen);
        
        // ==================== MÉTODOS CON RELACIONES ====================
        Task<ImagenTorta?> GetByIdWithTortaAsync(int id);
        Task<IEnumerable<ImagenTorta>> GetAllWithTortaAsync();
        
        // ==================== FILTROS POR TORTA ====================
        Task<IEnumerable<ImagenTorta>> GetByTortaIdAsync(int tortaId);
        Task<ImagenTorta?> GetImagenPrincipalAsync(int tortaId);
        Task<int> CountByTortaIdAsync(int tortaId);
        
        // ==================== GESTIÓN DE IMAGEN PRINCIPAL ====================
        Task<bool> SetImagenPrincipalAsync(int imagenId);
        Task RemoverPrincipalAsync(int tortaId);
        Task<bool> TieneImagenPrincipalAsync(int tortaId);
        
        // ==================== ORDENAMIENTO ====================
        Task<IEnumerable<ImagenTorta>> GetByTortaIdOrderedAsync(int tortaId);
        
        // ==================== ESTADÍSTICAS ====================
        Task<int> CountAsync();
        Task<bool> ExistsAsync(int id);
        
        // ==================== ELIMINAR MÚLTIPLES ====================
        Task DeleteByTortaIdAsync(int tortaId);
        
        // ==================== MÉTODOS ADICIONALES ====================
        Task<bool> ExistsAsync(Expression<Func<ImagenTorta, bool>> predicate);
        Task<bool> SaveChangesAsync();
        Task<bool> UpdateOrdenAsync(int imagenId, int nuevoOrden);
        Task ReordenarImagenesAsync(int tortaId);
        Task<IEnumerable<ImagenTorta>> GetImagenesRecientesAsync(int dias = 7);
        Task<long> GetTotalTamanioArchivosAsync(int tortaId);
        Task<int> GetMaxOrdenAsync(int tortaId);
        Task<IEnumerable<ImagenTorta>> GetImagenesPrincipalesAsync();
        Task<IEnumerable<ImagenTorta>> GetByTipoArchivoAsync(string tipoArchivo);
        Task<IEnumerable<ImagenTorta>> SearchAsync(string termino);

        Task<IEnumerable<ImagenTorta>> GetByTortaAsync(int tortaId);
        Task<ImagenTorta?> GetPrincipalByTortaAsync(int tortaId);
    }
}