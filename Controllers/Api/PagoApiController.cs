using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PagoApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IPagoService _pagoService;
        private readonly ILogger<PagoApiController> _logger;

        public PagoApiController(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            IPagoService pagoService,
            ILogger<PagoApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _pagoService = pagoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los pagos con paginación (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();
            var total = pagos.Count();

            var pagosPaginados = pagos
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(MapearPagoResumen)
                .ToList();

            return Ok(new
            {
                data = pagosPaginados,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener pago por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            return Ok(MapearPagoDetalle(pago));
        }

        /// <summary>
        /// Obtener pagos del comprador actual
        /// </summary>
        [HttpGet("mis-pagos")]
        public async Task<IActionResult> GetMisPagos()
        {
            var compradorId = await ObtenerCompradorIdActual();
            if (compradorId == null)
                return Unauthorized(new { message = "Comprador no encontrado" });

            var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdWithDetailsAsync(compradorId.Value);
            return Ok(pagos.Select(MapearPagoResumen));
        }

        /// <summary>
        ///  Obtener datos de pago de la plataforma
        /// </summary>
        [HttpGet("datos-plataforma")]
        public async Task<IActionResult> GetDatosPlataforma()
        {
            var datos = await _pagoService.GetDatosPagoPlataformaAsync();
            return Ok(datos);
        }

        /// <summary>
        /// Subir comprobante de pago
        /// POST /api/PagoApi/{pagoId}/comprobante
        /// </summary>
        [HttpPost("{pagoId}/comprobante")]
        public async Task<IActionResult> SubirComprobante(int pagoId, [FromForm] SubirComprobanteApiDTO request)
        {
            try
            {
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                    return Unauthorized(new { message = "Comprador no encontrado" });

                var pago = await _unitOfWork.PagoRepository.GetByIdAsync(pagoId);
                if (pago == null)
                    return NotFound(new { message = "Pago no encontrado" });

                if (pago.CompradorId != compradorId)
                    return Forbid();

                // Validar que puede subir comprobante
                if (!await _unitOfWork.PagoRepository.PuedeSubirComprobanteAsync(pagoId))
                    return BadRequest(new { message = "No se puede subir comprobante en el estado actual" });

                // Validar intentos
                var maxIntentos = await _unitOfWork.Configuracion.GetMaxIntentosRechazadosAsync();
                if (pago.IntentosRechazados >= maxIntentos)
                    return BadRequest(new { message = $"Se superó el máximo de {maxIntentos} intentos. Contacte soporte." });

                // Guardar archivo
                string archivoComprobante;
                if (request.Archivo != null)
                {
                    archivoComprobante = await _fileService.SaveFileAsync(request.Archivo, "comprobantes");
                }
                else if (!string.IsNullOrEmpty(request.ArchivoBase64))
                {
                    archivoComprobante = await _fileService.SaveBase64FileAsync(
                        request.ArchivoBase64, "comprobantes", request.NombreArchivo ?? "comprobante.jpg");
                }
                else
                {
                    return BadRequest(new { message = "Debe enviar un archivo o imagen base64" });
                }

                // Procesar
                var result = await _pagoService.SubirComprobanteAsync(
                    pago.VentaId, compradorId.Value, archivoComprobante,
                    request.MetodoPago, request.NumeroTransaccion);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    urlComprobante = _fileService.GetFileUrl(archivoComprobante)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir comprobante para pago {PagoId}", pagoId);
                return StatusCode(500, new { message = "Error al subir el comprobante" });
            }
        }


        /// <summary>
        /// Subir comprobante usando VentaId (alternativo)
        /// POST /api/PagoApi/subir-comprobante
        /// </summary>
        [HttpPost("subir-comprobante")]
        public async Task<IActionResult> SubirComprobanteByVenta([FromForm] SubirComprobantePorVentaDTO request)
        {
            try
            {
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                    return Unauthorized(new { message = "Comprador no encontrado" });

                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(request.VentaId);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                if (venta.CompradorId != compradorId)
                    return Forbid();

                var pago = venta.Pagos?
                    .Where(p => p.Estado == EstadoPago.Pendiente || p.Estado == EstadoPago.EnRevision)
                    .OrderByDescending(p => p.FechaPago)
                    .FirstOrDefault();

                if (pago == null)
                {
                    var comisionPorcentaje = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();
                    var comision = venta.Total * (comisionPorcentaje / 100m);

                    pago = new Pago
                    {
                        VentaId = request.VentaId,
                        CompradorId = compradorId.Value,
                        Monto = venta.Total,
                        ComisionPlataforma = comision,
                        MontoVendedores = venta.Total - comision,
                        Estado = EstadoPago.Pendiente,
                        MetodoPago = MetodoPago.Transferencia,
                        FechaPago = DateTime.Now
                    };

                    await _unitOfWork.PagoRepository.AddAsync(pago);
                    await _unitOfWork.SaveChangesAsync();
                }

                if (request.Comprobante == null || request.Comprobante.Length == 0)
                    return BadRequest(new { message = "Debe enviar un archivo de comprobante" });

                var archivoComprobante = await _fileService.SaveFileAsync(request.Comprobante, "comprobantes");

                pago.ArchivoComprobante = archivoComprobante;
                pago.FechaComprobante = DateTime.Now;
                pago.NumeroTransaccion = request.NumeroTransaccion;
                pago.MetodoPago = MetodoPago.Transferencia;
                pago.Estado = EstadoPago.EnRevision;

                _unitOfWork.PagoRepository.Update(pago);

                venta.Estado = EstadoVenta.PagoEnRevision;
                _unitOfWork.Ventas.Update(venta);

                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Comprobante enviado correctamente.",
                    pagoId = pago.Id,
                    urlComprobante = _fileService.GetFileUrl(archivoComprobante)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir comprobante para venta {VentaId}", request.VentaId);
                return StatusCode(500, new { message = "Error al subir el comprobante" });
            }
        }

        /// <summary>
        ///  Obtener pagos pendientes de verificación (Admin)
        /// </summary>
        [HttpGet("pendientes-verificacion")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetPendientesVerificacion()
        {
            var pagos = await _unitOfWork.PagoRepository.GetPendientesVerificacionAsync();
            return Ok(pagos.Select(p => new
            {
                id = p.Id,
                ventaId = p.VentaId,
                numeroOrden = p.Venta?.NumeroOrden,
                compradorNombre = p.Comprador?.Persona?.Nombre,
                compradorEmail = p.Comprador?.Persona?.Email,
                monto = p.Monto,
                comisionPlataforma = p.ComisionPlataforma,
                montoVendedores = p.MontoVendedores,
                metodoPago = p.MetodoPago?.ToString(),
                numeroTransaccion = p.NumeroTransaccion,
                fechaComprobante = p.FechaComprobante,
                urlComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante)
                    ? _fileService.GetFileUrl(p.ArchivoComprobante) : null,
                intentosRechazados = p.IntentosRechazados,
                vendedoresInvolucrados = p.Venta?.Detalles?
                    .GroupBy(d => d.VendedorId)
                    .Select(g => new
                    {
                        vendedorId = g.Key,
                        nombreComercial = g.First().Vendedor?.NombreComercial,
                        monto = g.Sum(d => d.Subtotal)
                    }).ToList()
            }));
        }

        /// <summary>
        ///  Verificar pago (Admin aprueba o rechaza)
        /// </summary>
        [HttpPost("{pagoId}/verificar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> VerificarPago(int pagoId, [FromBody] VerificarPagoDTO request)
        {
            try
            {
                var adminId = await ObtenerPersonaIdActual();
                if (adminId == null)
                    return Unauthorized();

                var result = await _pagoService.VerificarPagoAsync(
                    pagoId, adminId.Value, request.Aprobado,
                    request.Observaciones, request.MotivoRechazo);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar pago {PagoId}", pagoId);
                return StatusCode(500, new { message = "Error al verificar el pago" });
            }
        }

        /// <summary>
        ///  Obtener pagos rechazados
        /// </summary>
        [HttpGet("rechazados")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetRechazados()
        {
            var pagos = await _unitOfWork.PagoRepository.GetRechazadosAsync();
            return Ok(pagos.Select(p => new
            {
                id = p.Id,
                ventaId = p.VentaId,
                numeroOrden = p.Venta?.NumeroOrden,
                compradorNombre = p.Comprador?.Persona?.Nombre,
                monto = p.Monto,
                motivoRechazo = p.MotivoRechazo,
                fechaRechazo = p.FechaRechazo,
                intentosRechazados = p.IntentosRechazados
            }));
        }

        /// <summary>
        ///  Obtener reembolsos pendientes
        /// </summary>
        [HttpGet("reembolsos-pendientes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetReembolsosPendientes()
        {
            var pagos = await _unitOfWork.PagoRepository.GetReembolsosPendientesAsync();
            return Ok(pagos.Select(p => new
            {
                id = p.Id,
                ventaId = p.VentaId,
                numeroOrden = p.Venta?.NumeroOrden,
                compradorNombre = p.Comprador?.Persona?.Nombre,
                compradorEmail = p.Comprador?.Persona?.Email,
                monto = p.Monto,
                fechaPago = p.FechaPago
            }));
        }

        /// <summary>
        ///  Procesar reembolso (Admin)
        /// </summary>
        [HttpPost("{pagoId}/reembolso")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> ProcesarReembolso(int pagoId, [FromForm] ProcesarReembolsoApiDTO request)
        {
            try
            {
                var adminId = await ObtenerPersonaIdActual();
                if (adminId == null)
                    return Unauthorized();

                // Guardar comprobante de reembolso
                string archivoComprobante;
                if (request.Archivo != null)
                {
                    archivoComprobante = await _fileService.SaveFileAsync(request.Archivo, "reembolsos");
                }
                else
                {
                    return BadRequest(new { message = "Debe adjuntar comprobante de transferencia" });
                }

                var result = await _pagoService.ProcesarReembolsoAsync(
                    pagoId, adminId.Value, archivoComprobante, request.NumeroTransaccion);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar reembolso para pago {PagoId}", pagoId);
                return StatusCode(500, new { message = "Error al procesar el reembolso" });
            }
        }

        /// <summary>
        ///  Estadísticas de pagos y comisiones (Admin)
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var stats = await _pagoService.GetEstadisticasAsync();

            // Agregar datos adicionales
            var comisionesDelMes = await _unitOfWork.PagoRepository.GetComisionesDelMesAsync();
            var montoEnRevision = await _unitOfWork.PagoRepository.GetMontoEnRevisionAsync();
            var tiempoPromedioVerificacion = await _unitOfWork.PagoRepository.GetTiempoPromedioVerificacionAsync();

            return Ok(new
            {
                totalPagos = stats.TotalPagos,
                pagosPendientes = stats.PagosPendientes,
                pagosEnRevision = stats.PagosEnRevision,
                pagosVerificados = stats.PagosVerificados,
                pagosRechazados = stats.PagosRechazados,
                montoTotalRecibido = stats.MontoTotalRecibido,
                pagosHoy = stats.PagosHoy,
                montoHoy = stats.MontoHoy,
                comisionesDelMes,
                montoEnRevision,
                tiempoPromedioVerificacionHoras = tiempoPromedioVerificacion
            });
        }

        /// <summary>
        /// Obtener pagos por comprador
        /// </summary>
        [HttpGet("comprador/{compradorId}")]
        public async Task<IActionResult> GetByComprador(int compradorId)
        {
            var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdWithDetailsAsync(compradorId);
            return Ok(pagos.Select(MapearPagoResumen));
        }

        /// <summary>
        /// Obtener pagos por vendedor
        /// </summary>
        [HttpGet("vendedor/{vendedorId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> GetByVendedor(int vendedorId)
        {
            var pagos = await _unitOfWork.PagoRepository.GetByVendedorIdAsync(vendedorId);
            return Ok(pagos.Select(p => MapearPagoResumen(p)));
        }

        /// <summary>
        /// Cambiar estado de detalle de venta (Vendedor)
        /// </summary>
        [HttpPatch("detalle/{detalleId}/estado")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor,Admin")]
        public async Task<IActionResult> CambiarEstadoDetalle(int detalleId, [FromBody] CambiarEstadoDetalleDTO request)
        {
            if (!Enum.TryParse<EstadoDetalleVenta>(request.Estado, out var nuevoEstado))
                return BadRequest(new { message = "Estado inválido" });

            try
            {
                var detalle = await _unitOfWork.DetallesVenta.GetByIdWithTodoAsync(detalleId);
                if (detalle == null)
                    return NotFound(new { message = "Detalle no encontrado" });

                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null || detalle.VendedorId != vendedorId)
                    return Forbid();

                // Validar transición
                var transicionValida = ValidarTransicionEstado(detalle.Estado, nuevoEstado);
                if (!transicionValida)
                    return BadRequest(new { message = $"No se puede cambiar de '{detalle.Estado}' a '{nuevoEstado}'" });

                detalle.Estado = nuevoEstado;
                if (nuevoEstado == EstadoDetalleVenta.Entregado)
                    detalle.FechaRealPreparacion = DateTime.Now;
                if (nuevoEstado == EstadoDetalleVenta.EnPreparacion)
                    detalle.FechaEstimadaPreparacion = DateTime.Now.AddDays(detalle.Torta?.TiempoPreparacion ?? 1);

                _unitOfWork.DetallesVenta.Update(detalle);

                // Actualizar estado de venta
                await ActualizarEstadoVenta(detalle.VentaId);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = $"Estado actualizado a '{nuevoEstado}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del detalle {DetalleId}", detalleId);
                return StatusCode(500, new { message = "Error al actualizar el estado" });
            }
        }

        // ==================== HELPERS ====================

        private object MapearPagoResumen(Pago p) => new
        {
            id = p.Id,
            ventaId = p.VentaId,
            numeroOrden = p.Venta?.NumeroOrden,
            compradorId = p.CompradorId,
            nombreComprador = p.Comprador?.Persona?.Nombre,
            monto = p.Monto,
            comisionPlataforma = p.ComisionPlataforma,
            montoVendedores = p.MontoVendedores,
            estado = p.Estado.ToString(),
            metodoPago = p.MetodoPago?.ToString(),
            fechaPago = p.FechaPago,
            tieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante),
            intentosRechazados = p.IntentosRechazados
        };

        private object MapearPagoDetalle(Pago pago) => new
        {
            id = pago.Id,
            ventaId = pago.VentaId,
            numeroOrden = pago.Venta?.NumeroOrden,
            compradorId = pago.CompradorId,
            nombreComprador = pago.Comprador?.Persona?.Nombre,
            emailComprador = pago.Comprador?.Persona?.Email,
            monto = pago.Monto,
            comisionPlataforma = pago.ComisionPlataforma,
            montoVendedores = pago.MontoVendedores,
            estado = pago.Estado.ToString(),
            metodoPago = pago.MetodoPago?.ToString(),
            fechaPago = pago.FechaPago,
            numeroTransaccion = pago.NumeroTransaccion,
            archivoComprobante = pago.ArchivoComprobante,
            urlComprobante = !string.IsNullOrEmpty(pago.ArchivoComprobante)
                ? _fileService.GetFileUrl(pago.ArchivoComprobante) : null,
            fechaComprobante = pago.FechaComprobante,
            fechaVerificacion = pago.FechaVerificacion,
            motivoRechazo = pago.MotivoRechazo,
            observacionesAdmin = pago.ObservacionesAdmin,
            intentosRechazados = pago.IntentosRechazados,
            detalles = pago.Venta?.Detalles?.Select(d => new
            {
                tortaId = d.TortaId,
                nombreTorta = d.Torta?.Nombre,
                cantidad = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal = d.Subtotal,
                vendedorId = d.VendedorId,
                nombreVendedor = d.Vendedor?.NombreComercial
            }).ToList()
        };

        private bool ValidarTransicionEstado(EstadoDetalleVenta actual, EstadoDetalleVenta nuevo)
        {
            return (actual, nuevo) switch
            {
                (EstadoDetalleVenta.Pendiente, EstadoDetalleVenta.Confirmado) => true,
                (EstadoDetalleVenta.Pendiente, EstadoDetalleVenta.Cancelado) => true,
                (EstadoDetalleVenta.Confirmado, EstadoDetalleVenta.EnPreparacion) => true,
                (EstadoDetalleVenta.Confirmado, EstadoDetalleVenta.Cancelado) => true,
                (EstadoDetalleVenta.EnPreparacion, EstadoDetalleVenta.Listo) => true,
                (EstadoDetalleVenta.Listo, EstadoDetalleVenta.Entregado) => true,
                _ => false
            };
        }

        private async Task ActualizarEstadoVenta(int ventaId)
        {
            var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(ventaId);
            if (venta == null) return;

            if (venta.Detalles.All(d => d.Estado == EstadoDetalleVenta.Entregado))
            {
                venta.Estado = EstadoVenta.Entregada;
                venta.FechaEntregaReal = DateTime.Now;
            }
            else if (venta.Detalles.Any(d => d.Estado == EstadoDetalleVenta.Listo))
                venta.Estado = EstadoVenta.ListaParaRetiro;
            else if (venta.Detalles.Any(d => d.Estado == EstadoDetalleVenta.EnPreparacion))
                venta.Estado = EstadoVenta.EnPreparacion;

            venta.FechaActualizacion = DateTime.Now;
            _unitOfWork.Ventas.Update(venta);
        }

        private async Task<int?> ObtenerCompradorIdActual()
        {
            var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(personaIdClaim, out int personaId)) return null;
            var comprador = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(personaId);
            return comprador?.Id;
        }

        private async Task<int?> ObtenerVendedorIdActual()
        {
            var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(personaIdClaim, out int personaId)) return null;
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(personaId);
            return persona?.Vendedor?.Id;
        }

        private async Task<int?> ObtenerPersonaIdActual()
        {
            var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(personaIdClaim, out int personaId)) return null;
            return personaId;
        }

        /// <summary>
        /// subir comprobante usando VentaId (alternativo)
        /// POST /api/PagoApi/venta/{ventaId}/comprobante
        /// </summary>
        [HttpPost("venta/{ventaId}/comprobante")]
        public async Task<IActionResult> SubirComprobantePorVenta(int ventaId, [FromForm] SubirComprobanteApiDTO request)
        {
            try
            {
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                    return Unauthorized(new { message = "Comprador no encontrado" });

                // Verificar que la venta existe y pertenece al comprador
                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(ventaId);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                if (venta.CompradorId != compradorId)
                    return Forbid();

                // Verificar que la venta está en un estado que permite subir comprobante
                if (venta.Estado != EstadoVenta.Pendiente && venta.Estado != EstadoVenta.PagoEnRevision)
                    return BadRequest(new { message = $"No se puede subir comprobante para una venta en estado '{venta.Estado}'" });

                // Buscar pago existente por ventaId
                var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(ventaId);
                var pago = pagos.FirstOrDefault();

                if (pago == null)
                {
                    // Si no existe pago, crear uno nuevo (ordenes antiguas sin pago creado)
                    _logger.LogInformation("Creando pago faltante para Venta {VentaId} del Comprador {CompradorId}", ventaId, compradorId);

                    var comisionPct = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();
                    pago = new Pago
                    {
                        VentaId = ventaId,
                        CompradorId = compradorId.Value,
                        Monto = venta.Total,
                        ComisionPlataforma = Math.Round(venta.Total * (comisionPct / 100m), 2),
                        MontoVendedores = Math.Round(venta.Total * (1 - comisionPct / 100m), 2),
                        FechaPago = DateTime.Now,
                        Estado = EstadoPago.Pendiente
                    };
                    await _unitOfWork.PagoRepository.AddAsync(pago);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    // Validar que el pago existente pueda recibir comprobante
                    var maxIntentos = await _unitOfWork.Configuracion.GetMaxIntentosRechazadosAsync();
                    if (pago.IntentosRechazados >= maxIntentos)
                        return BadRequest(new { message = $"Se superó el máximo de {maxIntentos} intentos. Contacte soporte." });

                    if (pago.Estado != EstadoPago.Pendiente && pago.Estado != EstadoPago.Rechazado)
                        return BadRequest(new { message = "El pago no acepta comprobante en su estado actual" });
                }

                // Guardar el archivo
                string archivoComprobante;
                if (request.Archivo != null)
                {
                    archivoComprobante = await _fileService.SaveFileAsync(request.Archivo, "comprobantes");
                }
                else if (!string.IsNullOrEmpty(request.ArchivoBase64))
                {
                    archivoComprobante = await _fileService.SaveBase64FileAsync(
                        request.ArchivoBase64, "comprobantes", request.NombreArchivo ?? "comprobante.jpg");
                }
                else
                {
                    return BadRequest(new { message = "Debe enviar un archivo o imagen base64" });
                }

                // Delegar al servicio de pagos
                var result = await _pagoService.SubirComprobanteAsync(
                    ventaId, compradorId.Value, archivoComprobante,
                    request.MetodoPago, request.NumeroTransaccion);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    pagoId = pago.Id,
                    urlComprobante = _fileService.GetFileUrl(archivoComprobante)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir comprobante para venta {VentaId}", ventaId);
                return StatusCode(500, new { message = "Error al subir el comprobante" });
            }
        }
    }
}