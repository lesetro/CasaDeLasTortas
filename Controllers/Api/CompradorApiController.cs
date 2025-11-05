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
    public class CompradorApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompradorApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtener todos los compradores
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var compradores = await _unitOfWork.CompradorRepository.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.CompradorRepository.CountAsync();

            return Ok(new
            {
                data = compradores,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener comprador por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            return Ok(comprador);
        }

        /// <summary>
        /// Obtener comprador por IdPersona
        /// </summary>
        [HttpGet("persona/{personaId}")]
        public async Task<IActionResult> GetByPersonaId(int personaId)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(personaId);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            return Ok(comprador);
        }

        /// <summary>
        /// Crear nuevo comprador
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompradorCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que la persona existe
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(dto.PersonaId);
            if (persona == null)
                return BadRequest(new { message = "Persona no encontrada" });

            // Verificar que la persona no sea ya un comprador
            var compradorExistente = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(dto.PersonaId);
            if (compradorExistente != null)
                return BadRequest(new { message = "Esta persona ya está registrada como comprador" });

            var comprador = new Comprador
            {
                PersonaId = dto.PersonaId,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Ciudad = dto.Ciudad,
                Provincia = dto.Provincia,
                CodigoPostal = dto.CodigoPostal,
                FechaNacimiento = dto.FechaNacimiento,
                Preferencias = dto.Preferencias,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            await _unitOfWork.CompradorRepository.AddAsync(comprador);
            await _unitOfWork.SaveChangesAsync();

            // Obtener el comprador creado con información de persona
            var compradorCreado = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(comprador.Id);

            return CreatedAtAction(nameof(GetById), new { id = comprador.Id }, compradorCreado);
        }

        /// <summary>
        /// Actualizar comprador existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompradorUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            comprador.Direccion = dto.Direccion;
            comprador.Telefono = dto.Telefono;
            comprador.Ciudad = dto.Ciudad;
            comprador.Provincia = dto.Provincia;
            comprador.CodigoPostal = dto.CodigoPostal;
            comprador.FechaNacimiento = dto.FechaNacimiento;
            comprador.Preferencias = dto.Preferencias;
            comprador.Activo = dto.Activo;

            _unitOfWork.CompradorRepository.Update(comprador);
            await _unitOfWork.SaveChangesAsync();

            var compradorActualizado = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);
            return Ok(compradorActualizado);
        }

        /// <summary>
        /// Eliminar comprador (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            // Soft delete - marcar como inactivo
            comprador.Activo = false;
            _unitOfWork.CompradorRepository.Update(comprador);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtener pagos del comprador
        /// </summary>
        [HttpGet("{id}/pagos")]
        public async Task<IActionResult> GetPagos(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPagosAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            var pagosDTO = comprador.Pagos?.Select(p => new PagoListDTO
            {
                Id = p.Id,
                NombreTorta = p.Torta?.Nombre ?? "Torta no disponible",
                NombreComprador = comprador.Persona?.Nombre ?? "Comprador no disponible",
                NombreVendedor = p.Vendedor?.Persona?.Nombre ?? "Vendedor no disponible",
                Cantidad = p.Cantidad,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                Estado = p.Estado,
                MetodoPago = p.MetodoPago,
                TieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante),
                ImagenTorta = p.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
            }).ToList() ?? new List<PagoListDTO>();

            return Ok(pagosDTO);
        }

        /// <summary>
        /// Obtener estadísticas del comprador
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> GetEstadisticas(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPagosAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            var pagosCompletados = comprador.Pagos?.Where(p => p.Estado == EstadoPago.Completado).ToList() ?? new List<Pago>();
            var totalCompras = pagosCompletados.Count;
            var totalGastado = pagosCompletados.Sum(p => p.Monto);
            var compraPromedio = totalCompras > 0 ? totalGastado / totalCompras : 0;

            // Compras del último mes
            var ultimoMes = DateTime.Now.AddMonths(-1);
            var comprasUltimoMes = pagosCompletados.Count(p => p.FechaPago >= ultimoMes);
            var gastoUltimoMes = pagosCompletados.Where(p => p.FechaPago >= ultimoMes).Sum(p => p.Monto);

            return Ok(new
            {
                idComprador = comprador.Id,
                nombrePersona = comprador.Persona?.Nombre ?? "N/A",
                email = comprador.Persona?.Email ?? "N/A",
                totalCompras,
                totalGastado,
                compraPromedio,
                comprasUltimoMes,
                gastoUltimoMes,
                fechaPrimeraCompra = pagosCompletados.Any() ? pagosCompletados.Min(p => p.FechaPago) : (DateTime?)null,
                fechaUltimaCompra = pagosCompletados.Any() ? pagosCompletados.Max(p => p.FechaPago) : (DateTime?)null
            });
        }

        /// <summary>
        /// Obtener historial de compras del comprador
        /// </summary>
        [HttpGet("{id}/historial")]
        public async Task<IActionResult> GetHistorial(int id, [FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPagosAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            var pagos = comprador.Pagos?
                .OrderByDescending(p => p.FechaPago)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new PagoHistorialDTO
                {
                    Id = p.Id,
                    FechaPago = p.FechaPago,
                    NombreTorta = p.Torta?.Nombre ?? "Torta no disponible",
                    NombreVendedor = p.Vendedor?.Persona?.Nombre ?? "Vendedor no disponible",
                    Cantidad = p.Cantidad,
                    Monto = p.Monto,
                    Estado = p.Estado,
                    EstadoTexto = p.Estado.ToString(),
                    ImagenTorta = p.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    FechaEntrega = p.FechaEntrega,
                    PuedeCalificar = p.Estado == EstadoPago.Completado && p.FechaEntrega.HasValue && p.FechaEntrega.Value.AddDays(7) >= DateTime.Now
                })
                .ToList() ?? new List<PagoHistorialDTO>();

            var totalPagos = comprador.Pagos?.Count ?? 0;

            return Ok(new
            {
                data = pagos,
                pagina,
                registrosPorPagina,
                totalRegistros = totalPagos,
                totalPaginas = (int)Math.Ceiling((double)totalPagos / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener perfil completo del comprador
        /// </summary>
        [HttpGet("{id}/perfil")]
        public async Task<IActionResult> GetPerfil(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPagosAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            var pagosCompletados = comprador.Pagos?.Where(p => p.Estado == EstadoPago.Completado).ToList() ?? new List<Pago>();
            var totalGastado = pagosCompletados.Sum(p => p.Monto);

            var historialCompras = pagosCompletados
                .OrderByDescending(p => p.FechaPago)
                .Take(10) // Últimas 10 compras
                .Select(p => new PagoHistorialDTO
                {
                    Id = p.Id,
                    FechaPago = p.FechaPago,
                    NombreTorta = p.Torta?.Nombre ?? "Torta no disponible",
                    NombreVendedor = p.Vendedor?.Persona?.Nombre ?? "Vendedor no disponible",
                    Cantidad = p.Cantidad,
                    Monto = p.Monto,
                    Estado = p.Estado,
                    EstadoTexto = p.Estado.ToString(),
                    ImagenTorta = p.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    FechaEntrega = p.FechaEntrega,
                    PuedeCalificar = p.Estado == EstadoPago.Completado && p.FechaEntrega.HasValue && p.FechaEntrega.Value.AddDays(7) >= DateTime.Now
                })
                .ToList();

            var perfil = new CompradorPerfilDTO
            {
                Id = comprador.Id,
                NombrePersona = comprador.Persona?.Nombre ?? "N/A",
                Email = comprador.Persona?.Email ?? "N/A",
                Telefono = comprador.Telefono,
                DireccionCompleta = $"{comprador.Direccion}, {comprador.Ciudad}, {comprador.Provincia}",
                TotalCompras = comprador.TotalCompras,
                TotalGastado = totalGastado,
                HistorialCompras = historialCompras
            };

            return Ok(perfil);
        }
    }
}