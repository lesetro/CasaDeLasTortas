using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagoApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<PagoApiController> _logger;

        public PagoApiController(
            IUnitOfWork unitOfWork, 
            IFileService fileService,
            ILogger<PagoApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los pagos con paginación
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var pagos = await _unitOfWork.PagoRepository.GetAllAsync();
            var total = pagos.Count();
            
            var pagosPaginados = pagos
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new
                {
                    id = p.Id,
                    tortaId = p.TortaId,
                    compradorId = p.CompradorId,
                    vendedorId = p.VendedorId,
                    monto = p.Monto,
                    cantidad = p.Cantidad,
                    estado = p.Estado.ToString(),
                    metodoPago = p.MetodoPago?.ToString(),
                    fechaPago = p.FechaPago,
                    tieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante)
                })
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
            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            return Ok(new
            {
                id = pago.Id,
                tortaId = pago.TortaId,
                compradorId = pago.CompradorId,
                vendedorId = pago.VendedorId,
                monto = pago.Monto,
                precioUnitario = pago.PrecioUnitario,
                cantidad = pago.Cantidad,
                subtotal = pago.Subtotal,
                descuento = pago.Descuento,
                estado = pago.Estado.ToString(),
                metodoPago = pago.MetodoPago?.ToString(),
                fechaPago = pago.FechaPago,
                numeroTransaccion = pago.NumeroTransaccion,
                observaciones = pago.Observaciones,
                direccionEntrega = pago.DireccionEntrega,
                fechaEntrega = pago.FechaEntrega,
                archivoComprobante = pago.ArchivoComprobante,
                urlComprobante = !string.IsNullOrEmpty(pago.ArchivoComprobante) 
                    ? _fileService.GetFileUrl(pago.ArchivoComprobante) 
                    : null,
                notificacionEnviada = pago.NotificacionEnviada,
                fechaActualizacion = pago.FechaActualizacion
            });
        }

        /// <summary>
        /// Obtener pagos por comprador
        /// </summary>
        [HttpGet("comprador/{compradorId}")]
        public async Task<IActionResult> GetByComprador(int compradorId)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(compradorId);
            
            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            var pagos = comprador.Pagos.Select(p => new
            {
                id = p.Id,
                tortaId = p.TortaId,
                monto = p.Monto,
                cantidad = p.Cantidad,
                estado = p.Estado.ToString(),
                metodoPago = p.MetodoPago?.ToString(),
                fechaPago = p.FechaPago,
                fechaEntrega = p.FechaEntrega
            }).ToList();

            return Ok(pagos);
        }

        /// <summary>
        /// Obtener pagos por vendedor
        /// </summary>
        [HttpGet("vendedor/{vendedorId}")]
        [Authorize]
        public async Task<IActionResult> GetByVendedor(int vendedorId)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(vendedorId);
            
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            var pagos = vendedor.PagosRecibidos.Select(p => new
            {
                id = p.Id,
                tortaId = p.TortaId,
                compradorId = p.CompradorId,
                monto = p.Monto,
                cantidad = p.Cantidad,
                estado = p.Estado.ToString(),
                metodoPago = p.MetodoPago?.ToString(),
                fechaPago = p.FechaPago,
                tieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante)
            }).ToList();

            return Ok(pagos);
        }

        /// <summary>
        /// Obtener pagos por estado
        /// </summary>
        [HttpGet("estado/{estado}")]
        [Authorize]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            if (!Enum.TryParse<EstadoPago>(estado, true, out var estadoPago))
                return BadRequest(new { message = "Estado inválido. Valores: Pendiente, Completado, Cancelado" });

            var pagos = await _unitOfWork.PagoRepository.GetAllAsync();
            var pagosFiltrados = pagos.Where(p => p.Estado == estadoPago).ToList();

            return Ok(pagosFiltrados.Select(p => new
            {
                id = p.Id,
                tortaId = p.TortaId,
                compradorId = p.CompradorId,
                vendedorId = p.VendedorId,
                monto = p.Monto,
                cantidad = p.Cantidad,
                estado = p.Estado.ToString(),
                fechaPago = p.FechaPago
            }));
        }

        /// <summary>
        /// Crear nuevo pago
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PagoCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verificar que la torta existe y tiene stock
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(dto.TortaId);
                if (torta == null)
                    return BadRequest(new { message = "Torta no encontrada" });

                if (torta.Stock < dto.Cantidad)
                    return BadRequest(new { message = $"Stock insuficiente. Disponible: {torta.Stock}" });

                // Verificar que el comprador existe
                var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(dto.CompradorId);
                if (comprador == null)
                    return BadRequest(new { message = "Comprador no encontrado" });

                // Calcular montos
                var subtotal = torta.Precio * dto.Cantidad;
                var descuento = dto.Descuento ?? 0;
                var montoTotal = subtotal - descuento;

                var pago = new Pago
                {
                    TortaId = dto.TortaId,
                    CompradorId = dto.CompradorId,
                    VendedorId = torta.VendedorId,
                    Cantidad = dto.Cantidad,
                    PrecioUnitario = torta.Precio,
                    Subtotal = subtotal,
                    Descuento = descuento,
                    Monto = montoTotal,
                    MetodoPago = dto.MetodoPago,
                    Estado = EstadoPago.Pendiente,
                    FechaPago = DateTime.Now,
                    NumeroTransaccion = dto.NumeroTransaccion,
                    Observaciones = dto.Observaciones,
                    DireccionEntrega = dto.DireccionEntrega,
                    FechaEntrega = dto.FechaEntrega,
                    NotificacionEnviada = false
                };

                // Guardar comprobante si existe
                if (dto.ArchivoComprobante != null)
                {
                    try
                    {
                        var rutaComprobante = await _fileService.SaveFileAsync(
                            dto.ArchivoComprobante, 
                            "comprobantes"
                        );
                        pago.ArchivoComprobante = rutaComprobante;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al guardar comprobante");
                        return BadRequest(new { message = "Error al guardar el comprobante: " + ex.Message });
                    }
                }

                await _unitOfWork.PagoRepository.AddAsync(pago);
                
                // Actualizar stock de la torta
                torta.Stock -= dto.Cantidad;
                torta.VecesVendida += dto.Cantidad;
                _unitOfWork.TortaRepository.Update(torta);

                // Actualizar total de compras del comprador
                comprador.TotalCompras += 1;
                _unitOfWork.CompradorRepository.Update(comprador);

                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = pago.Id }, new
                {
                    id = pago.Id,
                    monto = pago.Monto,
                    estado = pago.Estado.ToString(),
                    fechaPago = pago.FechaPago,
                    message = "Pago creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago");
                return StatusCode(500, new { message = "Error al procesar el pago" });
            }
        }

        /// <summary>
        /// Actualizar estado del pago
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoDTO dto)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            if (!Enum.TryParse<EstadoPago>(dto.NuevoEstado, true, out var nuevoEstado))
                return BadRequest(new { message = "Estado inválido. Valores: Pendiente, Completado, Cancelado" });

            pago.Estado = nuevoEstado;
            pago.FechaActualizacion = DateTime.Now;

            _unitOfWork.PagoRepository.Update(pago);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Estado actualizado exitosamente", 
                estado = pago.Estado.ToString() 
            });
        }

        /// <summary>
        /// Actualizar pago completo
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] PagoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            try
            {
                // Actualizar campos
                pago.Estado = dto.Estado;
                pago.MetodoPago = dto.MetodoPago;
                pago.NumeroTransaccion = dto.NumeroTransaccion;
                pago.Observaciones = dto.Observaciones;
                pago.DireccionEntrega = dto.DireccionEntrega;
                pago.FechaEntrega = dto.FechaEntrega;
                pago.FechaActualizacion = DateTime.Now;

                // Actualizar comprobante si se proporciona uno nuevo
                if (dto.ArchivoComprobante != null)
                {
                    // Eliminar comprobante anterior si existe
                    if (!string.IsNullOrEmpty(pago.ArchivoComprobante))
                    {
                        await _fileService.DeleteFileAsync(pago.ArchivoComprobante);
                    }

                    var rutaComprobante = await _fileService.SaveFileAsync(
                        dto.ArchivoComprobante, 
                        "comprobantes"
                    );
                    pago.ArchivoComprobante = rutaComprobante;
                }

                _unitOfWork.PagoRepository.Update(pago);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Pago actualizado exitosamente",
                    pago = new
                    {
                        id = pago.Id,
                        estado = pago.Estado.ToString(),
                        monto = pago.Monto,
                        fechaActualizacion = pago.FechaActualizacion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pago");
                return StatusCode(500, new { message = "Error al actualizar el pago" });
            }
        }

        /// <summary>
        /// Subir o actualizar comprobante de pago
        /// </summary>
        [HttpPost("{id}/comprobante")]
        public async Task<IActionResult> SubirComprobante(int id, [FromForm] IFormFile comprobante)
        {
            if (comprobante == null)
                return BadRequest(new { message = "Debe proporcionar un archivo" });

            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            try
            {
                // Eliminar comprobante anterior si existe
                if (!string.IsNullOrEmpty(pago.ArchivoComprobante))
                {
                    await _fileService.DeleteFileAsync(pago.ArchivoComprobante);
                }

                // Guardar nuevo comprobante
                var rutaComprobante = await _fileService.SaveFileAsync(comprobante, "comprobantes");
                pago.ArchivoComprobante = rutaComprobante;
                pago.FechaActualizacion = DateTime.Now;

                _unitOfWork.PagoRepository.Update(pago);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Comprobante guardado exitosamente",
                    urlComprobante = _fileService.GetFileUrl(rutaComprobante)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar comprobante");
                return BadRequest(new { message = "Error al guardar el comprobante: " + ex.Message });
            }
        }

        /// <summary>
        /// Confirmar pago (cambiar a completado)
        /// </summary>
        [HttpPost("{id}/confirmar")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> ConfirmarPago(int id, [FromBody] ConfirmarPagoDTO? dto = null)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            if (pago.Estado != EstadoPago.Pendiente)
                return BadRequest(new { message = "Solo se pueden confirmar pagos pendientes" });

            try
            {
                pago.Estado = EstadoPago.Completado;
                pago.FechaActualizacion = DateTime.Now;
                pago.NotificacionEnviada = true;

                if (dto != null && !string.IsNullOrEmpty(dto.NumeroTransaccion))
                {
                    pago.NumeroTransaccion = dto.NumeroTransaccion;
                }

                _unitOfWork.PagoRepository.Update(pago);

                // Actualizar TotalVentas del vendedor
                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(pago.VendedorId);
                if (vendedor != null)
                {
                    vendedor.TotalVentas += 1;
                    _unitOfWork.VendedorRepository.Update(vendedor);
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Pago confirmado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar pago");
                return StatusCode(500, new { message = "Error al confirmar el pago" });
            }
        }

        /// <summary>
        /// Cancelar pago
        /// </summary>
        [HttpPost("{id}/cancelar")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> CancelarPago(int id, [FromBody] CancelarPagoDTO dto)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            if (pago.Estado == EstadoPago.Completado)
                return BadRequest(new { message = "No se puede cancelar un pago completado" });

            try
            {
                var estadoAnterior = pago.Estado;
                
                pago.Estado = EstadoPago.Cancelado;
                pago.FechaActualizacion = DateTime.Now;
                pago.Observaciones = (pago.Observaciones ?? "") + $"\nCancelado: {dto.MotivoCancelacion}";

                _unitOfWork.PagoRepository.Update(pago);

                // Restaurar stock si estaba pendiente
                if (estadoAnterior == EstadoPago.Pendiente)
                {
                    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                    if (torta != null)
                    {
                        torta.Stock += pago.Cantidad;
                        torta.VecesVendida = Math.Max(0, torta.VecesVendida - pago.Cantidad);
                        _unitOfWork.TortaRepository.Update(torta);
                    }

                    // Decrementar TotalCompras del comprador
                    var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(pago.CompradorId);
                    if (comprador != null && comprador.TotalCompras > 0)
                    {
                        comprador.TotalCompras -= 1;
                        _unitOfWork.CompradorRepository.Update(comprador);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Pago cancelado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar pago");
                return StatusCode(500, new { message = "Error al cancelar el pago" });
            }
        }

        /// <summary>
        /// Eliminar pago (solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdAsync(id);

            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });

            try
            {
                // Eliminar comprobante si existe
                if (!string.IsNullOrEmpty(pago.ArchivoComprobante))
                {
                    await _fileService.DeleteFileAsync(pago.ArchivoComprobante);
                }

                _unitOfWork.PagoRepository.Delete(pago);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Pago eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago");
                return StatusCode(500, new { message = "Error al eliminar el pago" });
            }
        }

        /// <summary>
        /// Generar reporte de pagos por rango de fechas
        /// </summary>
        [HttpGet("reporte")]
        [Authorize]
        public async Task<IActionResult> GenerarReporte(
            [FromQuery] DateTime? desde, 
            [FromQuery] DateTime? hasta,
            [FromQuery] int? vendedorId = null)
        {
            var fechaDesde = desde ?? DateTime.Now.AddMonths(-1);
            var fechaHasta = hasta ?? DateTime.Now;

            var pagos = await _unitOfWork.PagoRepository.GetAllAsync();
            var pagosFiltrados = pagos.Where(p => 
                p.FechaPago >= fechaDesde && 
                p.FechaPago <= fechaHasta).ToList();

            // Filtrar por vendedor si se especifica
            if (vendedorId.HasValue)
            {
                pagosFiltrados = pagosFiltrados.Where(p => p.VendedorId == vendedorId.Value).ToList();
            }

            var reporte = new
            {
                periodo = new { desde = fechaDesde, hasta = fechaHasta },
                totalPagos = pagosFiltrados.Count,
                montoTotal = pagosFiltrados.Sum(p => p.Monto),
                
                porEstado = new
                {
                    pendientes = pagosFiltrados.Count(p => p.Estado == EstadoPago.Pendiente),
                    completados = pagosFiltrados.Count(p => p.Estado == EstadoPago.Completado),
                    cancelados = pagosFiltrados.Count(p => p.Estado == EstadoPago.Cancelado)
                },
                
                montosPorEstado = new
                {
                    pendiente = pagosFiltrados.Where(p => p.Estado == EstadoPago.Pendiente).Sum(p => p.Monto),
                    completado = pagosFiltrados.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto),
                    cancelado = pagosFiltrados.Where(p => p.Estado == EstadoPago.Cancelado).Sum(p => p.Monto)
                },
                
                porMetodoPago = pagosFiltrados
                    .Where(p => p.MetodoPago.HasValue)
                    .GroupBy(p => p.MetodoPago)
                    .Select(g => new
                    {
                        metodo = g.Key.ToString(),
                        cantidad = g.Count(),
                        monto = g.Sum(p => p.Monto)
                    })
                    .OrderByDescending(x => x.monto)
                    .ToList(),
                
                promedios = new
                {
                    montoPromedio = pagosFiltrados.Any() ? pagosFiltrados.Average(p => p.Monto) : 0,
                    cantidadPromedio = pagosFiltrados.Any() ? pagosFiltrados.Average(p => p.Cantidad) : 0
                }
            };

            return Ok(reporte);
        }

        /// <summary>
        /// Obtener estadísticas de pagos
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize]
        public async Task<IActionResult> GetEstadisticas([FromQuery] int? vendedorId = null)
        {
            var pagos = await _unitOfWork.PagoRepository.GetAllAsync();

            if (vendedorId.HasValue)
            {
                pagos = pagos.Where(p => p.VendedorId == vendedorId.Value).ToList();
            }

            var estadisticas = new
            {
                
                totales = new
                {
                    totalPagos = pagos.Count(),
                    montoTotal = pagos.Sum(p => p.Monto),
                    cantidadTotal = pagos.Sum(p => p.Cantidad)
                },
                
                hoy = new
                {
                    pagos = pagos.Count(p => p.FechaPago.Date == DateTime.Today),
                    monto = pagos.Where(p => p.FechaPago.Date == DateTime.Today).Sum(p => p.Monto)
                },
                
                esteMes = new
                {
                    pagos = pagos.Count(p => p.FechaPago.Month == DateTime.Now.Month && 
                                            p.FechaPago.Year == DateTime.Now.Year),
                    monto = pagos.Where(p => p.FechaPago.Month == DateTime.Now.Month && 
                                            p.FechaPago.Year == DateTime.Now.Year).Sum(p => p.Monto)
                },
                
                ultimos7Dias = pagos
                    .Where(p => p.FechaPago >= DateTime.Now.AddDays(-7))
                    .GroupBy(p => p.FechaPago.Date)
                    .Select(g => new
                    {
                        fecha = g.Key,
                        cantidad = g.Count(),
                        monto = g.Sum(p => p.Monto)
                    })
                    .OrderBy(x => x.fecha)
                    .ToList()
            };

            return Ok(estadisticas);
        }
    }

    // DTOs auxiliares
    public class ActualizarEstadoDTO
    {
        public string NuevoEstado { get; set; }
    }
}