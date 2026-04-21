using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using CasaDeLasTortas.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    /// <summary>
    /// API de DetalleVenta para el vendedor.
    /// Permite consultar y gestionar el estado de sus pedidos
    /// desde cualquier cliente usando JWT.
    /// </summary>
    [Route("api/DetalleVenta")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
    public class DetalleVentaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILiberacionService _liberacionService;
        private readonly ILogger<DetalleVentaApiController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DetalleVentaApiController(
            IUnitOfWork unitOfWork,
            ILiberacionService liberacionService,
            ILogger<DetalleVentaApiController> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _liberacionService = liberacionService;
            _logger = logger;
            _hubContext = hubContext;
        }

        // ─────────────────────────────────────────────
        // GET /api/DetalleVenta/vendedor/{vendedorId}
        // Todos los pedidos de un vendedor con filtros opcionales
        // ?estado=0&pagina=1&tamanioPagina=20
        // ─────────────────────────────────────────────
        [HttpGet("vendedor/{vendedorId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
        public async Task<IActionResult> GetByVendedor(
            int vendedorId,
            [FromQuery] int? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 20)
        {
            try
            {
                // Verificar que el vendedor solo vea sus propios pedidos
                var miVendedorId = await ObtenerVendedorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && miVendedorId != vendedorId)
                    return Forbid();

                var detalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(vendedorId);

                // Filtrar por estado si se especifica
                if (estado.HasValue && Enum.IsDefined(typeof(EstadoDetalleVenta), estado.Value))
                {
                    var estadoEnum = (EstadoDetalleVenta)estado.Value;
                    detalles = detalles.Where(d => d.Estado == estadoEnum);
                }

                // Ordenar por fecha de venta descendente
                detalles = detalles.OrderByDescending(d => d.Venta.FechaVenta);

                var totalRegistros = detalles.Count();
                var detallesPaginados = detalles
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                return Ok(new
                {
                    data = detallesPaginados.Select(MapDetalleVendedor),
                    pagina,
                    tamanioPagina,
                    totalRegistros,
                    totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanioPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pedidos del vendedor {Id}", vendedorId);
                return StatusCode(500, new { message = "Error al obtener los pedidos" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/DetalleVenta/mis-pedidos
        // Pedidos del vendedor autenticado
        // ─────────────────────────────────────────────
        [HttpGet("mis-pedidos")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
        public async Task<IActionResult> MisPedidos(
            [FromQuery] int? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 20)
        {
            try
            {
                var vendedorId = await ObtenerVendedorId();
                if (vendedorId == null)
                    return Unauthorized(new { message = "No se encontró el perfil de vendedor" });

                var detalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(vendedorId.Value);

                if (estado.HasValue && Enum.IsDefined(typeof(EstadoDetalleVenta), estado.Value))
                {
                    var estadoEnum = (EstadoDetalleVenta)estado.Value;
                    detalles = detalles.Where(d => d.Estado == estadoEnum);
                }

                detalles = detalles.OrderByDescending(d => d.Venta.FechaVenta);

                var totalRegistros = detalles.Count();
                var detallesPaginados = detalles
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                return Ok(new
                {
                    data = detallesPaginados.Select(MapDetalleVendedor),
                    pagina,
                    tamanioPagina,
                    totalRegistros,
                    totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanioPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mis pedidos");
                return StatusCode(500, new { message = "Error al obtener los pedidos" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/DetalleVenta/{id}
        // Detalle individual de un pedido
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var detalle = await _unitOfWork.DetallesVenta.GetByIdWithTodoAsync(id);
                if (detalle == null)
                    return NotFound(new { message = "Pedido no encontrado" });

                // Solo el vendedor dueño o admin puede verlo
                var vendedorId = await ObtenerVendedorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && detalle.VendedorId != vendedorId)
                    return Forbid();

                return Ok(MapDetalleVendedor(detalle));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el pedido" });
            }
        }

        // ─────────────────────────────────────────────
        // PATCH /api/DetalleVenta/{id}/estado
        // El vendedor actualiza el estado de un pedido
        // Body: { "estado": 2, "notas": "Empezamos la preparación" }
        // Estados: 0=Pendiente 1=Confirmado 2=EnPreparacion 3=Listo 4=Entregado 5=Cancelado
        // ─────────────────────────────────────────────
        [HttpPatch("{id}/estado")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest request)
        {
            // Acepta nombre del enum ("Confirmado") o número ("1")
            if (!Enum.TryParse<EstadoDetalleVenta>(request.Estado, ignoreCase: true, out var estadoNuevo))
                return BadRequest(new { message = "Estado inválido. Use el nombre (ej: 'Confirmado') o número (ej: 1)" });

            try
            {
                var detalle = await _unitOfWork.DetallesVenta.GetByIdWithTodoAsync(id);
                if (detalle == null)
                    return NotFound(new { message = "Pedido no encontrado" });

                var vendedorId = await ObtenerVendedorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && detalle.VendedorId != vendedorId)
                    return Forbid();

                if (!EsTransicionValida(detalle.Estado, estadoNuevo) && rol != "Admin")
                    return BadRequest(new
                    {
                        message = $"No se puede cambiar de '{detalle.Estado}' a '{estadoNuevo}'"
                    });

                // El vendedor solo puede actuar sobre pedidos con pago aprobado por el admin.
                // Se verifica por el estado de la venta (más confiable que cargar la colección Pagos).
                if (rol != "Admin")
                {
                    var ventaEstado = detalle.Venta?.Estado;
                    var pagoAprobado = ventaEstado == EstadoVenta.Pagada
                        || ventaEstado == EstadoVenta.EnPreparacion
                        || ventaEstado == EstadoVenta.ListaParaRetiro
                        || ventaEstado == EstadoVenta.Entregada;
                    if (!pagoAprobado)
                        return BadRequest(new { message = "No se puede gestionar el pedido hasta que el admin verifique el pago." });
                }

                var estadoAnterior = detalle.Estado;
                detalle.Estado = estadoNuevo;

                if (!string.IsNullOrEmpty(request.Notas))
                    detalle.NotasPersonalizacion = request.Notas;

                if (estadoNuevo == EstadoDetalleVenta.Listo)
                    detalle.FechaRealPreparacion = DateTime.Now;

                _unitOfWork.DetallesVenta.Update(detalle);

                // Actualizar estado de la venta según el progreso agregado de sus detalles
                {
                    var todosLosDetalles = (await _unitOfWork.DetallesVenta.GetByVentaIdAsync(detalle.VentaId)).ToList();
                    var activos = todosLosDetalles.Where(d => d.Estado != EstadoDetalleVenta.Cancelado).ToList();
                    var venta = await _unitOfWork.Ventas.GetByIdAsync(detalle.VentaId);

                    if (venta != null && activos.Any())
                    {
                        if (activos.All(d => d.Estado == EstadoDetalleVenta.Entregado)
                            && venta.Estado != EstadoVenta.Entregada)
                        {
                            venta.Estado = EstadoVenta.Entregada;
                            venta.FechaEntregaReal = DateTime.Now;
                            _unitOfWork.Ventas.Update(venta);
                            _logger.LogInformation("Venta {VentaId} marcada como Entregada automáticamente", detalle.VentaId);

                            var liberaciones = await _unitOfWork.Liberaciones.GetByVentaIdAsync(detalle.VentaId);
                            foreach (var lib in liberaciones.Where(l => l.Estado == EstadoLiberacion.Pendiente))
                                await _unitOfWork.Liberaciones.MarcarListaParaLiberarAsync(lib.Id);
                        }
                        else if (activos.All(d => d.Estado >= EstadoDetalleVenta.Listo)
                            && (venta.Estado == EstadoVenta.EnPreparacion || venta.Estado == EstadoVenta.Pagada))
                        {
                            venta.Estado = EstadoVenta.ListaParaRetiro;
                            _unitOfWork.Ventas.Update(venta);
                            _logger.LogInformation("Venta {VentaId} marcada como ListaParaRetiro automáticamente", detalle.VentaId);
                        }
                        else if (activos.Any(d => d.Estado >= EstadoDetalleVenta.Confirmado)
                            && venta.Estado == EstadoVenta.Pagada)
                        {
                            venta.Estado = EstadoVenta.EnPreparacion;
                            _unitOfWork.Ventas.Update(venta);
                            _logger.LogInformation("Venta {VentaId} marcada como EnPreparacion automáticamente", detalle.VentaId);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Notificar al comprador cuando un ítem pasa a Listo para retirar
                if (estadoNuevo == EstadoDetalleVenta.Listo)
                {
                    var compradorPersonaId = detalle.Venta?.Comprador?.PersonaId.ToString();
                    if (!string.IsNullOrEmpty(compradorPersonaId))
                    {
                        var nombreVendedor  = detalle.Vendedor?.NombreComercial ?? "el vendedor";
                        var direccionRetiro = detalle.Vendedor?.Persona?.Direccion;
                        var horarioRetiro   = detalle.Vendedor?.Horario;
                        var numeroOrden     = detalle.Venta?.NumeroOrden;

                        await _hubContext.Clients.User(compradorPersonaId).SendAsync("PedidoListo", new
                        {
                            tipo            = "pedido_listo",
                            detalleId       = detalle.Id,
                            ventaId         = detalle.VentaId,
                            numeroOrden,
                            nombreTorta     = detalle.Torta?.Nombre,
                            nombreVendedor,
                            direccionRetiro,
                            horarioRetiro,
                            mensaje         = $"¡Tu pedido #{numeroOrden} está listo para retirar en {nombreVendedor}!",
                            timestamp       = DateTime.UtcNow
                        });

                        _logger.LogInformation(
                            "Notificación PedidoListo enviada al comprador {CompradorId} — Orden #{Orden} — Vendedor: {Vendedor}",
                            compradorPersonaId, numeroOrden, nombreVendedor);
                    }
                }

                _logger.LogInformation(
                    "Detalle {Id} actualizado de {Anterior} a {Nuevo} por vendedor {VendedorId}",
                    id, estadoAnterior, estadoNuevo, vendedorId
                );

                return Ok(new
                {
                    success = true,
                    message = $"Estado actualizado a '{estadoNuevo}'",
                    estadoAnterior = estadoAnterior.ToString(),
                    estadoNuevo = estadoNuevo.ToString(),
                    accionesDisponibles = GetAccionesDisponibles(estadoNuevo),
                    detalle = MapDetalleVendedor(detalle)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado del detalle {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el estado" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/DetalleVenta/vendedor/{vendedorId}/pendientes
        // Solo los pedidos pendientes del vendedor (para el badge de notificación)
        // ─────────────────────────────────────────────
        [HttpGet("vendedor/{vendedorId}/pendientes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
        public async Task<IActionResult> GetPendientes(int vendedorId)
        {
            try
            {
                var miVendedorId = await ObtenerVendedorId();
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol != "Admin" && miVendedorId != vendedorId)
                    return Forbid();

                var pendientes = await _unitOfWork.DetallesVenta.GetByVendedorIdPendientesAsync(vendedorId);

                return Ok(new
                {
                    total = pendientes.Count(),
                    pedidos = pendientes.Select(MapDetalleVendedor)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pendientes del vendedor {Id}", vendedorId);
                return StatusCode(500, new { message = "Error al obtener los pendientes" });
            }
        }

        // ─────────────────────────────────────────────
        // Helpers privados
        // ─────────────────────────────────────────────
        private async Task<int?> ObtenerVendedorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("PersonaId")?.Value;

            if (!int.TryParse(userIdClaim, out int personaId)) return null;

            var vendedor = await _unitOfWork.VendedorRepository.GetByPersonaIdAsync(personaId);
            return vendedor?.Id;
        }

        /// <summary>
        /// Valida que la transición de estados sea lógica y hacia adelante.
        /// </summary>
        private static List<string> GetAccionesDisponibles(EstadoDetalleVenta estado) => estado switch
        {
            EstadoDetalleVenta.Pendiente     => new List<string> { "Confirmado", "Cancelado" },
            EstadoDetalleVenta.Confirmado    => new List<string> { "EnPreparacion", "Cancelado" },
            EstadoDetalleVenta.EnPreparacion => new List<string> { "Listo" },
            EstadoDetalleVenta.Listo         => new List<string> { "Entregado" },
            _                                => new List<string>()
        };

        private bool EsTransicionValida(EstadoDetalleVenta actual, EstadoDetalleVenta nuevo)
        {
            // Cancelado siempre es válido desde cualquier estado (excepto Entregado)
            if (nuevo == EstadoDetalleVenta.Cancelado && actual != EstadoDetalleVenta.Entregado)
                return true;

            // No se puede ir hacia atrás
            return (int)nuevo > (int)actual;
        }

        private object MapDetalleVendedor(DetalleVenta d) => new
        {
            id = d.Id,
            ventaId = d.VentaId,
            numeroOrden = d.Venta?.NumeroOrden,
            fechaVenta = d.Venta?.FechaVenta,
            estado = d.Estado.ToString(),
            estadoNumerico = (int)d.Estado,

            // Torta
            tortaId = d.TortaId,
            nombreTorta = d.Torta?.Nombre,
            imagenTorta = d.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
            cantidad = d.Cantidad,
            precioUnitario = d.PrecioUnitario,
            descuento = d.Descuento,
            subtotal = d.Subtotal,
            notasPersonalizacion = d.NotasPersonalizacion,

            // Comprador
            nombreComprador = d.Venta?.Comprador?.Persona?.Nombre,
            telefonoComprador = d.Venta?.Comprador?.Telefono,
            direccionEntrega = d.Venta?.DireccionEntrega,

            // Fechas
            fechaEstimadaPreparacion = d.FechaEstimadaPreparacion,
            fechaRealPreparacion = d.FechaRealPreparacion
        };
    }

}