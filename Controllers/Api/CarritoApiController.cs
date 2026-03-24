using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models;
using CasaDeLasTortas.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CarritoApiController : ControllerBase
    {
        private readonly ICarritoService _carritoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CarritoApiController> _logger;

        public CarritoApiController(
            ICarritoService carritoService,
            IUnitOfWork unitOfWork,
            ILogger<CarritoApiController> logger)
        {
            _carritoService = carritoService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // GET /api/Carrito
        // Devuelve el carrito completo con detalles
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetCarrito()
        {
            try
            {
                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                return Ok(MapCarritoResponse(carrito));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo carrito");
                return StatusCode(500, new { message = "Error al obtener el carrito" });
            }
        }

        // ─────────────────────────────────────────────
        // GET /api/Carrito/contador
        // Solo devuelve la cantidad de items (para el badge del navbar)
        // ─────────────────────────────────────────────
        [HttpGet("contador")]
        public IActionResult GetContador()
        {
            try
            {
                var total = _carritoService.GetTotalItems();
                return Ok(new { total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo contador");
                return StatusCode(500, new { message = "Error al obtener el contador" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Carrito/agregar
        // Body: { "tortaId": 1, "cantidad": 2, "notas": "sin azúcar" }
        // ─────────────────────────────────────────────
        [HttpPost("agregar")]
        public async Task<IActionResult> Agregar([FromBody] AgregarItemRequest request)
        {
            if (request.TortaId <= 0 || request.Cantidad <= 0)
                return BadRequest(new { message = "TortaId y cantidad son requeridos y deben ser positivos" });

            try
            {
                // Verificar que la torta existe y tiene stock
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(request.TortaId);
                if (torta == null)
                    return NotFound(new { message = "Torta no encontrada" });

                if (!torta.Disponible || torta.Stock < request.Cantidad)
                    return BadRequest(new { message = "La torta no está disponible o no hay suficiente stock" });

                var resultado = await _carritoService.AgregarItem(
                    request.TortaId,
                    request.Cantidad,
                    request.Notas
                );

                if (!resultado)
                    return BadRequest(new { message = "No se pudo agregar el producto al carrito" });

                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                return Ok(new
                {
                    success = true,
                    message = $"{torta.Nombre} agregada al carrito",
                    totalItems = carrito.TotalItems,
                    carrito = MapCarritoResponse(carrito)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando item al carrito: TortaId={TortaId}", request.TortaId);
                return StatusCode(500, new { message = "Error al agregar al carrito" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Carrito/actualizar
        // Body: { "tortaId": 1, "cantidad": 3 }
        // ─────────────────────────────────────────────
        [HttpPost("actualizar")]
        public async Task<IActionResult> ActualizarCantidad([FromBody] ActualizarItemRequest request)
        {
            if (request.TortaId <= 0 || request.Cantidad <= 0)
                return BadRequest(new { message = "TortaId y cantidad son requeridos" });

            try
            {
                var resultado = await _carritoService.ActualizarCantidad(request.TortaId, request.Cantidad);

                if (!resultado)
                    return BadRequest(new { message = "No hay suficiente stock disponible" });

                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                return Ok(new
                {
                    success = true,
                    totalItems = carrito.TotalItems,
                    carrito = MapCarritoResponse(carrito)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cantidad: TortaId={TortaId}", request.TortaId);
                return StatusCode(500, new { message = "Error al actualizar la cantidad" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Carrito/quitar
        // Body: { "tortaId": 1 }
        // ─────────────────────────────────────────────
        [HttpPost("quitar")]
        public async Task<IActionResult> QuitarItem([FromBody] QuitarItemRequest request)
        {
            if (request.TortaId <= 0)
                return BadRequest(new { message = "TortaId es requerido" });

            try
            {
                var resultado = await _carritoService.QuitarItem(request.TortaId);

                if (!resultado)
                    return NotFound(new { message = "Item no encontrado en el carrito" });

                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                return Ok(new
                {
                    success = true,
                    message = "Producto eliminado del carrito",
                    totalItems = carrito.TotalItems,
                    carrito = MapCarritoResponse(carrito)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error quitando item: TortaId={TortaId}", request.TortaId);
                return StatusCode(500, new { message = "Error al quitar el producto" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Carrito/vaciar
        // ─────────────────────────────────────────────
        [HttpPost("vaciar")]
        public IActionResult VaciarCarrito()
        {
            try
            {
                _carritoService.VaciarCarrito();
                return Ok(new { success = true, message = "Carrito vaciado", totalItems = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error vaciando carrito");
                return StatusCode(500, new { message = "Error al vaciar el carrito" });
            }
        }

        // ─────────────────────────────────────────────
        // POST /api/Carrito/actualizar-notas
        // Body: { "tortaId": 1, "notas": "sin azúcar" }
        // ─────────────────────────────────────────────
        [HttpPost("actualizar-notas")]
        public async Task<IActionResult> ActualizarNotas([FromBody] ActualizarNotasRequest request)
        {
            try
            {
                var resultado = await _carritoService.ActualizarNotas(request.TortaId, request.Notas);
                return Ok(new { success = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando notas");
                return StatusCode(500, new { message = "Error al actualizar las notas" });
            }
        }

        // ─────────────────────────────────────────────
        // Helpers privados
        // ─────────────────────────────────────────────
        private int? ObtenerCompradorId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("PersonaId")?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }

        private object MapCarritoResponse(CarritoSession carrito)
        {
            return new
            {
                items = carrito.Items.Select(i => new
                {
                    tortaId          = i.TortaId,
                    nombre           = i.Nombre,
                    precioUnitario   = i.PrecioUnitario,
                    cantidad         = i.Cantidad,
                    descuento        = i.Descuento,
                    subtotal         = i.Subtotal,
                    total            = i.Total,
                    notasPersonalizacion = i.NotasPersonalizacion,
                    vendedorId       = i.VendedorId,
                    nombreVendedor   = i.NombreVendedor,
                    imagenPrincipal  = i.ImagenPrincipal
                }),
                totalItems    = carrito.TotalItems,
                subtotal      = carrito.Subtotal,
                descuentoTotal = carrito.DescuentoTotal,
                total         = carrito.Total,
                fechaCreacion = carrito.FechaCreacion
            };
        }
    }

    // ─────────────────────────────────────────────
    // Request DTOs (clases simples para el body)
    // ─────────────────────────────────────────────
    public record AgregarItemRequest(int TortaId, int Cantidad, string? Notas = null);
    public record ActualizarItemRequest(int TortaId, int Cantidad);
    public record QuitarItemRequest(int TortaId);
    public record ActualizarNotasRequest(int TortaId, string? Notas);
}