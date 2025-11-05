using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class VendedorRepository : IVendedorRepository
    {
        private readonly ApplicationDbContext _context;

        public VendedorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Vendedor?> GetByIdAsync(int id)
        {
            return await _context.Vendedores
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Vendedor>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .OrderBy(v => v.NombreComercial)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Vendedor vendedor)
        {
            await _context.Vendedores.AddAsync(vendedor);
        }

        public void Update(Vendedor vendedor)
        {
            _context.Vendedores.Update(vendedor);
        }

        public void Delete(Vendedor vendedor)
        {
            _context.Vendedores.Remove(vendedor);
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<Vendedor?> GetByIdWithPersonaAsync(int id)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vendedor?> GetByIdWithTortasAsync(int id)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Include(v => v.Tortas)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vendedor?> GetByIdWithDetallesCompletosAsync(int id)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Include(v => v.Tortas)
                .Include(v => v.PagosRecibidos)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Vendedor>> GetAllWithPersonaAsync()
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();
        }

        // ==================== BÚSQUEDA POR PERSONA ====================

        public async Task<Vendedor?> GetByPersonaIdAsync(int idPersona)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .FirstOrDefaultAsync(v => v.PersonaId == idPersona);
        }

        public async Task<bool> ExistsByPersonaIdAsync(int idPersona)
        {
            return await _context.Vendedores
                .AnyAsync(v => v.PersonaId == idPersona);
        }

        // ==================== FILTROS ====================

        public async Task<IEnumerable<Vendedor>> GetByEspecialidadAsync(string especialidad)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Especialidad != null && v.Especialidad.Contains(especialidad))
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendedor>> GetActivosAsync()
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendedor>> GetVerificadosAsync()
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Verificado)
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendedor>> SearchAsync(string termino)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => 
                    v.NombreComercial.Contains(termino) ||
                    (v.Especialidad != null && v.Especialidad.Contains(termino)) ||
                    (v.Descripcion != null && v.Descripcion.Contains(termino)) ||
                    v.Persona.Nombre.Contains(termino) ||
                    v.Persona.Email.Contains(termino))
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.Vendedores.CountAsync();
        }

        public async Task<int> CountActivosAsync()
        {
            return await _context.Vendedores
                .CountAsync(v => v.Activo);
        }

        public async Task<decimal> GetCalificacionPromedioAsync(int idVendedor)
        {
            return await _context.Vendedores
                .Where(v => v.Id == idVendedor)
                .Select(v => v.Calificacion)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalVentasAsync(int idVendedor)
        {
            return await _context.Vendedores
                .Where(v => v.Id == idVendedor)
                .Select(v => v.TotalVentas)
                .FirstOrDefaultAsync();
        }

        // ==================== ORDENAMIENTO ====================

        public async Task<IEnumerable<Vendedor>> GetTopByCalificacionAsync(int cantidad = 10)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderByDescending(v => v.Calificacion)
                .ThenBy(v => v.NombreComercial)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendedor>> GetTopByVentasAsync(int cantidad = 10)
        {
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderByDescending(v => v.TotalVentas)
                .ThenBy(v => v.NombreComercial)
                .Take(cantidad)
                .ToListAsync();
        }

        // ==================== MÉTODOS ADICIONALES ====================
        public async Task UpdateAsync(Vendedor vendedor)
        {
            _context.Vendedores.Update(vendedor);
            // No llamamos SaveChangesAsync aquí para permitir transacciones en UnitOfWork
            await Task.CompletedTask; // Para cumplir con la firma async
        }
        // ✅ MÉTODO IMPLEMENTADO
        public async Task<bool> ExistsAsync(Expression<Func<Vendedor, bool>> predicate)
        {
            return await _context.Vendedores.AnyAsync(predicate);
        }

        // ✅ MÉTODO PARA GUARDAR CAMBIOS (Útil para operaciones que requieren persistencia inmediata)
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // ✅ MÉTODO PARA OBTENER VENDEDORES RECIENTES
        public async Task<IEnumerable<Vendedor>> GetVendedoresRecientesAsync(int dias = 30)
        {
            var fechaLimite = DateTime.Now.AddDays(-dias);
            return await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.FechaCreacion >= fechaLimite)
                .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();
        }
        public async Task DeleteAsync(Vendedor vendedor)
        {
            _context.Vendedores.Remove(vendedor);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync()
        {
            return await _context.Personas
                .Include(p => p.Vendedor) // Incluir la relación con Vendedor para poder verificar
                .Where(p => p.Vendedor == null) // Personas que no tienen vendedor asociado
                .Where(p => p.Activo) // Solo personas activas
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        public async Task<int> GetCountAsync()
        {
            return await _context.Vendedores
                .Where(v => v.Activo)
                .CountAsync();
        }
    }
}