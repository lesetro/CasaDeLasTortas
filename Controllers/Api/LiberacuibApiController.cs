using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LiberacionApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILiberacionService _liberacionService;
        private readonly IFileService _fileService;
        private readonly ILogger<LiberacionApiController> _logger;

        public LiberacionApiController(
            IUnitOfWork unitOfWork,
            ILiberacionService liberacionService,
            IFileService fileService,
            ILogger<LiberacionApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _liberacionService = liberacionService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las liberaciones (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var liberaciones = await _unitOfWork.Liberaciones.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.Liberaciones.CountAsync();

            return Ok(new
            {
                data = liberaciones.Select(MapearLiberacion),
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener liberación por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> GetById(int id)
        {
            var liberacion = await _unitOfWork.Liberaciones.GetByIdWithDetallesAsync(id);
            if (liberacion == null)
                return NotFound(new { message = "Liberación no encontrada" });

            return Ok(MapearLiberacionDetalle(liberacion));
        }

        /// <summary>
        /// Obtener liberaciones pendientes de procesar (Admin)
        /// </summary>
        [HttpGet("pendientes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetPendientes()
        {
            var liberaciones = await _liberacionService.GetLiberacionesPendientesAsync();
            return Ok(liberaciones);
        }

        /// <summary>
        /// Obtener liberaciones listas para liberar (Admin)
        /// </summary>
        [HttpGet("listas-para-liberar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetListasParaLiberar()
        {
            var liberaciones = await _unitOfWork.Liberaciones.GetListasParaLiberarAsync();
            return Ok(liberaciones.Select(l => new
            {
                id = l.Id,
                ventaId = l.VentaId,
                numeroOrden = l.Venta?.NumeroOrden,
                vendedorId = l.VendedorId,
                nombreComercial = l.Vendedor?.NombreComercial,
                nombreVendedor = l.Vendedor?.Persona?.Nombre,
                email = l.Vendedor?.Persona?.Email,
                aliasCBU = l.Vendedor?.AliasCBU,
                cbu = l.Vendedor?.CBU,
                banco = l.Vendedor?.Banco,
                titularCuenta = l.Vendedor?.TitularCuenta,
                datosPagoCompletos = l.Vendedor?.DatosPagoCompletos ?? false,
                montoBruto = l.MontoBruto,
                comision = l.Comision,
                montoNeto = l.MontoNeto,
                fechaListoParaLiberar = l.FechaListoParaLiberar,
                diasEsperando = l.FechaListoParaLiberar.HasValue
                    ? (int)(DateTime.Now - l.FechaListoParaLiberar.Value).TotalDays : 0
            }));
        }

        /// <summary>
        /// Obtener liberaciones por estado (Admin)
        /// </summary>
        [HttpGet("estado/{estado}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            if (!Enum.TryParse<EstadoLiberacion>(estado, true, out var estadoEnum))
                return BadRequest(new { message = "Estado inválido" });

            var liberaciones = await _unitOfWork.Liberaciones.GetByEstadoAsync(estadoEnum);
            return Ok(liberaciones.Select(MapearLiberacion));
        }

        /// <summary>
        /// Obtener liberaciones por vendedor
        /// </summary>
        [HttpGet("vendedor/{vendedorId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> GetByVendedor(int vendedorId)
        {
            var liberaciones = await _unitOfWork.Liberaciones.GetByVendedorIdAsync(vendedorId);
            return Ok(liberaciones.Select(MapearLiberacion));
        }

        /// <summary>
        /// Obtener liberaciones por venta
        /// </summary>
        [HttpGet("venta/{ventaId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetByVenta(int ventaId)
        {
            var liberaciones = await _unitOfWork.Liberaciones.GetByVentaIdAsync(ventaId);
            return Ok(liberaciones.Select(MapearLiberacion));
        }

        /// <summary>
        /// Procesar transferencia a vendedor (Admin)
        /// </summary>
        [HttpPost("{id}/transferir")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> ProcesarTransferencia(int id, [FromForm] ProcesarTransferenciaDTO request)
        {
            try
            {
                var adminId = await ObtenerPersonaIdActual();
                if (adminId == null)
                    return Unauthorized();

                // Guardar comprobante
                string archivoComprobante;
                if (request.Archivo != null)
                {
                    archivoComprobante = await _fileService.SaveFileAsync(request.Archivo, "liberaciones");
                }
                else
                {
                    return BadRequest(new { message = "Debe adjuntar comprobante de transferencia" });
                }

                var result = await _liberacionService.ProcesarLiberacionAsync(
                    id, adminId.Value, request.NumeroOperacion, archivoComprobante);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar transferencia para liberación {Id}", id);
                return StatusCode(500, new { message = "Error al procesar la transferencia" });
            }
        }

        /// <summary>
        /// Confirmar recepción de fondos (Vendedor)
        /// </summary>
        [HttpPost("{id}/confirmar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> ConfirmarRecepcion(int id)
        {
            try
            {
                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null)
                    return Unauthorized();

                var result = await _liberacionService.ConfirmarRecepcionVendedorAsync(id, vendedorId.Value);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar recepción para liberación {Id}", id);
                return StatusCode(500, new { message = "Error al confirmar la recepción" });
            }
        }

        /// <summary>
        /// Marcar entrega confirmada por comprador (dispara liberación)
        /// </summary>
        [HttpPost("venta/{ventaId}/entrega-confirmada")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Comprador")]
        public async Task<IActionResult> MarcarEntregaConfirmada(int ventaId)
        {
            try
            {
                var result = await _liberacionService.MarcarEntregaConfirmadaAsync(ventaId);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar entrega confirmada para venta {VentaId}", ventaId);
                return StatusCode(500, new { message = "Error al confirmar la entrega" });
            }
        }

        /// <summary>
        /// Obtener estadísticas de liberaciones (Admin)
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var stats = await _liberacionService.GetEstadisticasAsync();
            return Ok(stats);
        }

        // ==================== HELPERS ====================

        private object MapearLiberacion(LiberacionFondos l) => new
        {
            id = l.Id,
            ventaId = l.VentaId,
            numeroOrden = l.Venta?.NumeroOrden,
            vendedorId = l.VendedorId,
            nombreComercial = l.Vendedor?.NombreComercial,
            montoBruto = l.MontoBruto,
            comision = l.Comision,
            montoNeto = l.MontoNeto,
            porcentajeComision = l.PorcentajeComision,
            estado = l.Estado.ToString(),
            fechaCreacion = l.FechaCreacion,
            fechaListoParaLiberar = l.FechaListoParaLiberar,
            fechaTransferencia = l.FechaTransferencia,
            fechaConfirmacion = l.FechaConfirmacion
        };

        private object MapearLiberacionDetalle(LiberacionFondos l) => new
        {
            id = l.Id,
            ventaId = l.VentaId,
            numeroOrden = l.Venta?.NumeroOrden,
            vendedorId = l.VendedorId,
            nombreComercial = l.Vendedor?.NombreComercial,
            nombreVendedor = l.Vendedor?.Persona?.Nombre,
            emailVendedor = l.Vendedor?.Persona?.Email,
            aliasCBU = l.AliasDestino,
            cbu = l.CBUDestino,
            titularCuenta = l.TitularDestino,
            montoBruto = l.MontoBruto,
            comision = l.Comision,
            montoNeto = l.MontoNeto,
            porcentajeComision = l.PorcentajeComision,
            estado = l.Estado.ToString(),
            fechaCreacion = l.FechaCreacion,
            fechaListoParaLiberar = l.FechaListoParaLiberar,
            fechaTransferencia = l.FechaTransferencia,
            numeroOperacion = l.NumeroOperacion,
            urlComprobante = !string.IsNullOrEmpty(l.ArchivoComprobante)
                ? _fileService.GetFileUrl(l.ArchivoComprobante) : null,
            fechaConfirmacion = l.FechaConfirmacion,
            procesadoPor = l.ProcesadoPor?.Nombre,
            compradorNombre = l.Venta?.Comprador?.Persona?.Nombre
        };

        private async Task<int?> ObtenerPersonaIdActual()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;
            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Id;
        }

        private async Task<int?> ObtenerVendedorIdActual()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var personaId))
                return null;
            var vendedor = await _unitOfWork.VendedorRepository.GetByPersonaIdAsync(personaId);
            return vendedor?.Id;
        }
    }

    // ==================== DTOs ====================

    public class ProcesarTransferenciaDTO
    {
        [Required]
        public IFormFile Archivo { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string NumeroOperacion { get; set; } = string.Empty;
    }
}