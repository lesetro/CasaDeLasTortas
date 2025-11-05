using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

namespace CasaDeLasTortas.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositorios
        private IPersonaRepository? _personaRepository;
        private IVendedorRepository? _vendedorRepository;
        private ICompradorRepository? _compradorRepository;
        private ITortaRepository? _tortaRepository;
        private IImagenTortaRepository? _imagenTortaRepository;
        private IPagoRepository? _pagoRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== PROPIEDADES DE REPOSITORIOS CORREGIDAS ====================

        // ✅ LOS NOMBRES DEBEN COINCIDIR EXACTAMENTE CON LA INTERFAZ
        public IPersonaRepository PersonaRepository
        {
            get
            {
                _personaRepository ??= new PersonaRepository(_context);
                return _personaRepository;
            }
        }

        public IVendedorRepository VendedorRepository
        {
            get
            {
                _vendedorRepository ??= new VendedorRepository(_context);
                return _vendedorRepository;
            }
        }

        public ICompradorRepository CompradorRepository
        {
            get
            {
                _compradorRepository ??= new CompradorRepository(_context);
                return _compradorRepository;
            }
        }

        public ITortaRepository TortaRepository
        {
            get
            {
                _tortaRepository ??= new TortaRepository(_context);
                return _tortaRepository;
            }
        }

        public IImagenTortaRepository ImagenesTorta
        {
            get
            {
                _imagenTortaRepository ??= new ImagenTortaRepository(_context);
                return _imagenTortaRepository;
            }
        }

        public IPagoRepository PagoRepository
        {
            get
            {
                _pagoRepository ??= new PagoRepository(_context);
                return _pagoRepository;
            }
        }

        // ==================== MÉTODOS DE TRANSACCIÓN ====================

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error al guardar los cambios en la base de datos", ex);
            }
        }

        public async Task<int> SaveAsync()
        {
            return await SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Ya existe una transacción activa");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para confirmar");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para revertir");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ==================== MÉTODOS ADICIONALES IMPLEMENTADOS ====================

        public void DetachAllEntities()
        {
            var changedEntries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in changedEntries)
            {
                entry.State = EntityState.Detached;
            }
        }

        public async Task<bool> SaveChangesReturnBoolAsync()
        {
            try
            {
                var result = await SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        // ✅ IMPLEMENTACIÓN DE LOS MÉTODOS QUE FALTABAN

        public async Task<bool> AnyChangesAsync()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Update(entity);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Remove(entity);
        }

        public async Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            return await _context.FindAsync<TEntity>(keyValues);
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>();
        }

        // ==================== DISPOSE ====================

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}