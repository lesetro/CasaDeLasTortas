using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Repositories
{
    public class ImagenTortaRepository : IImagenTortaRepository
    {
        private readonly ApplicationDbContext _context;

        public ImagenTortaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD BÁSICO ====================

        public async Task<ImagenTorta?> GetByIdAsync(int id)
        {
            return await _context.ImagenesTorta
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<ImagenTorta>> GetAllAsync(int pagina = 1, int registrosPorPagina = 20)
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .OrderBy(i => i.Orden)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task AddAsync(ImagenTorta imagenTorta)
        {
            await _context.ImagenesTorta.AddAsync(imagenTorta);
        }

        public void Update(ImagenTorta imagenTorta)
        {
            _context.ImagenesTorta.Update(imagenTorta);
        }

        public void Delete(ImagenTorta imagenTorta)
        {
            _context.ImagenesTorta.Remove(imagenTorta);
        }

        // ==================== MÉTODOS CON RELACIONES ====================

        public async Task<ImagenTorta?> GetByIdWithTortaAsync(int id)
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<ImagenTorta>> GetAllWithTortaAsync()
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .OrderBy(i => i.Orden)
                .ToListAsync();
        }

        // ==================== BÚSQUEDA POR TORTA ====================

        public async Task<IEnumerable<ImagenTorta>> GetByTortaIdAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId)
                .OrderBy(i => i.Orden)
                .ToListAsync();
        }

        public async Task<ImagenTorta?> GetImagenPrincipalAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId && i.EsPrincipal)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountByTortaIdAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .CountAsync(i => i.TortaId == tortaId);
        }

        // ==================== GESTIÓN DE IMAGEN PRINCIPAL ====================

        public async Task<bool> SetImagenPrincipalAsync(int imagenId)
        {
            var imagen = await GetByIdAsync(imagenId);
            if (imagen == null) return false;

            // Quitar principal de otras imágenes de la misma torta
            var otrasImagenes = await _context.ImagenesTorta
                .Where(i => i.TortaId == imagen.TortaId && i.Id != imagenId)
                .ToListAsync();

            foreach (var otraImagen in otrasImagenes)
            {
                otraImagen.EsPrincipal = false;
                _context.ImagenesTorta.Update(otraImagen);
            }

            // Establecer esta imagen como principal
            imagen.EsPrincipal = true;
            _context.ImagenesTorta.Update(imagen);

            return true;
        }

        public async Task RemoverPrincipalAsync(int tortaId)
        {
            var imagenPrincipal = await GetImagenPrincipalAsync(tortaId);
            if (imagenPrincipal != null)
            {
                imagenPrincipal.EsPrincipal = false;
                _context.ImagenesTorta.Update(imagenPrincipal);
            }
        }

        public async Task<bool> TieneImagenPrincipalAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .AnyAsync(i => i.TortaId == tortaId && i.EsPrincipal);
        }

        // ==================== ORDENAMIENTO ====================

        public async Task<IEnumerable<ImagenTorta>> GetByTortaIdOrderedAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId)
                .OrderBy(i => i.Orden)
                .ToListAsync();
        }

        // ==================== ESTADÍSTICAS ====================

        public async Task<int> CountAsync()
        {
            return await _context.ImagenesTorta.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ImagenesTorta.AnyAsync(i => i.Id == id);
        }

        // ==================== ELIMINAR MÚLTIPLES ====================

        public async Task DeleteByTortaIdAsync(int tortaId)
        {
            var imagenes = await GetByTortaIdAsync(tortaId);
            _context.ImagenesTorta.RemoveRange(imagenes);
        }

        // ==================== MÉTODOS ADICIONALES ====================

        public async Task<bool> ExistsAsync(Expression<Func<ImagenTorta, bool>> predicate)
        {
            return await _context.ImagenesTorta.AnyAsync(predicate);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateOrdenAsync(int imagenId, int nuevoOrden)
        {
            var imagen = await GetByIdAsync(imagenId);
            if (imagen == null) return false;

            imagen.Orden = nuevoOrden;
            _context.ImagenesTorta.Update(imagen);
            return true;
        }

        public async Task ReordenarImagenesAsync(int tortaId)
        {
            var imagenes = await GetByTortaIdAsync(tortaId);
            var orden = 0;

            foreach (var imagen in imagenes.OrderBy(i => i.Orden))
            {
                imagen.Orden = orden++;
                _context.ImagenesTorta.Update(imagen);
            }
        }

        public async Task<IEnumerable<ImagenTorta>> GetImagenesRecientesAsync(int dias = 7)
        {
            var fechaLimite = DateTime.Now.AddDays(-dias);
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .Where(i => i.FechaSubida >= fechaLimite)
                .OrderByDescending(i => i.FechaSubida)
                .ToListAsync();
        }

        public async Task<long> GetTotalTamanioArchivosAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId && i.TamanioArchivo.HasValue)
                .SumAsync(i => i.TamanioArchivo.Value);
        }

        public async Task<int> GetMaxOrdenAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId)
                .MaxAsync(i => (int?)i.Orden) ?? 0;
        }

        public async Task<IEnumerable<ImagenTorta>> GetImagenesPrincipalesAsync()
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .Where(i => i.EsPrincipal)
                .OrderBy(i => i.Torta.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImagenTorta>> GetByTipoArchivoAsync(string tipoArchivo)
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .Where(i => i.TipoArchivo != null && i.TipoArchivo.Contains(tipoArchivo))
                .OrderBy(i => i.Orden)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImagenTorta>> SearchAsync(string termino)
        {
            return await _context.ImagenesTorta
                .Include(i => i.Torta)
                .Where(i =>
                    i.NombreArchivo.Contains(termino) ||
                    (i.TipoArchivo != null && i.TipoArchivo.Contains(termino)) ||
                    i.Torta.Nombre.Contains(termino))
                .OrderBy(i => i.Orden)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImagenTorta>> GetByTortaAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .Where(i => i.TortaId == tortaId)
                .OrderBy(i => i.Orden)
                .ThenBy(i => i.FechaSubida)
                .ToListAsync();
        }

        // ✅ IMPLEMENTACIÓN DE GetPrincipalByTortaAsync
        public async Task<ImagenTorta?> GetPrincipalByTortaAsync(int tortaId)
        {
            return await _context.ImagenesTorta
                .FirstOrDefaultAsync(i => i.TortaId == tortaId && i.EsPrincipal);
        }


        
    }
}