using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Repositories
{
    public class DisputaRepository : IDisputaRepository
    {
        private readonly ApplicationDbContext _context;

        public DisputaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Disputa?> GetByIdAsync(int id)
            => await _context.Disputas.FindAsync(id);

        public async Task<IEnumerable<Disputa>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
            => await _context.Disputas
                .Include(d => d.Venta)
                .Include(d => d.Iniciador)
                .Include(d => d.AdminAsignado)
                .OrderByDescending(d => d.FechaCreacion)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

        public async Task AddAsync(Disputa disputa)
            => await _context.Disputas.AddAsync(disputa);

        public void Update(Disputa disputa)
            => _context.Disputas.Update(disputa);

        public async Task UpdateAsync(Disputa disputa)
        {
            _context.Disputas.Update(disputa);
            await Task.CompletedTask;
        }

        public async Task<Disputa?> GetByIdWithMensajesAsync(int id)
            => await _context.Disputas
                .Include(d => d.Venta).ThenInclude(v => v.Comprador).ThenInclude(c => c.Persona)
                .Include(d => d.Iniciador)
                .Include(d => d.AdminAsignado)
                .Include(d => d.Mensajes.OrderBy(m => m.Fecha)).ThenInclude(m => m.Autor)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<Disputa?> GetByNumeroDisputaAsync(string numeroDisputa)
            => await _context.Disputas
                .Include(d => d.Venta)
                .Include(d => d.Iniciador)
                .FirstOrDefaultAsync(d => d.NumeroDisputa == numeroDisputa);

        public async Task<IEnumerable<Disputa>> GetByVentaIdAsync(int ventaId)
            => await _context.Disputas
                .Include(d => d.Iniciador)
                .Where(d => d.VentaId == ventaId)
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Disputa>> GetByIniciadorIdAsync(int personaId)
            => await _context.Disputas
                .Include(d => d.Venta)
                .Where(d => d.IniciadorId == personaId)
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Disputa>> GetByEstadoAsync(EstadoDisputa estado)
            => await _context.Disputas
                .Include(d => d.Venta)
                .Include(d => d.Iniciador)
                .Where(d => d.Estado == estado)
                .OrderByDescending(d => d.Prioridad)
                .ThenByDescending(d => d.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Disputa>> GetAbiertasAsync()
            => await _context.Disputas
                .Include(d => d.Venta)
                .Include(d => d.Iniciador)
                .Where(d => d.EstaAbierta)
                .OrderByDescending(d => d.Prioridad)
                .ThenBy(d => d.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Disputa>> GetSinAsignarAsync()
            => await _context.Disputas
                .Include(d => d.Venta)
                .Include(d => d.Iniciador)
                .Where(d => d.EstaAbierta && d.AdminAsignadoId == null)
                .OrderByDescending(d => d.Prioridad)
                .ToListAsync();

        public async Task<int> CountAsync()
            => await _context.Disputas.CountAsync();

        public async Task<int> CountAbiertasAsync()
            => await _context.Disputas.CountAsync(d => d.EstaAbierta);

        public async Task<decimal> GetMontoTotalInvolucradoAsync()
            => await _context.Disputas
                .Where(d => d.MontoInvolucrado.HasValue)
                .SumAsync(d => d.MontoInvolucrado!.Value);

        public async Task<bool> ExistsAsync(int id)
            => await _context.Disputas.AnyAsync(d => d.Id == id);

        public async Task<bool> ExisteDisputaAbiertaParaVentaAsync(int ventaId)
            => await _context.Disputas
                .AnyAsync(d => d.VentaId == ventaId && d.EstaAbierta);

        public async Task<IEnumerable<MensajeDisputa>> GetMensajesByDisputaIdAsync(int disputaId)
            => await _context.MensajesDisputa
                .Include(m => m.Autor)
                .Where(m => m.DisputaId == disputaId)
                .OrderBy(m => m.Fecha)
                .ToListAsync();

        public async Task AddMensajeAsync(MensajeDisputa mensaje)
            => await _context.MensajesDisputa.AddAsync(mensaje);

        public async Task<string> GenerarNumeroDisputaAsync()
        {
            var fecha = DateTime.Now;
            var contador = await _context.Disputas
                .CountAsync(d => d.FechaCreacion.Year == fecha.Year && d.FechaCreacion.Month == fecha.Month);
            return $"DIS-{fecha:yyyyMM}-{(contador + 1):D4}";
        }

        public async Task AsignarAdminAsync(int disputaId, int adminId)
        {
            var disputa = await _context.Disputas.FindAsync(disputaId);
            if (disputa != null)
            {
                disputa.AdminAsignadoId = adminId;
                disputa.Estado = EstadoDisputa.EnInvestigacion;
                disputa.FechaActualizacion = DateTime.Now;
            }
        }

        public async Task CambiarEstadoAsync(int disputaId, EstadoDisputa nuevoEstado)
        {
            var disputa = await _context.Disputas.FindAsync(disputaId);
            if (disputa != null)
            {
                disputa.Estado = nuevoEstado;
                disputa.FechaActualizacion = DateTime.Now;
            }
        }

        public async Task ResolverAsync(int disputaId, ResolucionDisputa resolucion, string detalle, decimal? montoResolucion = null)
        {
            var disputa = await _context.Disputas.FindAsync(disputaId);
            if (disputa != null)
            {
                disputa.Resolucion = resolucion;
                disputa.DetalleResolucion = detalle;
                disputa.MontoResolucion = montoResolucion;
                disputa.FechaResolucion = DateTime.Now;
                disputa.FechaActualizacion = DateTime.Now;
                
                disputa.Estado = resolucion switch
                {
                    ResolucionDisputa.ReembolsoTotal or ResolucionDisputa.ReembolsoParcial => EstadoDisputa.ResueltaFavorComprador,
                    ResolucionDisputa.SinAccion => EstadoDisputa.ResueltaFavorVendedor,
                    _ => EstadoDisputa.ResueltaAcuerdo
                };
            }
        }
    }
}