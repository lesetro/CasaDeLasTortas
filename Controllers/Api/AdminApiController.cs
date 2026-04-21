using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    /// <summary>
    /// API completa del Administrador.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;
        private readonly ILiberacionService _liberacionService;
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(
            IUnitOfWork unitOfWork,
            IPagoService pagoService,
            ILiberacionService liberacionService,
            ILogger<AdminApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
            _liberacionService = liberacionService;
            _logger = logger;
        }

        // ══════════════════════════════════════════════
        // DASHBOARD (MODIFICADO)
        // ══════════════════════════════════════════════

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();
                var vendedores = await _unitOfWork.VendedorRepository.GetAllAsync();
                var compradores = await _unitOfWork.CompradorRepository.GetAllAsync();
                var tortas = await _unitOfWork.TortaRepository.GetAllAsync();
                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();
                var topTortas = await _unitOfWork.TortaRepository.GetTopVendidasAsync(5);

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var pagosMes = pagos.Where(p => p.FechaPago >= inicioMes && 
                    (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado));

                // Estadísticas de pagos pendientes
                var pagosEnRevision = await _unitOfWork.PagoRepository.CountEnRevisionAsync();
                var liberacionesPendientes = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.ListoParaLiberar);
                var disputasAbiertas = await _unitOfWork.Disputas.CountAbiertasAsync();
                var comisionesDelMes = await _unitOfWork.PagoRepository.GetComisionesDelMesAsync();

                // Ventas e ingresos de los últimos 7 días
                var ventasPorDia = new Dictionary<string, int>();
                var ingresosPorDia = new Dictionary<string, decimal>();

                for (int i = 6; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddDays(-i).Date;
                    var etiqueta = fecha.ToString("dd/MM");
                    var pagosDelDia = pagos.Where(p => p.FechaPago.Date == fecha && 
                        (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado));
                    ventasPorDia[etiqueta] = pagosDelDia.Count();
                    ingresosPorDia[etiqueta] = pagosDelDia.Sum(p => p.Monto);
                }

                return Ok(new
                {
                    totales = new
                    {
                        personas = personas.Count(),
                        vendedores = vendedores.Count(),
                        compradores = compradores.Count(),
                        tortas = tortas.Count(),
                        tortasActivas = tortas.Count(t => t.Disponible && t.Stock > 0)
                    },

                    ventas = new
                    {
                        totalPagos = pagos.Count(),
                        ventasMes = pagosMes.Count(),
                        ingresosMes = pagosMes.Sum(p => p.Monto),
                        ingresosTotales = pagos.Where(p => p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado).Sum(p => p.Monto),
                        pagosPendientes = pagos.Count(p => p.Estado == EstadoPago.Pendiente)
                    },

                    // : Panel de acciones pendientes
                    accionesPendientes = new
                    {
                        pagosEnRevision,
                        liberacionesPendientes,
                        disputasAbiertas,
                        comisionesDelMes
                    },

                    ultimosVendedores = vendedores
                        .OrderByDescending(v => v.FechaCreacion)
                        .Take(5)
                        .Select(v => new
                        {
                            id = v.Id,
                            nombreComercial = v.NombreComercial,
                            especialidad = v.Especialidad,
                            verificado = v.Verificado,
                            datosPagoCompletos = v.DatosPagoCompletos,
                            totalVentas = v.TotalVentas,
                            fechaCreacion = v.FechaCreacion
                        }),

                    ultimosCompradores = compradores
                        .OrderByDescending(c => c.FechaCreacion)
                        .Take(5)
                        .Select(c => new
                        {
                            id = c.Id,
                            nombre = c.Persona?.Nombre,
                            email = c.Persona?.Email,
                            totalCompras = c.TotalCompras,
                            fechaCreacion = c.FechaCreacion
                        }),

                    ultimosPagos = pagos
                        .OrderByDescending(p => p.FechaPago)
                        .Take(10)
                        .Select(p => new
                        {
                            id = p.Id,
                            monto = p.Monto,
                            estado = p.Estado.ToString(),
                            metodoPago = p.MetodoPago?.ToString(),
                            fechaPago = p.FechaPago,
                            numeroOrden = p.Venta?.NumeroOrden,
                            comprador = p.Venta?.Comprador?.Persona?.Nombre,
                            tieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante)
                        }),

                    topVendedores = vendedores
                        .OrderByDescending(v => v.TotalVentas)
                        .Take(5)
                        .Select(v => new
                        {
                            id = v.Id,
                            nombreComercial = v.NombreComercial,
                            totalVentas = v.TotalVentas,
                            verificado = v.Verificado,
                            totalCobrado = v.TotalCobrado,
                            pendienteCobro = v.PendienteCobro
                        }),

                    topTortas = topTortas.Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre,
                        precio = t.Precio,
                        imagen = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    }),

                    graficos = new
                    {
                        ventasPorDia,
                        ingresosPorDia
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dashboard admin");
                return StatusCode(500, new { message = "Error al cargar el dashboard" });
            }
        }

        // ══════════════════════════════════════════════
        // : RESUMEN FINANCIERO
        // ══════════════════════════════════════════════

        [HttpGet("resumen-financiero")]
        public async Task<IActionResult> GetResumenFinanciero()
        {
            try
            {
                var pagosStats = await _pagoService.GetEstadisticasAsync();
                var liberacionesStats = await _liberacionService.GetEstadisticasAsync();
                var comision = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();

                return Ok(new
                {
                    pagos = pagosStats,
                    liberaciones = liberacionesStats,
                    comisionPorcentaje = comision,
                    resumen = new
                    {
                        ingresosBrutos = pagosStats.MontoTotalRecibido,
                        comisionesGeneradas = await _unitOfWork.PagoRepository.GetTotalComisionesAsync(),
                        montoLiberadoVendedores = liberacionesStats.MontoTotalLiberado,
                        montoPendienteLiberacion = liberacionesStats.MontoPendienteLiberacion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen financiero");
                return StatusCode(500, new { message = "Error al cargar el resumen financiero" });
            }
        }

        // ══════════════════════════════════════════════
        // USUARIOS (sin cambios significativos)
        // ══════════════════════════════════════════════

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 10,
            [FromQuery] string? busqueda = null,
            [FromQuery] string? rol = null,
            [FromQuery] bool? activo = null)
        {
            try
            {
                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(busqueda))
                    personas = personas.Where(p =>
                        p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(rol))
                {
                    personas = rol switch
                    {
                        "Admin" => personas.Where(p => p.Rol == "Admin"),
                        "Vendedor" => personas.Where(p => p.Vendedor != null),
                        "Comprador" => personas.Where(p => p.Comprador != null),
                        _ => personas
                    };
                }

                if (activo.HasValue)
                    personas = personas.Where(p => p.Activo == activo.Value);

                var total = personas.Count();
                var usuarios = personas
                    .OrderByDescending(p => p.FechaRegistro)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.Nombre,
                        email = p.Email,
                        telefono = p.Telefono,
                        rol = p.Rol == "Admin" ? "Admin" :
                              p.Vendedor != null ? "Vendedor" :
                              p.Comprador != null ? "Comprador" : "Sin rol",
                        activo = p.Activo,
                        fechaRegistro = p.FechaRegistro,
                        avatar = p.Avatar,
                        vendedorId = p.Vendedor?.Id,
                        compradorId = p.Comprador?.Id,
                        datosPagoCompletos = p.Vendedor?.DatosPagoCompletos ?? false
                    })
                    .ToList();

                return Ok(new
                {
                    data = usuarios,
                    pagina,
                    tamanioPagina,
                    totalRegistros = total,
                    totalPaginas = (int)Math.Ceiling((double)total / tamanioPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios admin");
                return StatusCode(500, new { message = "Error al obtener usuarios" });
            }
        }

        // ══════════════════════════════════════════════
        // VENDEDORES 
        // ══════════════════════════════════════════════

        [HttpGet("vendedores")]
        public async Task<IActionResult> GetVendedores(
            [FromQuery] bool? verificado = null,
            [FromQuery] bool? datosPagoCompletos = null)
        {
            try
            {
                var vendedores = await _unitOfWork.VendedorRepository.GetAllWithPersonaAsync();

                if (verificado.HasValue)
                    vendedores = vendedores.Where(v => v.Verificado == verificado.Value);

                // : Filtro por datos de pago
                if (datosPagoCompletos.HasValue)
                    vendedores = vendedores.Where(v => v.DatosPagoCompletos == datosPagoCompletos.Value);

                return Ok(vendedores.Select(v => new
                {
                    id = v.Id,
                    nombreComercial = v.NombreComercial,
                    especialidad = v.Especialidad,
                    verificado = v.Verificado,
                    activo = v.Activo,
                    totalVentas = v.TotalVentas,
                    fechaCreacion = v.FechaCreacion,
                    nombre = v.Persona?.Nombre,
                    email = v.Persona?.Email,
                    // 
                    datosPagoCompletos = v.DatosPagoCompletos,
                    aliasCBU = v.AliasCBU,
                    banco = v.Banco,
                    totalCobrado = v.TotalCobrado,
                    totalComisiones = v.TotalComisiones,
                    pendienteCobro = v.PendienteCobro
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo vendedores admin");
                return StatusCode(500, new { message = "Error al obtener vendedores" });
            }
        }

        [HttpPatch("vendedores/{id}/verificar")]
        public async Task<IActionResult> VerificarVendedor(int id)
        {
            try
            {
                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(id);
                if (vendedor == null)
                    return NotFound(new { message = "Vendedor no encontrado" });

                vendedor.Verificado = !vendedor.Verificado;
                await _unitOfWork.VendedorRepository.UpdateAsync(vendedor);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Vendedor {(vendedor.Verificado ? "verificado" : "des-verificado")}",
                    verificado = vendedor.Verificado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando vendedor {Id}", id);
                return StatusCode(500, new { message = "Error al verificar el vendedor" });
            }
        }

        //  Ver datos de pago de un vendedor
        [HttpGet("vendedores/{id}/datos-pago")]
        public async Task<IActionResult> GetDatosPagoVendedor(int id)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdWithPersonaAsync(id);
            if (vendedor == null)
                return NotFound(new { message = "Vendedor no encontrado" });

            return Ok(new
            {
                vendedorId = vendedor.Id,
                nombreComercial = vendedor.NombreComercial,
                nombre = vendedor.Persona?.Nombre,
                email = vendedor.Persona?.Email,
                aliasCBU = vendedor.AliasCBU,
                cbu = vendedor.CBU,
                banco = vendedor.Banco,
                titularCuenta = vendedor.TitularCuenta,
                cuit = vendedor.CUIT,
                datosPagoCompletos = vendedor.DatosPagoCompletos,
                fechaDatosPago = vendedor.FechaDatosPago,
                totalCobrado = vendedor.TotalCobrado,
                totalComisiones = vendedor.TotalComisiones,
                pendienteCobro = vendedor.PendienteCobro
            });
        }

        // ══════════════════════════════════════════════
        // VENTAS 
        // ══════════════════════════════════════════════

        [HttpGet("ventas")]
        public async Task<IActionResult> GetVentas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 10,
            [FromQuery] string? estado = null)
        {
            try
            {
                var ventas = await _unitOfWork.Ventas.GetAllAsync();

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoVenta>(estado, out var estadoEnum))
                    ventas = ventas.Where(v => v.Estado == estadoEnum);

                var total = ventas.Count();
                var ventasPaginadas = ventas
                    .OrderByDescending(v => v.FechaVenta)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .Select(v => new
                    {
                        id = v.Id,
                        numeroOrden = v.NumeroOrden,
                        fechaVenta = v.FechaVenta,
                        total = v.Total,
                        estado = v.Estado.ToString(),
                        compradorId = v.CompradorId,
                        comprador = v.Comprador?.Persona?.Nombre,
                        cantidadItems = v.Detalles?.Sum(d => d.Cantidad) ?? 0,
                        comisionPlataforma = v.ComisionPlataforma,
                        fondosLiberados = v.FondosLiberados
                    })
                    .ToList();

                return Ok(new
                {
                    data = ventasPaginadas,
                    pagina,
                    tamanioPagina,
                    totalRegistros = total,
                    totalPaginas = (int)Math.Ceiling((double)total / tamanioPagina)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ventas admin");
                return StatusCode(500, new { message = "Error al obtener ventas" });
            }
        }

        [HttpPatch("ventas/{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoVenta(int id, [FromBody] ActualizarVentaRequest request)
        {
            if (!Enum.TryParse<EstadoVenta>(request.Estado, out var estadoEnum))
                return BadRequest(new { message = "Estado inválido" });

            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdAsync(id);
                if (venta == null)
                    return NotFound(new { message = "Venta no encontrada" });

                venta.Estado = estadoEnum;
                venta.FechaActualizacion = DateTime.Now;
                if (!string.IsNullOrEmpty(request.Notas))
                    venta.NotasInternas = request.Notas;

                _unitOfWork.Ventas.Update(venta);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Venta actualizada a '{estadoEnum}'",
                    estado = estadoEnum.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando venta {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la venta" });
            }
        }

        // ══════════════════════════════════════════════
        // REPORTES 
        // ══════════════════════════════════════════════

        [HttpGet("reportes")]
        public async Task<IActionResult> GetReportes(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
                var fin = fechaFin ?? DateTime.Now;

                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();
                var pagosEnRango = pagos
                    .Where(p => p.FechaPago >= inicio && p.FechaPago <= fin.AddDays(1) && 
                           (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado))
                    .ToList();

                // Ventas por vendedor
                var ventasPorVendedor = pagosEnRango
                    .SelectMany(p => p.Venta.Detalles)
                    .GroupBy(d => d.VendedorId)
                    .Select(g => new
                    {
                        vendedorId = g.Key,
                        nombreVendedor = g.First().Vendedor?.NombreComercial ?? "N/A",
                        cantidadVentas = g.Select(d => d.VentaId).Distinct().Count(),
                        totalIngresos = g.Sum(d => d.Subtotal),
                        productosVendidos = g.Sum(d => d.Cantidad)
                    })
                    .OrderByDescending(x => x.totalIngresos);

                // Productos más vendidos
                var productosMasVendidos = pagosEnRango
                    .SelectMany(p => p.Venta.Detalles)
                    .GroupBy(d => d.TortaId)
                    .Select(g => new
                    {
                        tortaId = g.Key,
                        nombreTorta = g.First().Torta?.Nombre ?? "N/A",
                        cantidadVendida = g.Sum(d => d.Cantidad),
                        totalIngresos = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.cantidadVendida)
                    .Take(10);

                // Ventas por día
                var ventasPorDia = pagosEnRango
                    .GroupBy(p => p.FechaPago.Date)
                    .Select(g => new
                    {
                        fecha = g.Key.ToString("yyyy-MM-dd"),
                        cantidadVentas = g.Count(),
                        ingresos = g.Sum(p => p.Monto),
                        comisiones = g.Sum(p => p.ComisionPlataforma)
                    })
                    .OrderBy(x => x.fecha);

                return Ok(new
                {
                    periodo = new { inicio, fin },
                    resumen = new
                    {
                        totalVentas = pagosEnRango.Count,
                        totalIngresos = pagosEnRango.Sum(p => p.Monto),
                        totalComisiones = pagosEnRango.Sum(p => p.ComisionPlataforma),
                        promedioVenta = pagosEnRango.Any() ? pagosEnRango.Average(p => p.Monto) : 0
                    },
                    ventasPorVendedor,
                    productosMasVendidos,
                    ventasPorDia
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte admin");
                return StatusCode(500, new { message = "Error al generar el reporte" });
            }
        }
    }

}