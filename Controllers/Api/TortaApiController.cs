using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TortaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TortaApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtener todas las tortas con paginación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var tortas = await _unitOfWork.TortaRepository.GetAllWithVendedorAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.TortaRepository.CountAsync();

            return Ok(new
            {
                data = tortas,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener torta por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            return Ok(torta);
        }

        /// <summary>
        /// Buscar tortas por término
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return BadRequest(new { message = "Término de búsqueda requerido" });

            var tortas = await _unitOfWork.TortaRepository.SearchAsync(termino);
            
            // Mapear a DTOs planos para evitar referencias circulares
            var result = tortas.Select(t => MapTortaToDto(t)).ToList();
            
            return Ok(result);
        }

        /// <summary>
        /// Filtrar tortas por categoría
        /// </summary>
        [HttpGet("categoria/{categoria}")]
        public async Task<IActionResult> GetByCategoria(string categoria)
        {
            var tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(categoria);
            var result = tortas.Select(t => MapTortaToDto(t)).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Filtrar tortas por rango de precio
        /// </summary>
        [HttpGet("precio")]
        public async Task<IActionResult> GetByPrecio([FromQuery] decimal min, [FromQuery] decimal max)
        {
            var tortas = await _unitOfWork.TortaRepository.GetByPrecioRangoAsync(min, max);
            var result = tortas.Select(t => MapTortaToDto(t)).ToList();
            return Ok(result);
        }

        /// <summary>
        /// 🔥 CORREGIDO: Obtener tortas disponibles - SIN referencias circulares
        /// </summary>
        [HttpGet("disponibles")]
        public async Task<IActionResult> GetDisponibles()
        {
            var tortas = await _unitOfWork.TortaRepository.GetDisponiblesAsync();

            // Mapear a DTOs planos para evitar referencias circulares
            var result = tortas.Select(t => MapTortaToDto(t)).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Crear nueva torta
        /// </summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Create([FromBody] TortaCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el vendedor existe
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(dto.VendedorId);
            if (vendedor == null)
                return BadRequest(new { message = "Vendedor no encontrado" });

            var torta = new Torta
            {
                VendedorId = dto.VendedorId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                Precio = dto.Precio,
                Stock = dto.Stock,
                Tamanio = dto.Tamanio,
                TiempoPreparacion = dto.TiempoPreparacion,
                Ingredientes = dto.Ingredientes,
                Personalizable = dto.Personalizable,
                Disponible = true,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.TortaRepository.AddAsync(torta);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = torta.Id }, torta);
        }

        /// <summary>
        /// Actualizar torta existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Update(int id, [FromBody] TortaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            torta.Nombre = dto.Nombre;
            torta.Descripcion = dto.Descripcion;
            torta.Categoria = dto.Categoria;
            torta.Precio = dto.Precio;
            torta.Stock = dto.Stock;
            torta.Tamanio = dto.Tamanio;
            torta.TiempoPreparacion = dto.TiempoPreparacion;
            torta.Ingredientes = dto.Ingredientes;
            torta.Personalizable = dto.Personalizable;
            torta.Disponible = dto.Disponible;
            torta.FechaActualizacion = DateTime.Now;

            _unitOfWork.TortaRepository.Update(torta);
            await _unitOfWork.SaveChangesAsync();

            return Ok(torta);
        }

        /// <summary>
        /// Cambiar disponibilidad de torta
        /// </summary>
        [HttpPatch("{id}/disponibilidad")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> CambiarDisponibilidad(int id, [FromBody] bool disponible)
        {
            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            torta.Disponible = disponible;
            torta.FechaActualizacion = DateTime.Now;

            _unitOfWork.TortaRepository.Update(torta);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Disponibilidad actualizada", disponible = torta.Disponible });
        }

        /// <summary>
        /// Eliminar torta
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Delete(int id)
        {
            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            _unitOfWork.TortaRepository.Delete(torta);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtener imágenes de la torta
        /// </summary>
        [HttpGet("{id}/imagenes")]
        public async Task<IActionResult> GetImagenes(int id)
        {
            var torta = await _unitOfWork.TortaRepository.GetByIdWithImagenesAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            return Ok(torta.Imagenes);
        }

        /// <summary>
        /// Obtener estadísticas de la torta
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> GetEstadisticas(int id)
        {
            var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            var detallesCompletados = torta.DetallesVenta?
                .Where(d => d.Venta.Pagos.Any(p => p.Estado == EstadoPago.Completado))
                .ToList() ?? new List<DetalleVenta>();

            var totalVentas = detallesCompletados.Sum(d => d.Cantidad);
            var ingresoTotal = detallesCompletados.Sum(d => d.Subtotal);

            return Ok(new
            {
                idTorta = torta.Id,
                nombre = torta.Nombre,
                totalVentas,
                ingresoTotal,
                precio = torta.Precio,
                stock = torta.Stock,
                disponible = torta.Disponible,
                vecesVendida = torta.VecesVendida
            });
        }

        /// <summary>
        /// Catálogo para el dashboard del comprador
        /// </summary>
        [HttpGet("catalogo")]
        public async Task<IActionResult> GetCatalogo(
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 12,
            [FromQuery] string? categoria = null,
            [FromQuery] string? busqueda = null)
        {
            IEnumerable<Torta> tortas;

            if (!string.IsNullOrWhiteSpace(busqueda))
                tortas = await _unitOfWork.TortaRepository.SearchAsync(busqueda);
            else if (!string.IsNullOrWhiteSpace(categoria))
                tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(categoria);
            else
                tortas = await _unitOfWork.TortaRepository.GetDisponiblesAsync();

            // Solo tortas disponibles con stock
            tortas = tortas.Where(t => t.Disponible && t.Stock > 0);

            var total = tortas.Count();

            var paginadas = tortas
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(t => MapTortaToDto(t))
                .ToList();

            return Ok(new
            {
                data = paginadas,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Detalle de una torta para el dashboard Vue
        /// </summary>
        [HttpGet("catalogo/{id}")]
        public async Task<IActionResult> GetCatalogoDetalle(int id)
        {
            var t = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
            if (t == null)
                return NotFound(new { message = "Torta no encontrada" });

            return Ok(new
            {
                id = t.Id,
                nombre = t.Nombre,
                descripcion = t.Descripcion ?? "",
                ingredientes = t.Ingredientes ?? "",
                precio = t.Precio,
                stock = t.Stock,
                categoria = t.Categoria ?? "",
                tamanio = t.Tamanio ?? "",
                tiempoPreparacion = t.TiempoPreparacion,
                personalizable = t.Personalizable,
                calificacion = t.Calificacion,
                vecesVendida = t.VecesVendida,
                disponible = t.Disponible,
                vendedorId = t.Vendedor?.Id ?? 0,
                nombreVendedor = t.Vendedor?.NombreComercial ?? "Sin vendedor",
                especialidadVendedor = t.Vendedor?.Especialidad ?? "",
                calificacionVendedor = t.Vendedor?.Calificacion ?? 0,
                imagenes = (t.Imagenes ?? new List<ImagenTorta>())
                    .OrderBy(i => i.Orden)
                    .Select(i => new { url = i.UrlImagen, esPrincipal = i.EsPrincipal })
                    .ToList(),
                imagenPrincipal = t.Imagenes?
                    .Where(i => i.EsPrincipal)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault()
                ?? t.Imagenes?
                    .OrderBy(i => i.Orden)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault()
            });
        }

        /// <summary>
        /// Categorías disponibles
        /// </summary>
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _unitOfWork.TortaRepository.GetCategoriasAsync();
            return Ok(categorias);
        }

        // ═══════════════════════════════════════════════════════════════
        // 🔥 MÉTODO HELPER: Mapear Torta a DTO plano (evita referencias circulares)
        // ═══════════════════════════════════════════════════════════════
        private object MapTortaToDto(Torta t)
        {
            return new
            {
                id = t.Id,
                nombre = t.Nombre,
                descripcion = t.Descripcion ?? "",
                precio = t.Precio,
                stock = t.Stock,
                categoria = t.Categoria ?? "",
                tamanio = t.Tamanio ?? "",
                tiempoPreparacion = t.TiempoPreparacion,
                personalizable = t.Personalizable,
                calificacion = t.Calificacion,
                vecesVendida = t.VecesVendida,
                disponible = t.Disponible,
                vendedorId = t.Vendedor?.Id ?? 0,
                nombreVendedor = t.Vendedor?.NombreComercial ?? "Sin vendedor",
                // Imágenes como array plano
                imagenes = (t.Imagenes ?? new List<ImagenTorta>())
                    .OrderBy(i => i.Orden)
                    .Select(i => new
                    {
                        id = i.Id,
                        urlImagen = i.UrlImagen,
                        esPrincipal = i.EsPrincipal,
                        orden = i.Orden
                    })
                    .ToList(),
                // Imagen principal directa
                imagenPrincipal = t.Imagenes?
                    .Where(i => i.EsPrincipal)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault()
                    ?? t.Imagenes?
                        .OrderBy(i => i.Orden)
                        .Select(i => i.UrlImagen)
                        .FirstOrDefault()
            };
        }
    }
}