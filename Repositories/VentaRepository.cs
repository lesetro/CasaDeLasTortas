using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly ApplicationDbContext _context;

        public VentaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Venta?> GetByIdAsync(int id)
        {
            return await _context.Ventas.FindAsync(id);
        }

        public async Task<IEnumerable<Venta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Ventas
                .OrderByDescending(v => v.FechaVenta)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Venta venta)
        {
            await _context.Ventas.AddAsync(venta);
        }

        public void Update(Venta venta)
        {
            _context.Ventas.Update(venta);
        }

        public async Task UpdateAsync(Venta venta)
        {
            _context.Ventas.Update(venta);
            await Task.CompletedTask;
        }

        public void Delete(Venta venta)
        {
            _context.Ventas.Remove(venta);
        }

        public async Task DeleteAsync(Venta venta)
        {
            _context.Ventas.Remove(venta);
            await Task.CompletedTask;
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<Venta?> GetByIdWithDetallesAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Torta)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venta?> GetByIdWithCompradorAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Comprador)
                    .ThenInclude(c => c.Persona)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venta?> GetByIdWithPagosAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venta?> GetByIdWithTodoAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Torta)
                        .ThenInclude(t => t.Imagenes)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Vendedor)
                        .ThenInclude(v => v.Persona)
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Venta>> GetAllWithDetailsAsync()
        {
            return await _context.Ventas
                .Include(v => v.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Torta)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Vendedor)
                .Include(v => v.Pagos)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==================== FILTROS POR COMPRADOR ====================

        public async Task<IEnumerable<Venta>> GetByCompradorIdAsync(int compradorId)
        {
            return await _context.Ventas
                .Where(v => v.CompradorId == compradorId)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetByCompradorIdWithDetailsAsync(int compradorId)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Torta)
                        .ThenInclude(t => t.Imagenes)
                .Include(v => v.Detalles)
                        .ThenInclude(d => d.Vendedor)  
                .Include(v => v.Pagos)
                .Where(v => v.CompradorId == compradorId)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==================== FILTROS POR ESTADO ====================

        public async Task<IEnumerable<Venta>> GetByEstadoAsync(EstadoVenta estado)
        {
            return await _context.Ventas
                .Where(v => v.Estado == estado)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetPendientesAsync()
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Pendiente)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetEnProcesoAsync()
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Pagada || 
                           v.Estado == EstadoVenta.EnPreparacion || 
                           v.Estado == EstadoVenta.ListaParaRetiro)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetCompletadasAsync()
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Entregada)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetCanceladasAsync()
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Cancelada || 
                           v.Estado == EstadoVenta.Reembolsada)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==================== FILTROS POR VENDEDOR ====================

        public async Task<IEnumerable<Venta>> GetByVendedorIdAsync(int vendedorId)
        {
            return await _context.Ventas
                .Where(v => v.Detalles.Any(d => d.VendedorId == vendedorId))
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetByVendedorIdWithDetailsAsync(int vendedorId)
        {
            return await _context.Ventas
                .Include(v => v.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Torta)
                        .ThenInclude(t => t.Imagenes)
                .Where(v => v.Detalles.Any(d => d.VendedorId == vendedorId))
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==================== FILTROS POR FECHA ====================

        public async Task<IEnumerable<Venta>> GetByFechaRangoAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Ventas
                .Where(v => v.FechaVenta.Date >= desde.Date && v.FechaVenta.Date <= hasta.Date)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasDelDiaAsync(DateTime fecha)
        {
            return await _context.Ventas
                .Where(v => v.FechaVenta.Date == fecha.Date)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasDelMesAsync(int año, int mes)
        {
            return await _context.Ventas
                .Where(v => v.FechaVenta.Year == año && v.FechaVenta.Month == mes)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // ==================== BÚSQUEDA ====================

        public async Task<IEnumerable<Venta>> SearchAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return new List<Venta>();

            termino = termino.ToLower();

            return await _context.Ventas
                .Include(v => v.Comprador)
                    .ThenInclude(c => c.Persona)
                .Where(v => 
                    v.NumeroOrden.ToLower().Contains(termino) ||
                    v.Comprador.Persona.Nombre.ToLower().Contains(termino) ||
                    v.Comprador.Persona.Email.ToLower().Contains(termino) ||
                    v.DireccionEntrega.ToLower().Contains(termino))
                .OrderByDescending(v => v.FechaVenta)
                .Take(50) // Limitar resultados
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.Ventas.CountAsync();
        }

        public async Task<int> CountByCompradorAsync(int compradorId)
        {
            return await _context.Ventas
                .Where(v => v.CompradorId == compradorId)
                .CountAsync();
        }

        public async Task<int> CountByVendedorAsync(int vendedorId)
        {
            return await _context.Ventas
                .Where(v => v.Detalles.Any(d => d.VendedorId == vendedorId))
                .CountAsync();
        }

        public async Task<decimal> GetTotalIngresosAsync()
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Pagada || 
                           v.Estado == EstadoVenta.Entregada)
                .SumAsync(v => v.Total);
        }

        public async Task<decimal> GetTotalIngresosByVendedorAsync(int vendedorId)
        {
            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Pagada || 
                           v.Estado == EstadoVenta.Entregada)
                .Where(v => v.Detalles.Any(d => d.VendedorId == vendedorId))
                .SumAsync(v => v.Total);
        }

        public async Task<decimal> GetTotalIngresosByCompradorAsync(int compradorId)
        {
            return await _context.Ventas
                .Where(v => v.CompradorId == compradorId)
                .Where(v => v.Estado == EstadoVenta.Pagada || 
                           v.Estado == EstadoVenta.Entregada)
                .SumAsync(v => v.Total);
        }

        public async Task<decimal> GetTotalIngresosByFechaRangoAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Ventas
                .Where(v => v.FechaVenta.Date >= desde.Date && v.FechaVenta.Date <= hasta.Date)
                .Where(v => v.Estado == EstadoVenta.Pagada || 
                           v.Estado == EstadoVenta.Entregada)
                .SumAsync(v => v.Total);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ventas.AnyAsync(v => v.Id == id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Venta, bool>> predicate)
        {
            return await _context.Ventas.AnyAsync(predicate);
        }

        // ==================== GENERAR NÚMERO DE ORDEN ====================

        public async Task<string> GenerarNumeroOrdenAsync()
        {
            var fecha = DateTime.Now;
            var prefijo = $"ORD-{fecha:yyyyMMdd}-";
            
            // Buscar la última orden del día
            var ultimaOrden = await _context.Ventas
                .Where(v => v.NumeroOrden.StartsWith(prefijo))
                .OrderByDescending(v => v.NumeroOrden)
                .FirstOrDefaultAsync();

            if (ultimaOrden == null)
            {
                return $"{prefijo}001";
            }

            // Extraer el número secuencial y aumentarlo
            var ultimoNumero = ultimaOrden.NumeroOrden.Substring(prefijo.Length);
            if (int.TryParse(ultimoNumero, out int numero))
            {
                return $"{prefijo}{(numero + 1):D3}";
            }

            return $"{prefijo}001";
        }
    }
}