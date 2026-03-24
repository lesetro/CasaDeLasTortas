using System;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositorios existentes
        IPersonaRepository PersonaRepository { get; }
        IVendedorRepository VendedorRepository { get; }
        ICompradorRepository CompradorRepository { get; }
        ITortaRepository TortaRepository { get; }
        IImagenTortaRepository ImagenesTorta { get; }
        IPagoRepository PagoRepository { get; }
        IVentaRepository Ventas { get; }
        IDetalleVentaRepository DetallesVenta { get; }
        
        // ✅ NUEVOS REPOSITORIOS
        ILiberacionRepository Liberaciones { get; }
        IDisputaRepository Disputas { get; }
        IConfiguracionRepository Configuracion { get; }
        
        // Transacciones
        Task<int> SaveChangesAsync();
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        // Utilidades
        void DetachAllEntities();
        Task<bool> SaveChangesReturnBoolAsync();
        Task<bool> AnyChangesAsync();
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    }
}