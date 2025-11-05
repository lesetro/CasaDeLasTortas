using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;

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
            return Ok(tortas);
        }

        /// <summary>
        /// Filtrar tortas por categoría
        /// </summary>
        [HttpGet("categoria/{categoria}")]
        public async Task<IActionResult> GetByCategoria(string categoria)
        {
            var tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(categoria);
            return Ok(tortas);
        }

        /// <summary>
        /// Filtrar tortas por rango de precio
        /// </summary>
        [HttpGet("precio")]
        public async Task<IActionResult> GetByPrecio([FromQuery] decimal min, [FromQuery] decimal max)
        {
            var tortas = await _unitOfWork.TortaRepository.GetByPrecioRangoAsync(min, max);
            return Ok(tortas);
        }

        /// <summary>
        /// Obtener tortas disponibles
        /// </summary>
        [HttpGet("disponibles")]
        public async Task<IActionResult> GetDisponibles()
        {
            var tortas = await _unitOfWork.TortaRepository.GetDisponiblesAsync();
            return Ok(tortas);
        }

        /// <summary>
        /// Crear nueva torta
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Vendedor")]
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
                Disponible = true, // Por defecto disponible al crear
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
        [Authorize(Roles = "Admin,Vendedor")]
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
        [Authorize(Roles = "Admin,Vendedor")]
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
        [Authorize(Roles = "Admin,Vendedor")]
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
            var torta = await _unitOfWork.TortaRepository.GetByIdWithPagosAsync(id);

            if (torta == null)
                return NotFound(new { message = "Torta no encontrada" });

            var totalVentas = torta.Pagos?.Count ?? 0;
            var ingresoTotal = torta.Pagos?.Sum(p => p.Monto) ?? 0;

            return Ok(new
            {
                idTorta = torta.Id,
                nombre = torta.Nombre,
                totalVentas,
                ingresoTotal,
                precio = torta.Precio,
                disponible = torta.Disponible
            });
        }
    }
}