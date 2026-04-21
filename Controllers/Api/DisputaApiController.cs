using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DisputaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DisputaApiController> _logger;

        public DisputaApiController(
            IUnitOfWork unitOfWork,
            ILogger<DisputaApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las disputas (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var disputas = await _unitOfWork.Disputas.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.Disputas.CountAsync();

            return Ok(new
            {
                data = disputas.Select(MapearDisputaResumen),
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener disputa por ID con mensajes
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var disputa = await _unitOfWork.Disputas.GetByIdWithMensajesAsync(id);
            if (disputa == null)
                return NotFound(new { message = "Disputa no encontrada" });

            return Ok(MapearDisputaDetalle(disputa));
        }

        /// <summary>
        /// Obtener disputas abiertas (Admin)
        /// </summary>
        [HttpGet("abiertas")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetAbiertas()
        {
            var disputas = await _unitOfWork.Disputas.GetAbiertasAsync();
            return Ok(disputas.Select(MapearDisputaResumen));
        }

        /// <summary>
        /// Obtener disputas sin asignar (Admin)
        /// </summary>
        [HttpGet("sin-asignar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetSinAsignar()
        {
            var disputas = await _unitOfWork.Disputas.GetSinAsignarAsync();
            return Ok(disputas.Select(MapearDisputaResumen));
        }

        /// <summary>
        /// Obtener disputas por estado (Admin)
        /// </summary>
        [HttpGet("estado/{estado}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            if (!Enum.TryParse<EstadoDisputa>(estado, true, out var estadoEnum))
                return BadRequest(new { message = "Estado inválido" });

            var disputas = await _unitOfWork.Disputas.GetByEstadoAsync(estadoEnum);
            return Ok(disputas.Select(MapearDisputaResumen));
        }

        /// <summary>
        /// Crear nueva disputa (Comprador o Vendedor)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearDisputa([FromBody] CrearDisputaRequestDTO request)
        {
            // Parsear el tipo (acepta nombre string o número)
            if (!Enum.TryParse<TipoDisputa>(request.TipoStr, ignoreCase: true, out var tipoDisputa))
                return BadRequest(new { message = $"Tipo de disputa inválido: '{request.TipoStr}'. Valores válidos: {string.Join(", ", Enum.GetNames<TipoDisputa>())}" });

            try
            {
                var personaId = await ObtenerPersonaIdActual();
                if (personaId == null)
                    return Unauthorized();

                // Verificar que la venta existe
                var venta = await _unitOfWork.Ventas.GetByIdAsync(request.VentaId);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                // Verificar que no hay disputa abierta
                if (await _unitOfWork.Disputas.ExisteDisputaAbiertaParaVentaAsync(request.VentaId))
                    return BadRequest(new { message = "Ya existe una disputa abierta para esta venta" });

                var numeroDisputa = await _unitOfWork.Disputas.GenerarNumeroDisputaAsync();

                // Título automático a partir del tipo si no se envía uno
                var titulo = request.Titulo ?? tipoDisputa switch
                {
                    TipoDisputa.ProductoNoRecibido => "Producto no recibido",
                    TipoDisputa.ProductoDaniado    => "Producto dañado",
                    TipoDisputa.ProductoDiferente  => "Producto diferente al pedido",
                    TipoDisputa.PagoNoReconocido   => "Pago no reconocido",
                    TipoDisputa.SolicitudReembolso => "Solicitud de reembolso",
                    TipoDisputa.ProblemaVendedor   => "Problema con el vendedor",
                    _                              => "Reclamo"
                };

                var disputa = new Disputa
                {
                    NumeroDisputa = numeroDisputa,
                    VentaId = request.VentaId,
                    IniciadorId = personaId.Value,
                    Tipo = tipoDisputa,
                    Titulo = titulo,
                    Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? titulo : request.Descripcion,
                    Prioridad = request.Prioridad ?? 3,
                    MontoInvolucrado = request.MontoInvolucrado ?? venta.Total,
                    Estado = EstadoDisputa.Abierta,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };

                await _unitOfWork.Disputas.AddAsync(disputa);

                // Actualizar estado de la venta
                venta.Estado = EstadoVenta.EnDisputa;
                _unitOfWork.Ventas.Update(venta);

                await _unitOfWork.SaveChangesAsync();

                // Agregar mensaje inicial
                if (!string.IsNullOrEmpty(request.MensajeInicial))
                {
                    var mensaje = new MensajeDisputa
                    {
                        DisputaId = disputa.Id,
                        AutorId = personaId.Value,
                        Contenido = request.MensajeInicial,
                        Fecha = DateTime.Now
                    };
                    await _unitOfWork.Disputas.AddMensajeAsync(mensaje);
                    await _unitOfWork.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetById), new { id = disputa.Id }, new
                {
                    success = true,
                    message = "Disputa creada correctamente",
                    numeroDisputa = disputa.NumeroDisputa,
                    id = disputa.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear disputa");
                return StatusCode(500, new { message = "Error al crear la disputa" });
            }
        }

        /// <summary>
        /// Obtener mensajes de una disputa
        /// </summary>
        [HttpGet("{id}/mensajes")]
        public async Task<IActionResult> GetMensajes(int id)
        {
            var disputa = await _unitOfWork.Disputas.GetByIdWithMensajesAsync(id);
            if (disputa == null)
                return NotFound(new { message = "Disputa no encontrada" });

            var mensajes = disputa.Mensajes?
                .OrderBy(m => m.Fecha)
                .Select(m => new
                {
                    id = m.Id,
                    autorId = m.AutorId,
                    autorNombre = m.Autor?.Nombre,
                    contenido = m.Contenido,
                    fecha = m.Fecha,
                    esInterno = m.EsInterno
                });

            return Ok(mensajes);
        }

        /// <summary>
        /// Agregar mensaje a disputa
        /// </summary>
        [HttpPost("{id}/mensajes")]
        public async Task<IActionResult> AgregarMensaje(int id, [FromBody] AgregarMensajeDTO request)
        {
            try
            {
                var personaId = await ObtenerPersonaIdActual();
                if (personaId == null)
                    return Unauthorized();

                var disputa = await _unitOfWork.Disputas.GetByIdAsync(id);
                if (disputa == null)
                    return NotFound(new { message = "Disputa no encontrada" });

                if (!disputa.EstaAbierta)
                    return BadRequest(new { message = "La disputa está cerrada" });

                var mensaje = new MensajeDisputa
                {
                    DisputaId = id,
                    AutorId = personaId.Value,
                    Contenido = request.Contenido,
                    Fecha = DateTime.Now
                };

                await _unitOfWork.Disputas.AddMensajeAsync(mensaje);
                
                disputa.FechaActualizacion = DateTime.Now;
                _unitOfWork.Disputas.Update(disputa);

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Mensaje agregado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar mensaje a disputa {Id}", id);
                return StatusCode(500, new { message = "Error al agregar el mensaje" });
            }
        }

        /// <summary>
        /// Asignar admin a disputa (Admin)
        /// </summary>
        [HttpPost("{id}/asignar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> AsignarAdmin(int id)
        {
            try
            {
                var adminId = await ObtenerPersonaIdActual();
                if (adminId == null)
                    return Unauthorized();

                await _unitOfWork.Disputas.AsignarAdminAsync(id, adminId.Value);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Disputa asignada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar admin a disputa {Id}", id);
                return StatusCode(500, new { message = "Error al asignar la disputa" });
            }
        }

        /// <summary>
        /// Cambiar estado de disputa (Admin)
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDisputaDTO request)
        {
            if (!Enum.TryParse<EstadoDisputa>(request.Estado, true, out var nuevoEstado))
                return BadRequest(new { message = "Estado inválido" });

            try
            {
                await _unitOfWork.Disputas.CambiarEstadoAsync(id, nuevoEstado);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Estado actualizado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de disputa {Id}", id);
                return StatusCode(500, new { message = "Error al cambiar el estado" });
            }
        }

        /// <summary>
        /// Resolver disputa (Admin)
        /// </summary>
        [HttpPost("{id}/resolver")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> ResolverDisputa(int id, [FromBody] ResolverDisputaDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var disputa = await _unitOfWork.Disputas.GetByIdAsync(id);
                if (disputa == null)
                    return NotFound(new { message = "Disputa no encontrada" });

                await _unitOfWork.Disputas.ResolverAsync(
                    id, request.Resolucion, request.DetalleResolucion, request.MontoResolucion);

                // Actualizar estado de la venta según resolución
                var venta = await _unitOfWork.Ventas.GetByIdAsync(disputa.VentaId);
                if (venta != null)
                {
                    if (request.Resolucion == ResolucionDisputa.ReembolsoTotal)
                    {
                        venta.Estado = EstadoVenta.Cancelada;
                        // Marcar pago para reembolso
                        var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(venta.Id);
                        var pago = pagos.FirstOrDefault();
                        if (pago != null)
                        {
                            pago.Estado = EstadoPago.ReembolsoPendiente;
                            _unitOfWork.PagoRepository.Update(pago);
                        }
                    }
                    else if (request.Resolucion == ResolucionDisputa.SinAccion)
                    {
                        venta.Estado = EstadoVenta.Pagada; // Volver a estado anterior
                    }
                    
                    _unitOfWork.Ventas.Update(venta);
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Disputa resuelta correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver disputa {Id}", id);
                return StatusCode(500, new { message = "Error al resolver la disputa" });
            }
        }

        /// <summary>
        /// Obtener mis disputas (Comprador/Vendedor)
        /// </summary>
        [HttpGet("mis-disputas")]
        public async Task<IActionResult> GetMisDisputas()
        {
            var personaId = await ObtenerPersonaIdActual();
            if (personaId == null)
                return Unauthorized();

            var disputas = await _unitOfWork.Disputas.GetByIniciadorIdAsync(personaId.Value);
            return Ok(disputas.Select(MapearDisputaResumen));
        }

        /// <summary>
        /// Obtener estadísticas de disputas (Admin)
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var total = await _unitOfWork.Disputas.CountAsync();
            var abiertas = await _unitOfWork.Disputas.CountAbiertasAsync();
            var montoInvolucrado = await _unitOfWork.Disputas.GetMontoTotalInvolucradoAsync();

            return Ok(new
            {
                totalDisputas = total,
                disputasAbiertas = abiertas,
                disputasCerradas = total - abiertas,
                montoTotalInvolucrado = montoInvolucrado
            });
        }

        // ==================== HELPERS ====================

        private object MapearDisputaResumen(Disputa d) => new
        {
            id = d.Id,
            numeroDisputa = d.NumeroDisputa,
            ventaId = d.VentaId,
            numeroOrden = d.Venta?.NumeroOrden,
            iniciadorNombre = d.Iniciador?.Nombre,
            tipo = d.Tipo.ToString(),
            prioridad = d.Prioridad.ToString(),
            estado = d.Estado.ToString(),
            montoInvolucrado = d.MontoInvolucrado,
            fechaCreacion = d.FechaCreacion,
            tieneAdminAsignado = d.AdminAsignadoId.HasValue,
            adminAsignado = d.AdminAsignado?.Nombre
        };

        private object MapearDisputaDetalle(Disputa d) => new
        {
            id = d.Id,
            numeroDisputa = d.NumeroDisputa,
            ventaId = d.VentaId,
            numeroOrden = d.Venta?.NumeroOrden,
            iniciadorId = d.IniciadorId,
            iniciadorNombre = d.Iniciador?.Nombre,
            iniciadorEmail = d.Iniciador?.Email,
            tipo = d.Tipo.ToString(),
            descripcion = d.Descripcion,
            prioridad = d.Prioridad.ToString(),
            estado = d.Estado.ToString(),
            montoInvolucrado = d.MontoInvolucrado,
            fechaCreacion = d.FechaCreacion,
            fechaActualizacion = d.FechaActualizacion,
            adminAsignadoId = d.AdminAsignadoId,
            adminAsignado = d.AdminAsignado?.Nombre,
            resolucion = d.Resolucion.ToString(),
            detalleResolucion = d.DetalleResolucion,
            montoResolucion = d.MontoResolucion,
            fechaResolucion = d.FechaResolucion,
            compradorNombre = d.Venta?.Comprador?.Persona?.Nombre,
            totalVenta = d.Venta?.Total,
            mensajes = d.Mensajes?.Select(m => new
            {
                id = m.Id,
                autorId = m.AutorId,
                autorNombre = m.Autor?.Nombre,
                contenido = m.Contenido,
                fecha = m.Fecha,
                esInterno = m.EsInterno
            }).ToList()
        };

        private Task<int?> ObtenerPersonaIdActual()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("PersonaId")?.Value;
            return Task.FromResult(int.TryParse(claim, out var id) ? id : (int?)null);
        }
    }
}