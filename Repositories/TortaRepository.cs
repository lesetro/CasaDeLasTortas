using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class TortaRepository : ITortaRepository
    {
        private readonly ApplicationDbContext _context;

        public TortaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<Torta?> GetByIdAsync(int id)
        {
            return await _context.Tortas
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Torta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            return await _context.Tortas
                .OrderBy(t => t.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(Torta torta)
        {
            await _context.Tortas.AddAsync(torta);
        }

        public void Update(Torta torta)
        {
            _context.Tortas.Update(torta);
        }

        public async Task UpdateAsync(Torta torta)
        {
            _context.Tortas.Update(torta);
            await _context.SaveChangesAsync();
        }

        public void Delete(Torta torta)
        {
            _context.Tortas.Remove(torta);
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<Torta?> GetByIdWithVendedorAsync(int id)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Torta?> GetByIdWithImagenesAsync(int id)
        {
            return await _context.Tortas
                .Include(t => t.Imagenes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Torta?> GetByIdWithDetallesCompletosAsync(int id)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .Include(t => t.DetallesVenta)  
                    .ThenInclude(d => d.Venta)   // Incluir la venta asociada
                .FirstOrDefaultAsync(t => t.Id == id);
        }

       public async Task<IEnumerable<Torta>> GetAllWithVendedorAsync()
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)          
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetAllWithImagenesAsync()
        {
            return await _context.Tortas
                .Include(t => t.Imagenes)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        // ==================== BÚSQUEDA POR VENDEDOR ====================

        public async Task<IEnumerable<Torta>> GetByVendedorIdAsync(int idVendedor)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.VendedorId == idVendedor)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<int> CountByVendedorIdAsync(int idVendedor)
        {
            return await _context.Tortas
                .CountAsync(t => t.VendedorId == idVendedor);
        }

        // ==================== FILTROS ====================

        public async Task<IEnumerable<Torta>> GetByCategoriaAsync(string categoria)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Categoria != null && t.Categoria.Contains(categoria))
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetByPrecioRangeAsync(decimal precioMin, decimal precioMax)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Precio >= precioMin && t.Precio <= precioMax)
                .OrderBy(t => t.Precio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetDisponiblesAsync()
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Stock > 0)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> SearchAsync(string termino)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t =>
                    t.Nombre.Contains(termino) ||
                    (t.Descripcion != null && t.Descripcion.Contains(termino)) ||
                    (t.Categoria != null && t.Categoria.Contains(termino)) ||
                    (t.Ingredientes != null && t.Ingredientes.Contains(termino)) ||
                    t.Vendedor.NombreComercial.Contains(termino))
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.Tortas.CountAsync();
        }

        public async Task<int> CountDisponiblesAsync()
        {
            return await _context.Tortas
                .CountAsync(t => t.Disponible && t.Stock > 0);
        }

        public async Task<decimal> GetPrecioPromedioAsync()
        {
            return await _context.Tortas
                .Where(t => t.Disponible)
                .AverageAsync(t => t.Precio);
        }

        // ==================== ORDENAMIENTO ====================

        public async Task<IEnumerable<Torta>> GetTopVendidasAsync(int cantidad = 10)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible)
                .OrderByDescending(t => t.VecesVendida)
                .ThenBy(t => t.Nombre)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetTopCalificadasAsync(int cantidad = 10)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Calificacion > 0)
                .OrderByDescending(t => t.Calificacion)
                .ThenBy(t => t.Nombre)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetRecientesAsync(int cantidad = 10)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible)
                .OrderByDescending(t => t.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();
        }

        // ==================== MÉTODOS ADICIONALES ====================

        public async Task<bool> ExistsAsync(Expression<Func<Torta, bool>> predicate)
        {
            return await _context.Tortas.AnyAsync(predicate);
        }

        // ==================== MÉTODOS  PARA EL HOME CONTROLLER ====================

        public async Task<IEnumerable<Torta>> GetTortasDisponiblesConDetallesAsync()
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Stock > 0)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriasAsync()
        {
            return await _context.Tortas
                .Where(t => t.Categoria != null && t.Disponible)
                .Select(t => t.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetTortasDestacadasAsync(int cantidad)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Stock > 0)
                .OrderByDescending(t => t.VecesVendida)
                .ThenByDescending(t => t.Calificacion)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Torta>> GetTortasPorCategoriasAsync(List<string?> categorias, int cantidad)
{
    if (categorias == null || !categorias.Any() || categorias.All(c => string.IsNullOrEmpty(c)))
    {
        return new List<Torta>();
    }

    var categoriasValidas = categorias.Where(c => !string.IsNullOrEmpty(c)).Select(c => c!);
    
    return await _context.Tortas
        .Include(t => t.Vendedor)
        .Include(t => t.Imagenes)
        .Where(t => t.Disponible && t.Stock > 0 && categoriasValidas.Contains(t.Categoria))
        .OrderByDescending(t => t.FechaCreacion)
        .Take(cantidad)
        .ToListAsync();
}

        public async Task<IEnumerable<Torta>> GetTortasNuevasHoyAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Stock > 0 && t.FechaCreacion.Date == hoy)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }
        
        public async Task<Torta?> GetByIdWithVentasAsync(int id)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Include(t => t.DetallesVenta)
                    .ThenInclude(d => d.Venta)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // ==================== MÉTODOS PARA EL PAGO CONTROLLER ====================

        public async Task<IEnumerable<Torta>> GetTortasDisponiblesAsync()
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible && t.Stock > 0)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }
        public async Task<Torta?> GetByIdWithDetallesAsync(int id)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
      public async Task<Torta?> GetByIdWithPagosAsync(int id)  
{
    // Los pagos ahora están en Venta a través de DetallesVenta
    return await _context.Tortas
        .Include(t => t.Vendedor)
        .Include(t => t.Imagenes)
        .Include(t => t.DetallesVenta)
            .ThenInclude(d => d.Venta)
                .ThenInclude(v => v.Pagos)
        .FirstOrDefaultAsync(t => t.Id == id);
}
        public async Task<IEnumerable<Torta>> GetByPrecioRangoAsync(decimal min, decimal max)
        {
            return await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible &&
                           t.Precio >= min &&
                           t.Precio <= max)
                .OrderBy(t => t.Precio)
                .ToListAsync();
        }
        public async Task<IEnumerable<TortaListDTO>> GetAllWithVendedorAsync(int pagina = 1, int registrosPorPagina = 10)
        {
            var skip = (pagina - 1) * registrosPorPagina;

            var tortas = await _context.Tortas
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Persona)
                .Include(t => t.Imagenes)
                .Where(t => t.Disponible)
                .OrderByDescending(t => t.FechaCreacion)
                .Skip(skip)
                .Take(registrosPorPagina)
                .Select(t => new TortaListDTO
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Precio = t.Precio,
                    Stock = t.Stock,
                    Categoria = t.Categoria,
                    Calificacion = t.Calificacion,
                    Disponible = t.Disponible,
                    NombreVendedor = t.Vendedor.NombreComercial,
                    ImagenPrincipal = t.Imagenes
                        .Where(i => i.EsPrincipal)
                        .Select(i => i.UrlImagen)
                        .FirstOrDefault() ?? t.Imagenes
                            .OrderBy(i => i.Orden)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return tortas;
        }

    }
}