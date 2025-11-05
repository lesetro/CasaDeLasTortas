using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class PagoRepository : IPagoRepository
    {
        private readonly ApplicationDbContext _context;

        public PagoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Pago?> GetByIdAsync(int id)
        {
            return await _context.Pagos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pago>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Pagos
                .OrderByDescending(p => p.FechaPago)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Pago pago)
        {
            await _context.Pagos.AddAsync(pago);
        }

        public void Update(Pago pago)
        {
            _context.Pagos.Update(pago);
        }

        public void Delete(Pago pago)
        {
            _context.Pagos.Remove(pago);
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<Pago?> GetByIdWithDetallesAsync(int id)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Vendedor)
                        .ThenInclude(v => v.Persona)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pago?> GetByIdWithTortaAsync(int id)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pago?> GetByIdWithCompradorAsync(int id)
        {
            return await _context.Pagos
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pago>> GetAllWithDetallesAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Vendedor)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .OrderByDescending(p => p.FechaPago)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        // ==================== FILTROS POR RELACIONES ====================

        public async Task<IEnumerable<Pago>> GetByCompradorIdAsync(int compradorId)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.CompradorId == compradorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetByCompradorIdWithDetailsAsync(int compradorId)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Where(p => p.CompradorId == compradorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetByTortaAsync(int tortaId)
        {
            return await _context.Pagos
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.TortaId == tortaId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetByVendedorAsync(int vendedorId)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Where(p => p.VendedorId == vendedorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        // ==================== FILTROS POR CARACTERÍSTICAS ====================

        public async Task<IEnumerable<Pago>> GetByEstadoAsync(EstadoPago estado)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetByMetodoPagoAsync(MetodoPago metodoPago)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.MetodoPago == metodoPago)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetByFechaRangoAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> SearchAsync(string termino)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p =>
                    p.Torta.Nombre.Contains(termino) ||
                    p.Comprador.Persona.Nombre.Contains(termino) ||
                    p.Vendedor.Persona.Nombre.Contains(termino) ||
                    (p.NumeroTransaccion != null && p.NumeroTransaccion.Contains(termino)) ||
                    (p.Observaciones != null && p.Observaciones.Contains(termino)))
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS GENERALES ====================

        public async Task<int> CountAsync()
        {
            return await _context.Pagos.CountAsync();
        }

        public async Task<decimal> GetTotalIngresosAsync()
        {
            return await _context.Pagos
                .Where(p => p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<decimal> GetIngresosByVendedorAsync(int vendedorId)
        {
            return await _context.Pagos
                .Where(p => p.VendedorId == vendedorId && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<decimal> GetIngresosByCompradorAsync(int compradorId)
        {
            return await _context.Pagos
                .Where(p => p.CompradorId == compradorId && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<decimal> GetIngresosByFechaAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Pagos
                .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        // ==================== ESTADÍSTICAS POR ESTADO ====================

        public async Task<int> CountByEstadoAsync(EstadoPago estado)
        {
            return await _context.Pagos
                .CountAsync(p => p.Estado == estado);
        }

        public async Task<int> CountPendientesAsync()
        {
            return await _context.Pagos
                .CountAsync(p => p.Estado == EstadoPago.Pendiente);
        }

        public async Task<int> CountCompletadosAsync()
        {
            return await _context.Pagos
                .CountAsync(p => p.Estado == EstadoPago.Completado);
        }

        public async Task<int> CountCanceladosAsync()
        {
            return await _context.Pagos
                .CountAsync(p => p.Estado == EstadoPago.Cancelado);
        }

        // ==================== ESTADÍSTICAS POR MÉTODO DE PAGO ====================

        public async Task<Dictionary<string, int>> GetCountByMetodoPagoAsync()
        {
            return await _context.Pagos
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { MetodoPago = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.MetodoPago, x => x.Count);
        }

        public async Task<Dictionary<string, decimal>> GetTotalByMetodoPagoAsync()
        {
            return await _context.Pagos
                .Where(p => p.Estado == EstadoPago.Completado)
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { MetodoPago = g.Key.ToString(), Total = g.Sum(p => p.Monto) })
                .ToDictionaryAsync(x => x.MetodoPago, x => x.Total);
        }

        // ==================== REPORTES ====================

        public async Task<IEnumerable<Pago>> GetPagosPendientesAsync()
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.Estado == EstadoPago.Pendiente)
                .OrderBy(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetPagosCompletadosAsync()
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.Estado == EstadoPago.Completado)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetUltimosPagosAsync(int cantidad = 10)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .OrderByDescending(p => p.FechaPago)
                .Take(cantidad)
                .ToListAsync();
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Pagos
                .AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Pago, bool>> predicate)
        {
            return await _context.Pagos.AnyAsync(predicate);
        }

        public async Task<bool> TieneComprobanteAsync(int pagoId)
        {
            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.Id == pagoId);

            return pago != null && !string.IsNullOrEmpty(pago.ArchivoComprobante);
        }

        // ==================== PAGOS DEL DÍA ====================

        public async Task<IEnumerable<Pago>> GetPagosDelDiaAsync(DateTime fecha)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.FechaPago >= fechaInicio && p.FechaPago < fechaFin)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<decimal> GetIngresosDelDiaAsync(DateTime fecha)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Pagos
                .Where(p => p.FechaPago >= fechaInicio && p.FechaPago < fechaFin && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        // ==================== MÉTODOS ESPECÍFICOS PARA DASHBOARD ====================

        public async Task<Dictionary<DateTime, decimal>> GetIngresosUltimos7DiasAsync()
        {
            var fechaInicio = DateTime.Now.AddDays(-7).Date;
            var fechaFin = DateTime.Now.Date.AddDays(1);

            return await _context.Pagos
                .Where(p => p.FechaPago >= fechaInicio && p.FechaPago < fechaFin && p.Estado == EstadoPago.Completado)
                .GroupBy(p => p.FechaPago.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(p => p.Monto) })
                .ToDictionaryAsync(x => x.Fecha, x => x.Total);
        }

        public async Task<IEnumerable<Pago>> GetPagosConEntregaProximaAsync()
        {
            var fechaLimite = DateTime.Now.AddDays(2);
            return await _context.Pagos
                .Include(p => p.Torta)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Where(p => p.FechaEntrega.HasValue &&
                           p.FechaEntrega.Value <= fechaLimite &&
                           p.Estado == EstadoPago.Completado)
                .OrderBy(p => p.FechaEntrega)
                .ToListAsync();
        }

        public async Task UpdateAsync(Pago pago)
        {
            _context.Pagos.Update(pago);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Pago pago)
        {
            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Pago>> GetAllWithDetailsAsync()
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                    .ThenInclude(v => v.Persona)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<decimal> GetVentasDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            var manana = hoy.AddDays(1);

            return await _context.Pagos
                .Where(p => p.FechaPago >= hoy && p.FechaPago < manana && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }

        public async Task<decimal> GetVentasDelMesAsync()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var inicioProximoMes = inicioMes.AddMonths(1);

            return await _context.Pagos
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioProximoMes && p.Estado == EstadoPago.Completado)
                .SumAsync(p => p.Monto);
        }
        public async Task<IEnumerable<Pago>> GetByVendedorIdAsync(int vendedorId)
        {
            return await _context.Pagos
                .Include(p => p.Torta)
                    .ThenInclude(t => t.Imagenes)
                .Include(p => p.Comprador)
                    .ThenInclude(c => c.Persona)
                .Include(p => p.Vendedor)
                .Where(p => p.VendedorId == vendedorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }
    }
}