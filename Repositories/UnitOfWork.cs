using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CasaDeLasTortas.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        // Campos de repositorios
        private IPersonaRepository? _personaRepo;
        private IVendedorRepository? _vendedorRepo;
        private ICompradorRepository? _compradorRepo;
        private ITortaRepository? _tortaRepo;
        private IImagenTortaRepository? _imagenTortaRepo;
        private IPagoRepository? _pagoRepo;
        private IVentaRepository? _ventaRepo;
        private IDetalleVentaRepository? _detalleVentaRepo;
        private ILiberacionRepository? _liberacionRepo;
        private IDisputaRepository? _disputaRepo;
        private IConfiguracionRepository? _configuracionRepo;

        public UnitOfWork(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        // ══════════════════════════════════════════════════════════════
        // REPOSITORIOS
        // ══════════════════════════════════════════════════════════════

        public IPersonaRepository PersonaRepository
            => _personaRepo ??= new PersonaRepository(_dbContext);

        public IVendedorRepository VendedorRepository
            => _vendedorRepo ??= new VendedorRepository(_dbContext);

        public ICompradorRepository CompradorRepository
            => _compradorRepo ??= new CompradorRepository(_dbContext);

        public ITortaRepository TortaRepository
            => _tortaRepo ??= new TortaRepository(_dbContext);

        public IImagenTortaRepository ImagenesTorta
            => _imagenTortaRepo ??= new ImagenTortaRepository(_dbContext);

        public IPagoRepository PagoRepository
            => _pagoRepo ??= new PagoRepository(_dbContext);

        public IVentaRepository Ventas
            => _ventaRepo ??= new VentaRepository(_dbContext);

        public IDetalleVentaRepository DetallesVenta
            => _detalleVentaRepo ??= new DetalleVentaRepository(_dbContext);

        public ILiberacionRepository Liberaciones
            => _liberacionRepo ??= new LiberacionRepository(_dbContext);

        public IDisputaRepository Disputas
            => _disputaRepo ??= new DisputaRepository(_dbContext);

        public IConfiguracionRepository Configuracion
            => _configuracionRepo ??= new ConfiguracionRepository(_dbContext);

        // ══════════════════════════════════════════════════════════════
        // PERSISTENCIA
        // ══════════════════════════════════════════════════════════════

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error al guardar cambios en la base de datos", ex);
            }
        }

        public async Task<int> SaveAsync()
        {
            return await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesReturnBoolAsync()
        {
            try
            {
                return await SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // TRANSACCIONES
        // ══════════════════════════════════════════════════════════════

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("Ya existe una transacción activa");

            _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No hay transacción activa para confirmar");

            try
            {
                await _dbContext.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No hay transacción activa para revertir");

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // UTILIDADES
        // ══════════════════════════════════════════════════════════════

        public void DetachAllEntities()
        {
            var entries = _dbContext.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }

        public Task<bool> AnyChangesAsync()
        {
            return Task.FromResult(_dbContext.ChangeTracker.HasChanges());
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Update(entity);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Remove(entity);
        }

        public async Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            var entity = await _dbContext.FindAsync<TEntity>(keyValues);
            return entity ?? throw new InvalidOperationException($"Entidad {typeof(TEntity).Name} no encontrada");
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }

        // ══════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════

        public void Dispose()
        {
            if (_disposed) return;

            _currentTransaction?.Dispose();
            _dbContext.Dispose();
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}