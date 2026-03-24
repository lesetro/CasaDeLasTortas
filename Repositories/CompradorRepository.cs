using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions; // ✅ AGREGAR ESTA DIRECTIVA

namespace CasaDeLasTortas.Repositories
{
    public class CompradorRepository : ICompradorRepository
    {
        private readonly ApplicationDbContext _context;

        public CompradorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Comprador?> GetByIdAsync(int id)
        {
            return await _context.Compradores
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comprador>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .OrderBy(c => c.Persona.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Comprador comprador)
        {
            await _context.Compradores.AddAsync(comprador);
        }

        public void Update(Comprador comprador)
        {
            _context.Compradores.Update(comprador);
        }

        public void Delete(Comprador comprador)
        {
            _context.Compradores.Remove(comprador);
        }

        public async Task DeleteAsync(Comprador comprador)
        {
            _context.Compradores.Remove(comprador);
            await _context.SaveChangesAsync();
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<Comprador?> GetByIdWithPersonaAsync(int id)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comprador?> GetByIdWithPagosAsync(int id)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Pagos)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comprador?> GetByIdWithDetallesCompletosAsync(int id)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Include(c => c.Ventas)  // ← Incluir ventas en lugar de pagos directos
                    .ThenInclude(v => v.Detalles)
                        .ThenInclude(d => d.Torta)
                            .ThenInclude(t => t.Imagenes)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Pagos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comprador>> GetAllWithPersonaAsync()
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }

        // ==================== BÚSQUEDA POR PERSONA ====================

        public async Task<Comprador?> GetByPersonaIdAsync(int idPersona)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .FirstOrDefaultAsync(c => c.PersonaId == idPersona);
        }

        public async Task<bool> ExistsByPersonaIdAsync(int idPersona)
        {
            return await _context.Compradores
                .AnyAsync(c => c.PersonaId == idPersona);
        }

        // ==================== FILTROS ====================

        public async Task<IEnumerable<Comprador>> GetByCiudadAsync(string ciudad)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Where(c => c.Ciudad != null && c.Ciudad.Contains(ciudad))
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comprador>> SearchAsync(string termino)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Where(c =>
                    c.Persona.Nombre.Contains(termino) ||
                    c.Persona.Email.Contains(termino) ||
                    (c.Direccion != null && c.Direccion.Contains(termino)) ||
                    (c.Ciudad != null && c.Ciudad.Contains(termino)) ||
                    (c.Telefono != null && c.Telefono.Contains(termino)))
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comprador>> GetActivosAsync()
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Where(c => c.Activo)
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.Compradores.CountAsync();
        }

        public async Task<decimal> GetTotalGastadoAsync(int idComprador)
        {
            return await _context.Pagos
                .Where(p => p.CompradorId == idComprador && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<int> GetTotalComprasAsync(int idComprador)
        {
            return await _context.Pagos
                .CountAsync(p => p.CompradorId == idComprador && p.Estado == EstadoPago.Completado);
        }

        // ==================== HISTORIAL Y TOP ====================

        public async Task<IEnumerable<Comprador>> GetTopCompradores(int cantidad = 10)
        {
            // Versión corregida usando Ventas en lugar de Pagos
            return await _context.Compradores
                .Include(c => c.Persona)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Pagos)
                .Where(c => c.Activo)
                .OrderByDescending(c => c.Ventas
                    .Where(v => v.Pagos.Any(p => p.Estado == EstadoPago.Completado))
                    .Sum(v => v.Total))
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comprador>> GetCompradorReciente(int dias = 30)
        {
            var fechaLimite = DateTime.Now.AddDays(-dias);

            return await _context.Compradores
                .Include(c => c.Persona)
                .Where(c => c.FechaCreacion >= fechaLimite)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }

        // ==================== MÉTODOS ADICIONALES ====================

        // MÉTODO IMPLEMENTADO
        public async Task<bool> ExistsAsync(Expression<Func<Comprador, bool>> predicate)
        {
            return await _context.Compradores.AnyAsync(predicate);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetTotalPaginasAsync(int registrosPorPagina = 10)
        {
            var totalRegistros = await _context.Compradores.CountAsync();
            return (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);
        }

        public async Task<IEnumerable<Comprador>> GetByProvinciaAsync(string provincia)
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Where(c => c.Provincia != null && c.Provincia.Contains(provincia))
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comprador>> GetCompradoresConPagosPendientes()
        {
            return await _context.Compradores
                .Include(c => c.Persona)
                .Include(c => c.Pagos)
                .Where(c => c.Pagos.Any(p => p.Estado == EstadoPago.Pendiente))
                .OrderBy(c => c.Persona.Nombre)
                .ToListAsync();
        }
        public async Task UpdateAsync(Comprador comprador)
        {
            _context.Compradores.Update(comprador);
            await _context.SaveChangesAsync();
        }

    }
}