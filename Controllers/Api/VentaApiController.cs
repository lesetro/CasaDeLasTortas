using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    /// <summary>
    /// API completa de Ventas.
    /// Permite a cualquier cliente (Vue, Android, iOS) consultar
    /// y crear ventas usando JWT.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VentaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICarritoService _carritoService;
        private readonly ILogger<VentaApiController> _logger;

        public VentaApiController(
            IUnitOfWork unitOfWork,
            ICarritoService carritoService,
            ILogger<VentaApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _carritoService = carritoService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // GET /api/Venta/{id}
        // Detalle completo de una venta (comprador o admin)
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(id);

                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                // Verificar que el comprador solo vea sus propias ventas
                var compradorId = await ObtenerCompradorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && venta.CompradorId != compradorId)
                    return Forbid();

                return Ok(MapVentaDetalle(venta));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo venta {Id}", id);
                return StatusCode(500, new { message = "Error al obtener la venta" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/Venta/mis-compras?pagina=1&tamanioPagina=10&estado=Pendiente
        // Historial del comprador autenticado con filtros y paginación
        // ─────────────────────────────────────────────
        [HttpGet("mis-compras")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Comprador,Admin")]
        public async Task<IActionResult> MisCompras(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 10,
            [FromQuery] string? estado = null)
        {
            try
            {
                var compradorId = await ObtenerCompradorId();
                if (compradorId == null)
                    return Unauthorized(new { message = "No se encontró el perfil de comprador" });

                var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(compradorId.Value);

                // Filtrar por estado si se especifica
                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoVenta>(estado, out var estadoEnum))
                    ventas = ventas.Where(v => v.Estado == estadoEnum);

                // Ordenar por fecha descendente
                ventas = ventas.OrderByDescending(v => v.FechaVenta);

                var totalRegistros = ventas.Count();
                var ventasPaginadas = ventas
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                return Ok(new
                {
                    ventas = ventasPaginadas.Select(MapVentaResumen),
                    pagina,
                    tamanioPagina,
                    totalRegistros,
                    totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanioPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mis compras");
                return StatusCode(500, new { message = "Error al obtener el historial" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/Venta/comprador/{compradorId}
        // Todas las ventas de un comprador (admin o el mismo comprador)
        // ─────────────────────────────────────────────
        [HttpGet("comprador/{compradorId}")]
        public async Task<IActionResult> GetByComprador(int compradorId)
        {
            try
            {
                // Solo el propio comprador o admin puede ver
                var miCompradorId = await ObtenerCompradorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && miCompradorId != compradorId)
                    return Forbid();

                var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(compradorId);
                var ordenadas = ventas.OrderByDescending(v => v.FechaVenta);

                return Ok(new
                {
                    data = ordenadas.Select(MapVentaResumen),
                    totalRegistros = ordenadas.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ventas del comprador {Id}", compradorId);
                return StatusCode(500, new { message = "Error al obtener las ventas" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Venta/crear-desde-carrito
        // Convierte el carrito actual en una venta
        // Body: { "direccionEntrega": "...", "notas": "..." }
        // ─────────────────────────────────────────────
        [HttpPost("crear-desde-carrito")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Comprador,Admin")]
        public async Task<IActionResult> CrearDesdeCarrito([FromBody] CrearVentaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DireccionEntrega))
                return BadRequest(new { message = "La dirección de entrega es requerida" });

            try
            {
                var compradorId = await ObtenerCompradorId();
                if (compradorId == null)
                    return Unauthorized(new { message = "No se encontró el perfil de comprador" });

                // Verificar que el carrito no esté vacío
                var totalItems = _carritoService.GetTotalItems();
                if (totalItems == 0)
                    return BadRequest(new { message = "El carrito está vacío" });

                // Crear la venta desde el carrito
                var venta = await _carritoService.ConvertirAVenta(
                    compradorId.Value,
                    request.DireccionEntrega,
                    request.Notas
                );

                if (venta == null)
                    return BadRequest(new { message = "No se pudo crear la venta. Verificá el stock disponible." });

                return CreatedAtAction(nameof(GetById), new { id = venta.Id }, new
                {
                    success = true,
                    message = "Venta creada exitosamente",
                    ventaId = venta.Id,
                    numeroOrden = venta.NumeroOrden,
                    total = venta.Total,
                    estado = venta.Estado.ToString()
                });
            }
            catch (InvalidOperationException ex)
            {
                // Errores de negocio (stock insuficiente, etc.)
                _logger.LogWarning(ex, "Error de validación al crear venta");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando venta desde carrito");
                return StatusCode(500, new { message = "Error al crear la venta" });
            }
        }

        // ─────────────────────────────────────────────
        // PATCH /api/Venta/{id}/cancelar
        // El comprador cancela su propia venta (solo si está Pendiente)
        // ─────────────────────────────────────────────
        [HttpPatch("{id}/cancelar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Comprador,Admin")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarVentaRequest? request = null)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdAsync(id);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                // Verificar que sea su propia venta
                var compradorId = await ObtenerCompradorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && venta.CompradorId != compradorId)
                    return Forbid();

                // Solo se puede cancelar si está Pendiente o Pagada
                if (venta.Estado != EstadoVenta.Pendiente && venta.Estado != EstadoVenta.Pagada)
                    return BadRequest(new { message = $"No se puede cancelar una venta en estado '{venta.Estado}'" });

                venta.Estado = EstadoVenta.Cancelada;
                venta.NotasInternas = request?.Motivo ?? "Cancelada por el comprador";
                venta.FechaActualizacion = DateTime.Now;

                _unitOfWork.Ventas.Update(venta);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Venta cancelada exitosamente",
                    estado = venta.Estado.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando venta {Id}", id);
                return StatusCode(500, new { message = "Error al cancelar la venta" });
            }
        }
        [HttpPost("{id}/confirmar-recepcion")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Comprador")]
        public async Task<IActionResult> ConfirmarRecepcion(int id)
        {
            try
            {
                var compradorId = await ObtenerCompradorId();
                if (compradorId == null)
                    return Unauthorized(new { message = "No se encontró el perfil de comprador" });

                var venta = await _unitOfWork.Ventas.GetByIdAsync(id);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                if (venta.CompradorId != compradorId)
                    return Forbid();

                if (venta.Estado != EstadoVenta.Entregada)
                    return BadRequest(new { message = "La venta aún no está marcada como entregada" });

                // Obtener liberaciones y actualizarlas DIRECTAMENTE
                var liberaciones = await _unitOfWork.Liberaciones.GetByVentaIdAsync(id);
                var actualizadas = 0;

                foreach (var lib in liberaciones)
                {
                    if (lib.Estado == EstadoLiberacion.Pendiente)
                    {
                        // Modificar directamente la entidad que ya tenemos
                        lib.Estado = EstadoLiberacion.ListoParaLiberar;
                        lib.FechaListoParaLiberar = DateTime.Now;
                        _unitOfWork.Liberaciones.Update(lib);  // Marcar como modificada
                        actualizadas++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Comprador {CompradorId} confirmó recepción de venta {VentaId}. {Count} liberaciones actualizadas.",
                    compradorId, id, actualizadas);

                return Ok(new
                {
                    success = true,
                    message = $"Recepción confirmada. {actualizadas} liberación(es) lista(s) para procesar.",
                    liberacionesActualizadas = actualizadas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando recepción de venta {Id}", id);
                return StatusCode(500, new { message = "Error al confirmar la recepción" });
            }
        }

        // ─────────────────────────────────────────────
        // Helpers privados
        // ─────────────────────────────────────────────
        private async Task<int?> ObtenerCompradorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("PersonaId")?.Value;

            if (!int.TryParse(userIdClaim, out int personaId)) return null;

            var comprador = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(personaId);
            return comprador?.Id;
        }

        private object MapVentaResumen(Venta v) => new
        {
            id = v.Id,
            numeroOrden = v.NumeroOrden,
            fechaVenta = v.FechaVenta,
            estado = v.Estado.ToString(),
            subtotal = v.Subtotal,
            descuentoTotal = v.DescuentoTotal,
            total = v.Total,
            totalItems = v.Detalles?.Sum(d => d.Cantidad) ?? 0,
            primerProducto = v.Detalles?.FirstOrDefault()?.Torta?.Nombre,
            imagenPrincipal = v.Detalles?.FirstOrDefault()?.Torta?.Imagenes?
                               .FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
        };

        private object MapVentaDetalle(Venta v) => new
        {
            id = v.Id,
            numeroOrden = v.NumeroOrden,
            fechaVenta = v.FechaVenta,
            estado = v.Estado.ToString(),
            subtotal = v.Subtotal,
            descuentoTotal = v.DescuentoTotal,
            total = v.Total,
            direccionEntrega = v.DireccionEntrega,
            ciudad = v.Ciudad,
            provincia = v.Provincia,
            codigoPostal = v.CodigoPostal,
            notasCliente = v.NotasCliente,
            fechaEntregaEstimada = v.FechaEntregaEstimada,
            fechaEntregaReal = v.FechaEntregaReal,
            detalles = v.Detalles?.Select(d => new
            {
                id = d.Id,
                tortaId = d.TortaId,
                nombreTorta = d.Torta?.Nombre,
                imagenTorta = d.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                nombreVendedor = d.Vendedor?.NombreComercial,
                cantidad = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                descuento = d.Descuento,
                subtotal = d.Subtotal,
                estado = d.Estado.ToString(),
                notasPersonalizacion = d.NotasPersonalizacion
            }),
            pagos = v.Pagos?.Select(p => new
            {
                id = p.Id,
                monto = p.Monto,
                estado = p.Estado.ToString(),
                metodoPago = p.MetodoPago?.ToString(),
                fechaPago = p.FechaPago
            })
        };
    }

}