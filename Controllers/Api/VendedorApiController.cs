using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VendedorApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILiberacionService _liberacionService;
        private readonly IFileService _fileService;

        public VendedorApiController(
            IUnitOfWork unitOfWork,
            ILiberacionService liberacionService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _liberacionService = liberacionService;
            _fileService = fileService;
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
                data = vendedores.Select(MapearVendedorResumen),
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

            return Ok(MapearVendedorDetalle(vendedor));
        }

        /// <summary>
        /// Obtener perfil del vendedor autenticado
        /// </summary>
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> GetMiPerfil()
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var vendedor = await _unitOfWork.VendedorRepository.GetByIdWithPersonaAsync(vendedorId.Value);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(MapearVendedorDetalle(vendedor));
        }

        // ==================== ✅ NUEVOS ENDPOINTS DE DATOS DE PAGO ====================

        /// <summary>
        /// ✅ NUEVO: Obtener datos de pago del vendedor
        /// </summary>
        [HttpGet("mi-perfil/datos-pago")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> GetMisDatosPago()
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(vendedorId.Value);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(new
            {
                aliasCBU = vendedor.AliasCBU,
                cbu = vendedor.CBU,
                banco = vendedor.Banco,
                titularCuenta = vendedor.TitularCuenta,
                cuit = vendedor.CUIT,
                imagenQR = vendedor.ImagenQR,
                urlImagenQR = !string.IsNullOrEmpty(vendedor.ImagenQR)
                    ? _fileService.GetFileUrl(vendedor.ImagenQR) : null,
                datosPagoCompletos = vendedor.DatosPagoCompletos,
                fechaDatosPago = vendedor.FechaDatosPago,
                puedePublicarTortas = vendedor.DatosPagoCompletos
            });
        }

        /// <summary>
        /// ✅ NUEVO: Actualizar datos de pago del vendedor
        /// </summary>
        [HttpPut("mi-perfil/datos-pago")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> ActualizarMisDatosPago([FromBody] ActualizarDatosPagoDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            // Validar CBU (22 dígitos)
            if (!string.IsNullOrEmpty(request.CBU) && request.CBU.Length != 22)
                return BadRequest(new { message = "El CBU debe tener 22 dígitos" });

            try
            {
                await _unitOfWork.VendedorRepository.ActualizarDatosPagoAsync(
                    vendedorId.Value,
                    request.AliasCBU,
                    request.CBU,
                    request.Banco,
                    request.TitularCuenta,
                    request.CUIT,
                    request.ImagenQR);

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Datos de pago actualizados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar datos de pago", error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Subir imagen QR de pago
        /// </summary>
        [HttpPost("mi-perfil/imagen-qr")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> SubirImagenQR([FromForm] IFormFile archivo)
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { message = "Debe enviar una imagen" });

            try
            {
                var rutaArchivo = await _fileService.SaveFileAsync(archivo, "qr-vendedores");

                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(vendedorId.Value);
                if (vendedor != null)
                {
                    vendedor.ImagenQR = rutaArchivo;
                    _unitOfWork.VendedorRepository.Update(vendedor);
                    await _unitOfWork.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    urlImagenQR = _fileService.GetFileUrl(rutaArchivo)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al subir imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Obtener resumen financiero del vendedor
        /// </summary>
        [HttpGet("mi-perfil/resumen-financiero")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> GetMiResumenFinanciero()
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var resumen = await _liberacionService.GetResumenVendedorAsync(vendedorId.Value);
            var comisionPorcentaje = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();

            return Ok(new
            {
                resumen.NombreComercial,
                resumen.TotalCobrado,
                resumen.TotalComisiones,
                resumen.PendienteCobro,
                resumen.CantidadLiberaciones,
                resumen.LiberacionesPendientes,
                resumen.UltimaLiberacion,
                comisionPorcentaje,
                mensaje = $"La plataforma retiene {comisionPorcentaje}% de comisión por cada venta"
            });
        }

        /// <summary>
        /// ✅ NUEVO: Obtener mis liberaciones de fondos
        /// </summary>
        [HttpGet("mi-perfil/liberaciones")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> GetMisLiberaciones([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var liberaciones = await _unitOfWork.Liberaciones.GetByVendedorIdAsync(vendedorId.Value);
            var total = liberaciones.Count();

            var liberacionesPaginadas = liberaciones
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(l => new
                {
                    id = l.Id,
                    ventaId = l.VentaId,
                    numeroOrden = l.Venta?.NumeroOrden,
                    montoBruto = l.MontoBruto,
                    comision = l.Comision,
                    montoNeto = l.MontoNeto,
                    porcentajeComision = l.PorcentajeComision,
                    estado = l.Estado.ToString(),
                    fechaCreacion = l.FechaCreacion,
                    fechaListoParaLiberar = l.FechaListoParaLiberar,
                    fechaTransferencia = l.FechaTransferencia,
                    fechaConfirmacion = l.FechaConfirmacion,
                    numeroOperacion = l.NumeroOperacion,
                    urlComprobante = !string.IsNullOrEmpty(l.ArchivoComprobante)
                        ? _fileService.GetFileUrl(l.ArchivoComprobante) : null,
                    puedeConfirmar = l.Estado == EstadoLiberacion.Transferido
                })
                .ToList();

            return Ok(new
            {
                data = liberacionesPaginadas,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// ✅ NUEVO: Confirmar recepción de fondos
        /// </summary>
        [HttpPost("liberaciones/{liberacionId}/confirmar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> ConfirmarRecepcionFondos(int liberacionId)
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var result = await _liberacionService.ConfirmarRecepcionVendedorAsync(liberacionId, vendedorId.Value);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// ✅ NUEVO: Verificar si puede publicar tortas
        /// </summary>
        [HttpGet("mi-perfil/puede-publicar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Vendedor")]
        public async Task<IActionResult> PuedePublicar()
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
                return NotFound(new { message = "No tienes un perfil de vendedor" });

            var puedePublicar = await _unitOfWork.VendedorRepository.PuedePublicarTortasAsync(vendedorId.Value);
            var tieneDatosPago = await _unitOfWork.VendedorRepository.TieneDatosPagoCompletosAsync(vendedorId.Value);

            return Ok(new
            {
                puedePublicar,
                tieneDatosPago,
                mensaje = !tieneDatosPago 
                    ? "Debe completar sus datos de pago antes de publicar tortas"
                    : "Puede publicar tortas"
            });
        }

        // ==================== ENDPOINTS EXISTENTES ====================

        /// <summary>
        /// Obtener estadísticas del vendedor
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> GetEstadisticasCompletas(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            var tortas = await _unitOfWork.TortaRepository.GetByVendedorIdAsync(id);
            var totalTortasActivas = tortas.Count(t => t.Disponible && t.Stock > 0);

            var detalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(id);
            var detallesCompletados = detalles.Where(d => d.Venta.Pagos.Any(p => 
                p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado));

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var detallesMes = detallesCompletados.Where(d => d.Venta.FechaVenta >= inicioMes);

            // ✅ NUEVO: Agregar datos financieros
            var resumenFinanciero = await _unitOfWork.VendedorRepository.GetResumenFinancieroAsync(id);

            return Ok(new
            {
                totalTortas = tortas.Count(),
                totalTortasActivas,
                totalVentas = detallesCompletados.Count(),
                ventasMes = detallesMes.Count(),
                ingresosBrutos = detallesCompletados.Sum(d => d.Subtotal),
                ingresosBrutosMes = detallesMes.Sum(d => d.Subtotal),
                pedidosPendientes = detalles.Count(d => d.Estado == EstadoDetalleVenta.Pendiente || 
                                                         d.Estado == EstadoDetalleVenta.EnPreparacion),
                // ✅ NUEVO
                totalCobrado = resumenFinanciero.TotalCobrado,
                totalComisiones = resumenFinanciero.TotalComisiones,
                pendienteCobro = resumenFinanciero.PendienteCobro
            });
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
        /// Obtener pedidos del vendedor
        /// </summary>
        [HttpGet("{id}/pedidos")]
        public async Task<IActionResult> GetPedidos(
            int id,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10,
            [FromQuery] string? estado = null)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            var detalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(id);

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDetalleVenta>(estado, out var estadoEnum))
                detalles = detalles.Where(d => d.Estado == estadoEnum);

            var total = detalles.Count();

            var pedidos = detalles
                .OrderByDescending(d => d.Venta.FechaVenta)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(d => new
                {
                    detalleId = d.Id,
                    ventaId = d.VentaId,
                    numeroOrden = d.Venta.NumeroOrden,
                    fecha = d.Venta.FechaVenta,
                    tortaId = d.TortaId,
                    nombreTorta = d.Torta.Nombre,
                    cantidad = d.Cantidad,
                    precioUnitario = d.PrecioUnitario,
                    subtotal = d.Subtotal,
                    notasPersonalizacion = d.NotasPersonalizacion,
                    estado = d.Estado.ToString(),
                    estadoLabel = GetEstadoLabel(d.Estado),
                    estadoPago = d.Venta.Pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault()?.Estado.ToString() ?? "SinPago",
                    compradorNombre = d.Venta.Comprador.Persona.Nombre,
                    direccionEntrega = d.Venta.DireccionEntrega,
                    accionesDisponibles = GetAccionesDisponibles(d.Estado)
                })
                .ToList();

            return Ok(new
            {
                data = pedidos,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        // ==================== CRUD EXISTENTE ====================

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Create([FromBody] VendedorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                FechaCreacion = DateTime.Now,
                TotalVentas = 0,
                Activo = true
            };

            await _unitOfWork.VendedorRepository.AddAsync(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vendedor.Id }, vendedor);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Update(int id, [FromBody] VendedorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            vendedor.NombreComercial = dto.NombreComercial;
            vendedor.Especialidad = dto.Especialidad;
            vendedor.Descripcion = dto.Descripcion;

            _unitOfWork.VendedorRepository.Update(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(vendedor);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            _unitOfWork.VendedorRepository.Delete(vendedor);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // ==================== HELPERS ====================

        private object MapearVendedorResumen(Vendedor v) => new
        {
            id = v.Id,
            personaId = v.PersonaId,
            nombreComercial = v.NombreComercial,
            especialidad = v.Especialidad,
            calificacion = v.Calificacion,
            activo = v.Activo,
            datosPagoCompletos = v.DatosPagoCompletos
        };

        private object MapearVendedorDetalle(Vendedor v) => new
        {
            id = v.Id,
            personaId = v.PersonaId,
            nombreComercial = v.NombreComercial,
            especialidad = v.Especialidad,
            descripcion = v.Descripcion,
            avatar = v.Persona?.Avatar,
            calificacion = v.Calificacion,
            totalVentas = v.TotalVentas,
            activo = v.Activo,
            verificado = v.Verificado,
            datosPagoCompletos = v.DatosPagoCompletos,
            totalCobrado = v.TotalCobrado,
            pendienteCobro = v.PendienteCobro
        };

        private async Task<int?> ObtenerVendedorIdActual()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var personaId))
                return null;

            var vendedor = await _unitOfWork.VendedorRepository.GetByPersonaIdAsync(personaId);
            return vendedor?.Id;
        }

        private static string GetEstadoLabel(EstadoDetalleVenta estado) => estado switch
        {
            EstadoDetalleVenta.Pendiente => "⏳ Pendiente",
            EstadoDetalleVenta.Confirmado => "✅ Confirmado",
            EstadoDetalleVenta.EnPreparacion => "👨‍🍳 En Preparación",
            EstadoDetalleVenta.Listo => "📦 Listo para Retirar",
            EstadoDetalleVenta.Entregado => "🎉 Entregado",
            EstadoDetalleVenta.Cancelado => "❌ Cancelado",
            _ => estado.ToString()
        };

        private static List<string> GetAccionesDisponibles(EstadoDetalleVenta estado) => estado switch
        {
            EstadoDetalleVenta.Pendiente => new List<string> { "Confirmado", "Cancelado" },
            EstadoDetalleVenta.Confirmado => new List<string> { "EnPreparacion", "Cancelado" },
            EstadoDetalleVenta.EnPreparacion => new List<string> { "Listo" },
            EstadoDetalleVenta.Listo => new List<string> { "Entregado" },
            _ => new List<string>()
        };
    }

    // ==================== DTOs ====================

    public class ActualizarDatosPagoDTO
    {
        [Required(ErrorMessage = "El alias es requerido")]
        [StringLength(50)]
        public string AliasCBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CBU es requerido")]
        [StringLength(22, MinimumLength = 22, ErrorMessage = "El CBU debe tener 22 dígitos")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU solo debe contener números")]
        public string CBU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El banco es requerido")]
        [StringLength(100)]
        public string Banco { get; set; } = string.Empty;

        [Required(ErrorMessage = "El titular es requerido")]
        [StringLength(200)]
        public string TitularCuenta { get; set; } = string.Empty;

        [StringLength(13)]
        public string? CUIT { get; set; }

        public string? ImagenQR { get; set; }
    }
}