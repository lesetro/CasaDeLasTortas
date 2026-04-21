using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
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
            => await _context.Pagos.FindAsync(id);

        public async Task<IEnumerable<Pago>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
            => await _context.Pagos
                .OrderByDescending(p => p.FechaPago)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

        public async Task AddAsync(Pago pago)
            => await _context.Pagos.AddAsync(pago);

        public void Update(Pago pago)
            => _context.Pagos.Update(pago);

        public async Task UpdateAsync(Pago pago)
        {
            _context.Pagos.Update(pago);
            await Task.CompletedTask;
        }

        public void Delete(Pago pago)
            => _context.Pagos.Remove(pago);

        public async Task DeleteAsync(Pago pago)
        {
            _context.Pagos.Remove(pago);
            await Task.CompletedTask;
        }

        // ==================== CON RELACIONES ====================

        public async Task<Pago?> GetByIdWithDetallesAsync(int id)
            => await _context.Pagos
                .Include(p => p.Venta).ThenInclude(v => v.Detalles).ThenInclude(d => d.Torta)
                .Include(p => p.Venta).ThenInclude(v => v.Comprador).ThenInclude(c => c.Persona)
                .Include(p => p.VerificadoPor)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Pago?> GetByIdWithVentaAsync(int id)
            => await _context.Pagos
                .Include(p => p.Venta)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Pago?> GetByIdWithCompradorAsync(int id)
            => await _context.Pagos
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Pago>> GetAllWithDetailsAsync()
            => await _context.Pagos
                .Include(p => p.Venta).ThenInclude(v => v.Detalles).ThenInclude(d => d.Vendedor)
                .Include(p => p.Venta).ThenInclude(v => v.Detalles).ThenInclude(d => d.Torta)
                .Include(p => p.Venta).ThenInclude(v => v.Comprador).ThenInclude(c => c.Persona)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Include(p => p.VerificadoPor)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<Pago?> GetByIdWithVerificadorAsync(int id)
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Include(p => p.VerificadoPor)
                .FirstOrDefaultAsync(p => p.Id == id);

        // ==================== FILTROS POR RELACIONES ====================

        public async Task<IEnumerable<Pago>> GetByCompradorIdAsync(int compradorId)
            => await _context.Pagos
                .Where(p => p.CompradorId == compradorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByCompradorIdWithDetailsAsync(int compradorId)
            => await _context.Pagos
                .Include(p => p.Venta).ThenInclude(v => v.Detalles).ThenInclude(d => d.Torta)
                .Where(p => p.CompradorId == compradorId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByVentaIdAsync(int ventaId)
            => await _context.Pagos
                .Where(p => p.VentaId == ventaId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByVendedorAsync(int vendedorId)
            => await _context.Pagos
                .Where(p => p.Venta.Detalles.Any(d => d.VendedorId == vendedorId))
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByVendedorIdAsync(int vendedorId)
            => await GetByVendedorAsync(vendedorId);

        // ==================== FILTROS POR CARACTERÍSTICAS ====================

        public async Task<IEnumerable<Pago>> GetByEstadoAsync(EstadoPago estado)
            => await _context.Pagos
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByMetodoPagoAsync(MetodoPago metodoPago)
            => await _context.Pagos
                .Where(p => p.MetodoPago == metodoPago)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetByFechaRangoAsync(DateTime desde, DateTime hasta)
            => await _context.Pagos
                .Where(p => p.FechaPago.Date >= desde.Date && p.FechaPago.Date <= hasta.Date)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> SearchAsync(string termino)
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Venta.NumeroOrden.Contains(termino) ||
                           (p.NumeroTransaccion != null && p.NumeroTransaccion.Contains(termino)) ||
                           p.Comprador.Persona.Nombre.Contains(termino))
                .Take(50)
                .ToListAsync();

        // Filtros para Admin
        public async Task<IEnumerable<Pago>> GetPendientesVerificacionAsync()
            => await _context.Pagos
                .Include(p => p.Venta).ThenInclude(v => v.Detalles).ThenInclude(d => d.Vendedor)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Estado == EstadoPago.EnRevision)
                .OrderBy(p => p.FechaComprobante)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetEnRevisionAsync()
            => await GetByEstadoAsync(EstadoPago.EnRevision);

        public async Task<IEnumerable<Pago>> GetRechazadosAsync()
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Estado == EstadoPago.Rechazado)
                .OrderByDescending(p => p.FechaRechazo)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetReembolsosPendientesAsync()
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Estado == EstadoPago.ReembolsoPendiente)
                .OrderBy(p => p.FechaPago)
                .ToListAsync();

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
            => await _context.Pagos.CountAsync();

        public async Task<decimal> GetTotalIngresosAsync()
            => await _context.Pagos
                .Where(p => p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado)
                .SumAsync(p => p.Monto);

        public async Task<decimal> GetIngresosByVendedorAsync(int vendedorId)
            => await _context.Pagos
                .Where(p => (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado) &&
                           p.Venta.Detalles.Any(d => d.VendedorId == vendedorId))
                .SumAsync(p => p.Monto);

        public async Task<decimal> GetIngresosByCompradorAsync(int compradorId)
            => await _context.Pagos
                .Where(p => p.CompradorId == compradorId && 
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                .SumAsync(p => p.Monto);

        public async Task<decimal> GetIngresosByFechaAsync(DateTime desde, DateTime hasta)
            => await _context.Pagos
                .Where(p => p.FechaPago.Date >= desde.Date && p.FechaPago.Date <= hasta.Date &&
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                .SumAsync(p => p.Monto);

        // Estadísticas de comisiones
        public async Task<decimal> GetTotalComisionesAsync()
            => await _context.Pagos
                .Where(p => p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado)
                .SumAsync(p => p.ComisionPlataforma);

        public async Task<decimal> GetComisionesDelMesAsync()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Pagos
                .Where(p => p.FechaPago >= inicioMes &&
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                .SumAsync(p => p.ComisionPlataforma);
        }

        public async Task<decimal> GetMontoEnRevisionAsync()
            => await _context.Pagos
                .Where(p => p.Estado == EstadoPago.EnRevision)
                .SumAsync(p => p.Monto);

        // ==================== ESTADÍSTICAS POR ESTADO ====================

        public async Task<int> CountByEstadoAsync(EstadoPago estado)
            => await _context.Pagos.CountAsync(p => p.Estado == estado);

        public async Task<int> CountPendientesAsync()
            => await CountByEstadoAsync(EstadoPago.Pendiente);

        public async Task<int> CountCompletadosAsync()
            => await CountByEstadoAsync(EstadoPago.Completado);

        public async Task<int> CountCanceladosAsync()
            => await CountByEstadoAsync(EstadoPago.Cancelado);

        public async Task<int> CountEnRevisionAsync()
            => await CountByEstadoAsync(EstadoPago.EnRevision);

        public async Task<int> CountRechazadosAsync()
            => await CountByEstadoAsync(EstadoPago.Rechazado);

        // ==================== ESTADÍSTICAS POR MÉTODO ====================

        public async Task<Dictionary<MetodoPago, int>> GetCountByMetodoPagoAsync()
            => await _context.Pagos
                .Where(p => p.MetodoPago.HasValue)
                .GroupBy(p => p.MetodoPago!.Value)
                .Select(g => new { Metodo = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Metodo, g => g.Count);

        public async Task<Dictionary<MetodoPago, decimal>> GetTotalByMetodoPagoAsync()
            => await _context.Pagos
                .Where(p => p.MetodoPago.HasValue && 
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                .GroupBy(p => p.MetodoPago!.Value)
                .Select(g => new { Metodo = g.Key, Total = g.Sum(p => p.Monto) })
                .ToDictionaryAsync(g => g.Metodo, g => g.Total);

        // ==================== REPORTES ====================

        public async Task<IEnumerable<Pago>> GetPagosPendientesAsync()
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Estado == EstadoPago.Pendiente)
                .OrderBy(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetPagosCompletadosAsync()
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .Where(p => p.Estado == EstadoPago.Completado)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetUltimosPagosAsync(int cantidad = 10)
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.Comprador).ThenInclude(c => c.Persona)
                .OrderByDescending(p => p.FechaPago)
                .Take(cantidad)
                .ToListAsync();

        public async Task<IEnumerable<Pago>> GetPagosVerificadosHoyAsync()
            => await _context.Pagos
                .Include(p => p.Venta)
                .Include(p => p.VerificadoPor)
                .Where(p => p.FechaVerificacion.HasValue && 
                           p.FechaVerificacion.Value.Date == DateTime.Today)
                .OrderByDescending(p => p.FechaVerificacion)
                .ToListAsync();

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
            => await _context.Pagos.AnyAsync(p => p.Id == id);

        public async Task<bool> ExistsAsync(Expression<Func<Pago, bool>> predicate)
            => await _context.Pagos.AnyAsync(predicate);

        public async Task<bool> TieneComprobanteAsync(int pagoId)
        {
            var pago = await _context.Pagos.FindAsync(pagoId);
            return pago != null && !string.IsNullOrEmpty(pago.ArchivoComprobante);
        }

        public async Task<bool> PuedeSerVerificadoAsync(int pagoId)
        {
            var pago = await _context.Pagos.FindAsync(pagoId);
            return pago != null && pago.Estado == EstadoPago.EnRevision && 
                   !string.IsNullOrEmpty(pago.ArchivoComprobante);
        }

        public async Task<bool> PuedeSubirComprobanteAsync(int pagoId)
        {
            var pago = await _context.Pagos.FindAsync(pagoId);
            if (pago == null) return false;
            return pago.Estado == EstadoPago.Pendiente || pago.Estado == EstadoPago.Rechazado;
        }

        public async Task<int> GetIntentosRechazadosAsync(int pagoId)
        {
            var pago = await _context.Pagos.FindAsync(pagoId);
            return pago?.IntentosRechazados ?? 0;
        }

        // ==================== PAGOS DEL DÍA ====================

        public async Task<IEnumerable<Pago>> GetPagosDelDiaAsync(DateTime fecha)
            => await _context.Pagos
                .Where(p => p.FechaPago.Date == fecha.Date)
                .OrderBy(p => p.FechaPago)
                .ToListAsync();

        public async Task<decimal> GetIngresosDelDiaAsync(DateTime fecha)
            => await _context.Pagos
                .Where(p => p.FechaPago.Date == fecha.Date && 
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                .SumAsync(p => p.Monto);

        public async Task<decimal> GetVentasDelDiaAsync()
            => await GetIngresosDelDiaAsync(DateTime.Today);

        public async Task<decimal> GetVentasDelMesAsync()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            return await GetIngresosByFechaAsync(inicioMes, finMes);
        }

        // ==================== DASHBOARD ====================

        public async Task<Dictionary<DateTime, decimal>> GetIngresosUltimos7DiasAsync()
        {
            var fechaInicio = DateTime.Now.AddDays(-6).Date;
            var ingresos = new Dictionary<DateTime, decimal>();

            for (int i = 0; i < 7; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                ingresos[fecha] = await GetIngresosDelDiaAsync(fecha);
            }
            return ingresos;
        }

        public async Task<IEnumerable<Pago>> GetPagosConEntregaProximaAsync()
        {
            var fechaLimite = DateTime.Now.AddDays(3);
            return await _context.Pagos
                .Include(p => p.Venta).ThenInclude(v => v.Detalles)
                .Where(p => p.Venta.FechaEntregaEstimada.HasValue &&
                           p.Venta.FechaEntregaEstimada.Value.Date <= fechaLimite.Date &&
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado) &&
                           p.Venta.Estado != EstadoVenta.Entregada)
                .OrderBy(p => p.Venta.FechaEntregaEstimada)
                .ToListAsync();
        }

        //  Cálculo en memoria para evitar ambigüedad de EF.Functions
        public async Task<double> GetTiempoPromedioVerificacionAsync()
        {
            var pagosVerificados = await _context.Pagos
                .Where(p => p.FechaVerificacion.HasValue && p.FechaComprobante.HasValue)
                .Select(p => new { 
                    FechaComprobante = p.FechaComprobante!.Value,
                    FechaVerificacion = p.FechaVerificacion!.Value
                })
                .ToListAsync();

            if (!pagosVerificados.Any()) return 0;

            // Calcular diferencia en memoria (evita ambigüedad MySQL/SQL Server)
            return pagosVerificados.Average(p => 
                (p.FechaVerificacion - p.FechaComprobante).TotalHours);
        }
    }
}