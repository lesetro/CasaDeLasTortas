using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class DetalleVentaRepository : IDetalleVentaRepository
    {
        private readonly ApplicationDbContext _context;

        public DetalleVentaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<DetalleVenta?> GetByIdAsync(int id)
        {
            return await _context.DetallesVenta.FindAsync(id);
        }

        public async Task<IEnumerable<DetalleVenta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 20)
        {
            return await _context.DetallesVenta
                .OrderByDescending(d => d.Id)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(DetalleVenta detalle)
        {
            await _context.DetallesVenta.AddAsync(detalle);
        }

        public async Task AddRangeAsync(IEnumerable<DetalleVenta> detalles)
        {
            await _context.DetallesVenta.AddRangeAsync(detalles);
        }

        public void Update(DetalleVenta detalle)
        {
            _context.DetallesVenta.Update(detalle);
        }

        public async Task UpdateAsync(DetalleVenta detalle)
        {
            _context.DetallesVenta.Update(detalle);
            await Task.CompletedTask;
        }

        public void Delete(DetalleVenta detalle)
        {
            _context.DetallesVenta.Remove(detalle);
        }

        public async Task DeleteAsync(DetalleVenta detalle)
        {
            _context.DetallesVenta.Remove(detalle);
            await Task.CompletedTask;
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<DetalleVenta?> GetByIdWithTodoAsync(int id)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Comprador)
                        .ThenInclude(c => c.Persona)
                .Include(d => d.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(d => d.Vendedor)
                    .ThenInclude(v => v.Persona)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DetalleVenta>> GetByVentaIdAsync(int ventaId)
        {
            return await _context.DetallesVenta
                .Where(d => d.VentaId == ventaId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> GetByVentaIdWithDetailsAsync(int ventaId)
        {
            return await _context.DetallesVenta
                .Include(d => d.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(d => d.Vendedor)
                .Where(d => d.VentaId == ventaId)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        // ==================== FILTROS POR VENDEDOR ====================

        public async Task<IEnumerable<DetalleVenta>> GetByVendedorIdAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> GetByVendedorIdPendientesAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Torta)
                .Where(d => d.VendedorId == vendedorId && 
                           d.Estado == EstadoDetalleVenta.Pendiente)
                .OrderBy(d => d.Venta.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> GetByVendedorIdWithVentaAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Comprador)
                        .ThenInclude(c => c.Persona)
                .Include(d => d.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(d => d.Vendedor)
                .Where(d => d.VendedorId == vendedorId)
                .OrderByDescending(d => d.Venta.FechaVenta)
                .ToListAsync();
        }

        // ==================== FILTROS POR TORTA ====================

        public async Task<IEnumerable<DetalleVenta>> GetByTortaIdAsync(int tortaId)
        {
            return await _context.DetallesVenta
                .Where(d => d.TortaId == tortaId)
                .OrderByDescending(d => d.Venta.FechaVenta)
                .ToListAsync();
        }

        public async Task<int> GetTotalVendidoByTortaAsync(int tortaId)
        {
            return await _context.DetallesVenta
                .Where(d => d.TortaId == tortaId)
                .SumAsync(d => d.Cantidad);
        }

        // ==================== FILTROS POR ESTADO ====================

        public async Task<IEnumerable<DetalleVenta>> GetByEstadoAsync(EstadoDetalleVenta estado)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Torta)
                .Where(d => d.Estado == estado)
                .OrderByDescending(d => d.Venta.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> GetPendientesByVendedorAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Torta)
                .Where(d => d.VendedorId == vendedorId && 
                           d.Estado == EstadoDetalleVenta.Pendiente)
                .OrderBy(d => d.Venta.FechaVenta)
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.DetallesVenta.CountAsync();
        }

        public async Task<decimal> GetTotalVentasByVendedorAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .Where(d => d.Venta.Estado == EstadoVenta.Pagada || 
                           d.Venta.Estado == EstadoVenta.Entregada)
                .SumAsync(d => d.Subtotal);
        }

        public async Task<Dictionary<int, int>> GetVentasPorTortaAsync(int vendedorId, int limite = 10)
        {
            var ventas = await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .GroupBy(d => d.TortaId)
                .Select(g => new 
                { 
                    TortaId = g.Key, 
                    Cantidad = g.Sum(d => d.Cantidad) 
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(limite)
                .ToDictionaryAsync(x => x.TortaId, x => x.Cantidad);

            return ventas;
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DetallesVenta.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DetalleVenta, bool>> predicate)
        {
            return await _context.DetallesVenta.AnyAsync(predicate);
        }

        public async Task<bool> TieneDetallesPendientesAsync(int ventaId)
        {
            return await _context.DetallesVenta
                .AnyAsync(d => d.VentaId == ventaId && 
                              d.Estado == EstadoDetalleVenta.Pendiente);
        }

        // ==================== MÉTODOS ADICIONALES ====================

        public async Task<int> GetTotalItemsVendidosByVendedorAsync(int vendedorId)
        {
            return await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .SumAsync(d => d.Cantidad);
        }

        public async Task<decimal> GetPromedioVentaByVendedorAsync(int vendedorId)
        {
            var detalles = await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .ToListAsync();

            if (!detalles.Any())
                return 0;

            return detalles.Average(d => d.Subtotal);
        }

        public async Task<Dictionary<EstadoDetalleVenta, int>> GetConteoPorEstadoAsync(int vendedorId)
        {
            var conteo = await _context.DetallesVenta
                .Where(d => d.VendedorId == vendedorId)
                .GroupBy(d => d.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.Estado, g => g.Cantidad);

            return conteo;
        }

        public async Task<IEnumerable<DetalleVenta>> GetUltimosPedidosByVendedorAsync(int vendedorId, int cantidad = 10)
        {
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Where(d => d.VendedorId == vendedorId)
                .OrderByDescending(d => d.Venta.FechaVenta)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> GetPedidosConEntregaProximaAsync(int vendedorId, int dias = 2)
        {
            var fechaLimite = DateTime.Now.AddDays(dias);
            
            return await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Torta)
                .Where(d => d.VendedorId == vendedorId &&
                           d.FechaEstimadaPreparacion.HasValue &&
                           d.FechaEstimadaPreparacion.Value.Date <= fechaLimite.Date &&
                           d.Estado != EstadoDetalleVenta.Entregado &&
                           d.Estado != EstadoDetalleVenta.Cancelado)
                .OrderBy(d => d.FechaEstimadaPreparacion)
                .ToListAsync();
        }

        public async Task<bool> ActualizarEstadoMasivoAsync(List<int> detallesIds, EstadoDetalleVenta nuevoEstado)
        {
            var detalles = await _context.DetallesVenta
                .Where(d => detallesIds.Contains(d.Id))
                .ToListAsync();

            foreach (var detalle in detalles)
            {
                detalle.Estado = nuevoEstado;
                if (nuevoEstado == EstadoDetalleVenta.EnPreparacion)
                {
                    detalle.FechaRealPreparacion = DateTime.Now;
                }
            }

            return true;
        }
    }
}