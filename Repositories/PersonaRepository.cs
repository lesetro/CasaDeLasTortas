using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasaDeLasTortas.Repositories
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Persona?> GetByIdAsync(int id)
        {
            return await _context.Personas
                .FirstOrDefaultAsync(p => p.Id == id); // IdPersona → Id
        }

        public async Task<IEnumerable<Persona>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Personas
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Persona persona)
        {
            await _context.Personas.AddAsync(persona);
        }

        public void Update(Persona persona)
        {
            _context.Personas.Update(persona);
        }

        public void Delete(Persona persona)
        {
            _context.Personas.Remove(persona);
        }

        // ==================== BÚSQUEDA Y CONSULTAS ====================

        public async Task<Persona?> GetByEmailAsync(string email)
        {
            return await _context.Personas
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IEnumerable<Persona>> SearchAsync(string termino)
        {
            return await _context.Personas
                .Where(p =>
                    p.Nombre.Contains(termino) ||
                    p.Email.Contains(termino) ||
                    (p.Telefono != null && p.Telefono.Contains(termino)))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Persona>> GetByRolAsync(string rol)
        {
            return await _context.Personas
                .Where(p => p.Rol == rol)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Personas
                .AnyAsync(p => p.Email == email);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Personas
                .AnyAsync(p => p.Id == id); 
        }

        // ==================== ESTADÍSTICAS Y CONTEOS ====================

        public async Task<int> CountAsync()
        {
            return await _context.Personas.CountAsync();
        }

        public async Task<int> CountByRolAsync(string rol)
        {
            return await _context.Personas
                .CountAsync(p => p.Rol == rol);
        }

        // ==================== FILTROS AVANZADOS ====================

        public async Task<IEnumerable<Persona>> GetActivasAsync()
        {
            return await _context.Personas
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Persona>> GetInactivasAsync()
        {
            return await _context.Personas
                .Where(p => !p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Persona>> GetByFechaRegistroAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Personas
                .Where(p => p.FechaRegistro >= desde && p.FechaRegistro <= hasta)
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();
        }

        // ==================== MÉTODOS ADICIONALES ====================

        public async Task UpdateAsync(Persona persona)
        {
            _context.Personas.Update(persona);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Persona, bool>> predicate)
        {
            return await _context.Personas.AnyAsync(predicate);
        }
        public async Task<IEnumerable<Persona>> GetPersonasSinCompradorAsync()
        {
            return await _context.Personas
                .Where(p => p.Comprador == null && p.Rol == "Comprador")
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        public async Task DeleteAsync(Persona persona)
        {
            _context.Personas.Remove(persona);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync()
        {
            return await _context.Personas
                .Where(p => p.Vendedor == null) // Personas que no tienen vendedor asociado
                .Where(p => p.Activo) // Solo personas activas
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Personas
                .AnyAsync(p => p.Email.ToLower() == email.Trim().ToLower());
        }

        public async Task<IEnumerable<Persona>> GetAllConPerfilesAsync(
            int pagina = 1, int registrosPorPagina = 10,
            string? busqueda = null, string? rol = null, bool? activo = null)
        {
            var query = _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Apellido.Contains(busqueda) ||
                    p.Email.Contains(busqueda));

            if (!string.IsNullOrEmpty(rol))
                query = rol switch
                {
                    "Admin"     => query.Where(p => p.Rol == "Admin"),
                    "Vendedor"  => query.Where(p => p.Vendedor != null),
                    "Comprador" => query.Where(p => p.Comprador != null),
                    _           => query
                };

            if (activo.HasValue)
                query = query.Where(p => p.Activo == activo.Value);

            return await query
                .OrderByDescending(p => p.FechaRegistro)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task<int> CountConPerfilesAsync(
            string? busqueda = null, string? rol = null, bool? activo = null)
        {
            var query = _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Apellido.Contains(busqueda) ||
                    p.Email.Contains(busqueda));

            if (!string.IsNullOrEmpty(rol))
                query = rol switch
                {
                    "Admin"     => query.Where(p => p.Rol == "Admin"),
                    "Vendedor"  => query.Where(p => p.Vendedor != null),
                    "Comprador" => query.Where(p => p.Comprador != null),
                    _           => query
                };

            if (activo.HasValue)
                query = query.Where(p => p.Activo == activo.Value);

            return await query.CountAsync();
        }

    }
    
}