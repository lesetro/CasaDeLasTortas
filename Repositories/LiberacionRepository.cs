using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Repositories
{
    public class LiberacionRepository : ILiberacionRepository
    {
        private readonly ApplicationDbContext _context;

        public LiberacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LiberacionFondos?> GetByIdAsync(int id)
            => await _context.LiberacionesFondos.FindAsync(id);

        public async Task<IEnumerable<LiberacionFondos>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
            => await _context.LiberacionesFondos
                .Include(l => l.Vendedor).ThenInclude(v => v.Persona)
                .Include(l => l.Venta)
                .OrderByDescending(l => l.FechaCreacion)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

        public async Task AddAsync(LiberacionFondos liberacion)
            => await _context.LiberacionesFondos.AddAsync(liberacion);

        public void Update(LiberacionFondos liberacion)
            => _context.LiberacionesFondos.Update(liberacion);

        public async Task UpdateAsync(LiberacionFondos liberacion)
        {
            _context.LiberacionesFondos.Update(liberacion);
            await Task.CompletedTask;
        }

        public async Task<LiberacionFondos?> GetByIdWithDetallesAsync(int id)
            => await _context.LiberacionesFondos
                .Include(l => l.Vendedor).ThenInclude(v => v.Persona)
                .Include(l => l.Venta).ThenInclude(v => v.Comprador).ThenInclude(c => c.Persona)
                .Include(l => l.ProcesadoPor)
                .FirstOrDefaultAsync(l => l.Id == id);

        public async Task<IEnumerable<LiberacionFondos>> GetByVendedorIdAsync(int vendedorId)
            => await _context.LiberacionesFondos
                .Include(l => l.Venta)
                .Where(l => l.VendedorId == vendedorId)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<LiberacionFondos>> GetByVentaIdAsync(int ventaId)
            => await _context.LiberacionesFondos
                .Include(l => l.Vendedor).ThenInclude(v => v.Persona)
                .Where(l => l.VentaId == ventaId)
                .ToListAsync();

        public async Task<IEnumerable<LiberacionFondos>> GetByEstadoAsync(EstadoLiberacion estado)
            => await _context.LiberacionesFondos
                .Include(l => l.Vendedor).ThenInclude(v => v.Persona)
                .Include(l => l.Venta)
                .Where(l => l.Estado == estado)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<LiberacionFondos>> GetPendientesAsync()
            => await GetByEstadoAsync(EstadoLiberacion.Pendiente);

        public async Task<IEnumerable<LiberacionFondos>> GetListasParaLiberarAsync()
            => await _context.LiberacionesFondos
                .Include(l => l.Vendedor).ThenInclude(v => v.Persona)
                .Include(l => l.Venta)
                .Where(l => l.Estado == EstadoLiberacion.ListoParaLiberar)
                .OrderBy(l => l.FechaListoParaLiberar)
                .ToListAsync();

        public async Task<int> CountAsync()
            => await _context.LiberacionesFondos.CountAsync();

        public async Task<int> CountByEstadoAsync(EstadoLiberacion estado)
            => await _context.LiberacionesFondos.CountAsync(l => l.Estado == estado);

        public async Task<decimal> GetMontoTotalLiberadoAsync()
            => await _context.LiberacionesFondos
                .Where(l => l.Estado == EstadoLiberacion.Confirmado)
                .SumAsync(l => l.MontoNeto);

        public async Task<decimal> GetMontoPendienteLiberacionAsync()
            => await _context.LiberacionesFondos
                .Where(l => l.Estado == EstadoLiberacion.Pendiente || l.Estado == EstadoLiberacion.ListoParaLiberar)
                .SumAsync(l => l.MontoNeto);

        public async Task<decimal> GetMontoLiberadoByVendedorAsync(int vendedorId)
            => await _context.LiberacionesFondos
                .Where(l => l.VendedorId == vendedorId && l.Estado == EstadoLiberacion.Confirmado)
                .SumAsync(l => l.MontoNeto);

        public async Task<bool> ExistsAsync(int id)
            => await _context.LiberacionesFondos.AnyAsync(l => l.Id == id);

        public async Task<bool> ExistePendienteParaVentaAsync(int ventaId, int vendedorId)
            => await _context.LiberacionesFondos
                .AnyAsync(l => l.VentaId == ventaId && l.VendedorId == vendedorId &&
                          l.Estado != EstadoLiberacion.Confirmado && l.Estado != EstadoLiberacion.Cancelado);

        //  Usar firma correcta de LiberacionFondos.Crear(Venta, int, decimal, decimal)
        public async Task<IEnumerable<LiberacionFondos>> CrearLiberacionesParaVentaAsync(int ventaId, decimal porcentajeComision)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null) return new List<LiberacionFondos>();

            var montosPorVendedor = venta.Detalles
                .GroupBy(d => d.VendedorId)
                .Select(g => new { VendedorId = g.Key, Monto = g.Sum(d => d.Subtotal) });

            var liberaciones = new List<LiberacionFondos>();

            foreach (var item in montosPorVendedor)
            {
                // Firma correcta: Crear(Venta venta, int vendedorId, decimal montoBruto, decimal porcentajeComision)
                var liberacion = LiberacionFondos.Crear(venta, item.VendedorId, item.Monto, porcentajeComision);
                liberaciones.Add(liberacion);
                await _context.LiberacionesFondos.AddAsync(liberacion);
            }

            return liberaciones;
        }

        public async Task MarcarListaParaLiberarAsync(int liberacionId)
        {
            var liberacion = await _context.LiberacionesFondos.FindAsync(liberacionId);
            if (liberacion != null)
            {
                liberacion.Estado = EstadoLiberacion.ListoParaLiberar;
                liberacion.FechaListoParaLiberar = DateTime.Now;
                _context.LiberacionesFondos.Update(liberacion);
            }
        }

        //  Se agregó _context.LiberacionesFondos.Update(liberacion) para persistir los cambios
        public async Task RegistrarTransferenciaAsync(int liberacionId, string numeroOperacion, string archivoComprobante, int adminId)
        {
            var liberacion = await _context.LiberacionesFondos
                .Include(l => l.Vendedor)
                .FirstOrDefaultAsync(l => l.Id == liberacionId);

            if (liberacion != null && liberacion.Vendedor != null)
            {
                // Firma correcta: RegistrarTransferencia(int adminId, string numeroOperacion, string? archivoComprobante, Vendedor vendedor)
                liberacion.RegistrarTransferencia(adminId, numeroOperacion, archivoComprobante, liberacion.Vendedor);
                
                //  Marcar la entidad como modificada para que EF Core guarde los cambios
                _context.LiberacionesFondos.Update(liberacion);
            }
        }

        //  Se agregó Update tanto para liberación como para vendedor
        public async Task ConfirmarRecepcionAsync(int liberacionId)
        {
            var liberacion = await _context.LiberacionesFondos
                .Include(l => l.Vendedor)
                .FirstOrDefaultAsync(l => l.Id == liberacionId);

            if (liberacion != null)
            {
                liberacion.ConfirmarRecepcion();
                
                //  Marcar liberación como modificada
                _context.LiberacionesFondos.Update(liberacion);

                // Actualizar estadísticas del vendedor
                if (liberacion.Vendedor != null)
                {
                    liberacion.Vendedor.TotalCobrado += liberacion.MontoNeto;
                    liberacion.Vendedor.TotalComisiones += liberacion.Comision;
                    liberacion.Vendedor.PendienteCobro -= liberacion.MontoBruto;
                    
                    //  Marcar vendedor como modificado
                    _context.Vendedores.Update(liberacion.Vendedor);
                }
            }
        }
    }
}