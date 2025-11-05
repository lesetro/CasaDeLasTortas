using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VendedorApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public VendedorApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtener todos los vendedores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var vendedores = await _unitOfWork.VendedorRepository.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.VendedorRepository.CountAsync();

            return Ok(new
            {
                data = vendedores,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener vendedor por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdWithPersonaAsync(id);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(vendedor);
        }

        /// <summary>
        /// Obtener vendedor por IdPersona
        /// </summary>
        [HttpGet("persona/{idPersona}")]
        public async Task<IActionResult> GetByPersonaId(int idPersona)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByPersonaIdAsync(idPersona);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(vendedor);
        }

        /// <summary>
        /// Crear nuevo vendedor
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Create([FromBody] VendedorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que la persona existe
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(dto.PersonaId);
            if (persona == null)
                return BadRequest(new { message = "Persona no encontrada" });

            var vendedor = new Vendedor
            {
                PersonaId = dto.PersonaId,
                NombreComercial = dto.NombreComercial,
                Especialidad = dto.Especialidad,
                Calificacion = dto.Calificacion,
                Descripcion = dto.Descripcion,
                FechaCreacion = dto.FechaCreacion,
                TotalVentas = dto.TotalVentas,
                Activo = dto.Activo
            };

            await _unitOfWork.VendedorRepository.AddAsync(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vendedor.Id }, vendedor);
        }

        /// <summary>
        /// Actualizar vendedor existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Update(int id, [FromBody] VendedorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            vendedor.NombreComercial = dto.NombreComercial;
            vendedor.Especialidad = dto.Especialidad;
            vendedor.FechaCreacion = dto.FechaCreacion;
            vendedor.Calificacion = dto.Calificacion;
            vendedor.Descripcion = dto.Descripcion;
            vendedor.TotalVentas = dto.TotalVentas;
            vendedor.Activo = dto.Activo;

            _unitOfWork.VendedorRepository.Update(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(vendedor);
        }

        /// <summary>
        /// Eliminar vendedor
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            _unitOfWork.VendedorRepository.Delete(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

       
        /// <summary>
        /// Obtener tortas del vendedor
        /// </summary>
        [HttpGet("{id}/tortas")]
        public async Task<IActionResult> GetTortas(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdWithTortasAsync(id);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(vendedor.Tortas);
        }

        /// <summary>
        /// Obtener perfil del vendedor autenticado
        /// GET: /api/vendedor/mi-perfil
        /// </summary>
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> GetMiPerfil()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var vendedor = await _unitOfWork.VendedorRepository
                .GetByPersonaIdAsync(int.Parse(userId));

            if (vendedor == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            return Ok(new
            {
                id = vendedor.Id,
                personaId = vendedor.PersonaId,
                nombreComercial = vendedor.NombreComercial,
                especialidad = vendedor.Especialidad,
                descripcion = vendedor.Descripcion,
                avatar = vendedor.Persona?.Avatar,
                calificacion = vendedor.Calificacion
            });
        }

        /// <summary>
        /// Obtener estadísticas mejoradas del vendedor
        /// GET: /api/vendedor/{id}/estadisticas
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> GetEstadisticasCompletas(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);

            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            // Obtener tortas
            var tortas = await _unitOfWork.TortaRepository.GetByVendedorIdAsync(id);
            var totalTortasActivas = tortas.Count(t => t.Disponible && t.Stock > 0);

            // Obtener pagos
            var pagos = await _unitOfWork.PagoRepository.GetByVendedorIdAsync(id);
            var pagosCompletados = pagos.Where(p => p.Estado == EstadoPago.Completado);
            var pagosPendientes = pagos.Where(p => p.Estado == EstadoPago.Pendiente);

            // Estadísticas del mes actual
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var pagosMes = pagosCompletados.Where(p => p.FechaPago >= inicioMes);

            return Ok(new
            {
                totalTortas = tortas.Count(),
                totalTortasActivas = totalTortasActivas,
                totalVentas = pagosCompletados.Count(),
                ventasMes = pagosMes.Count(),
                ingresosTotales = pagosCompletados.Sum(p => p.Monto),
                ingresosMes = pagosMes.Sum(p => p.Monto),
                pedidosPendientes = pagosPendientes.Count()
            });
        }

        /// <summary>
        /// Obtener tortas más vendidas del vendedor
        /// GET: /api/vendedor/{id}/tortas-mas-vendidas?limit=5
        /// </summary>
        [HttpGet("{id}/tortas-mas-vendidas")]
        public async Task<IActionResult> GetTortasMasVendidas(int id, [FromQuery] int limit = 5)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);

            if (vendedor == null)
                return NotFound();

            var pagos = await _unitOfWork.PagoRepository.GetByVendedorIdAsync(id);

            var tortasMasVendidas = pagos
                .Where(p => p.Estado == EstadoPago.Completado)
                .GroupBy(p => new { p.TortaId, p.Torta.Nombre, p.Torta.Precio })
                .Select(g => new
                {
                    id = g.Key.TortaId,
                    nombre = g.Key.Nombre,
                    precio = g.Key.Precio,
                    totalVentas = g.Sum(p => p.Cantidad)
                })
                .OrderByDescending(t => t.totalVentas)
                .Take(limit)
                .ToList();

            return Ok(tortasMasVendidas);
        }

        /// <summary>
        /// Obtener actividad reciente del vendedor
        /// GET: /api/vendedor/{id}/actividad-reciente?limit=10
        /// </summary>
        [HttpGet("{id}/actividad-reciente")]
        public async Task<IActionResult> GetActividadReciente(int id, [FromQuery] int limit = 10)
        {
            var pagos = await _unitOfWork.PagoRepository.GetByVendedorIdAsync(id);

            var actividades = pagos
                .OrderByDescending(p => p.FechaPago)
                .Take(limit)
                .Select(p => new
                {
                    id = p.Id,
                    tipo = "pago",
                    icon = p.Estado == EstadoPago.Completado
                        ? "fas fa-check-circle text-success"
                        : "fas fa-clock text-warning",
                    titulo = p.Estado == EstadoPago.Completado
                        ? "Venta completada"
                        : "Pedido pendiente",
                    descripcion = $"{p.Comprador.Persona.Nombre} - {p.Torta.Nombre} - ${p.Monto}",
                    fecha = p.FechaPago
                })
                .ToList();

            return Ok(actividades);
        }

        
    }
}