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
            => await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == id);

        public async Task<IEnumerable<Vendedor>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .OrderBy(v => v.NombreComercial)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

        public async Task AddAsync(Vendedor vendedor)
            => await _context.Vendedores.AddAsync(vendedor);

        public void Update(Vendedor vendedor)
            => _context.Vendedores.Update(vendedor);

        public void Delete(Vendedor vendedor)
            => _context.Vendedores.Remove(vendedor);

        public async Task UpdateAsync(Vendedor vendedor)
        {
            _context.Vendedores.Update(vendedor);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Vendedor vendedor)
        {
            _context.Vendedores.Remove(vendedor);
            await Task.CompletedTask;
        }

        // ==================== CON RELACIONES ====================

        public async Task<Vendedor?> GetByIdWithPersonaAsync(int id)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<Vendedor?> GetByIdWithTortasAsync(int id)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Include(v => v.Tortas)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<Vendedor?> GetByIdWithDetallesCompletosAsync(int id)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Include(v => v.Tortas).ThenInclude(t => t.Imagenes)
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Venta)
                .Include(v => v.Liberaciones)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<IEnumerable<Vendedor>> GetAllWithPersonaAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .OrderBy(v => v.NombreComercial)
                .ToListAsync();

        public async Task<Vendedor?> GetByIdWithLiberacionesAsync(int id)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Include(v => v.Liberaciones.OrderByDescending(l => l.FechaCreacion).Take(10))
                .FirstOrDefaultAsync(v => v.Id == id);

        // ==================== BÚSQUEDA POR PERSONA ====================

        public async Task<Vendedor?> GetByPersonaIdAsync(int idPersona)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .FirstOrDefaultAsync(v => v.PersonaId == idPersona);

        public async Task<bool> ExistsByPersonaIdAsync(int idPersona)
            => await _context.Vendedores.AnyAsync(v => v.PersonaId == idPersona);

        public async Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync()
            => await _context.Personas
                .Include(p => p.Vendedor)
                .Where(p => p.Vendedor == null && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

        // ==================== FILTROS ====================

        public async Task<IEnumerable<Vendedor>> GetByEspecialidadAsync(string especialidad)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Especialidad != null && v.Especialidad.Contains(especialidad))
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetActivosAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetVerificadosAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Verificado)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> SearchAsync(string termino)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.NombreComercial.Contains(termino) ||
                           (v.Especialidad != null && v.Especialidad.Contains(termino)) ||
                           v.Persona.Nombre.Contains(termino))
                .ToListAsync();

        // ✅ NUEVOS: Filtros de datos de pago
        public async Task<IEnumerable<Vendedor>> GetConDatosPagoCompletosAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.DatosPagoCompletos && v.Activo)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetSinDatosPagoAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => !v.DatosPagoCompletos && v.Activo)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetConPendientesCobroAsync()
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.PendienteCobro > 0)
                .OrderByDescending(v => v.PendienteCobro)
                .ToListAsync();

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
            => await _context.Vendedores.CountAsync();

        public async Task<int> CountActivosAsync()
            => await _context.Vendedores.CountAsync(v => v.Activo);

        public async Task<int> GetCountAsync()
            => await _context.Vendedores.CountAsync(v => v.Activo);

        public async Task<decimal> GetCalificacionPromedioAsync(int idVendedor)
            => await _context.Vendedores
                .Where(v => v.Id == idVendedor)
                .Select(v => v.Calificacion)
                .FirstOrDefaultAsync();

        public async Task<int> GetTotalVentasAsync(int idVendedor)
            => await _context.Vendedores
                .Where(v => v.Id == idVendedor)
                .Select(v => v.TotalVentas)
                .FirstOrDefaultAsync();

        // ✅ NUEVOS: Estadísticas financieras
        public async Task<decimal> GetTotalCobradoAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            return vendedor?.TotalCobrado ?? 0;
        }

        public async Task<decimal> GetTotalComisionesAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            return vendedor?.TotalComisiones ?? 0;
        }

        public async Task<decimal> GetPendienteCobroAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            return vendedor?.PendienteCobro ?? 0;
        }

        public async Task<int> CountConDatosPagoAsync()
            => await _context.Vendedores.CountAsync(v => v.DatosPagoCompletos && v.Activo);

        // ==================== ORDENAMIENTO ====================

        public async Task<IEnumerable<Vendedor>> GetTopByCalificacionAsync(int cantidad = 10)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderByDescending(v => v.Calificacion)
                .Take(cantidad)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetTopByVentasAsync(int cantidad = 10)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderByDescending(v => v.TotalVentas)
                .Take(cantidad)
                .ToListAsync();

        public async Task<IEnumerable<Vendedor>> GetTopByIngresosAsync(int cantidad = 10)
            => await _context.Vendedores
                .Include(v => v.Persona)
                .Where(v => v.Activo)
                .OrderByDescending(v => v.TotalCobrado)
                .Take(cantidad)
                .ToListAsync();

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(Expression<Func<Vendedor, bool>> predicate)
            => await _context.Vendedores.AnyAsync(predicate);

        public async Task<bool> TieneDatosPagoCompletosAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            return vendedor?.TieneDatosPagoValidos() ?? false;
        }

        public async Task<bool> PuedePublicarTortasAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            return vendedor != null && vendedor.Activo && vendedor.DatosPagoCompletos;
        }

        // ==================== OPERACIONES DE DATOS DE PAGO ====================

        public async Task ActualizarDatosPagoAsync(int vendedorId, string aliasCBU, string cbu, 
            string banco, string titularCuenta, string? cuit = null, string? imagenQR = null)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            if (vendedor != null)
            {
                vendedor.AliasCBU = aliasCBU;
                vendedor.CBU = cbu;
                vendedor.Banco = banco;
                vendedor.TitularCuenta = titularCuenta;
                vendedor.CUIT = cuit;
                vendedor.ImagenQR = imagenQR;
                vendedor.ActualizarEstadoDatosPago();
            }
        }

        public async Task AgregarPendienteCobroAsync(int vendedorId, decimal monto)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            if (vendedor != null)
            {
                vendedor.PendienteCobro += monto;
            }
        }

        public async Task ProcesarLiberacionAsync(int vendedorId, decimal montoBruto, decimal comision)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            if (vendedor != null)
            {
                var montoNeto = montoBruto - comision;
                vendedor.TotalCobrado += montoNeto;
                vendedor.TotalComisiones += comision;
                vendedor.PendienteCobro -= montoBruto;
                if (vendedor.PendienteCobro < 0) vendedor.PendienteCobro = 0;
            }
        }

        public async Task<(decimal TotalCobrado, decimal TotalComisiones, decimal PendienteCobro)> GetResumenFinancieroAsync(int vendedorId)
        {
            var vendedor = await _context.Vendedores.FindAsync(vendedorId);
            if (vendedor == null)
                return (0, 0, 0);
            return (vendedor.TotalCobrado, vendedor.TotalComisiones, vendedor.PendienteCobro);
        }
    }
}