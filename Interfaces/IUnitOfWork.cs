using System;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Interfaces
{
    /// <summary>
    /// Patrón Unit of Work que agrupa todos los repositorios
    /// y maneja las transacciones de base de datos
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repositorios
        IPersonaRepository PersonaRepository { get; }
        IVendedorRepository VendedorRepository { get; }
        ICompradorRepository CompradorRepository { get; }
        ITortaRepository TortaRepository { get; }
        IImagenTortaRepository ImagenesTorta { get; }
        IPagoRepository PagoRepository { get; }
        
        // Métodos de transacción básicos
        Task<int> SaveChangesAsync();
        Task<int> SaveAsync();
        
        // Transacciones explícitas
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        // Métodos adicionales
        void DetachAllEntities();
        Task<bool> SaveChangesReturnBoolAsync();
        
        // ✅ MÉTODOS NUEVOS AGREGADOS
        Task<bool> AnyChangesAsync();
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    }
}