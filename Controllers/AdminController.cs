using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Controllers
{
    /// <summary>
    /// Controller de Administración - Acceso restringido a rol Admin
    /// ✅ MODIFICADO: Incluye gestión de pagos, liberaciones y disputas
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;
        private readonly ILiberacionService _liberacionService;
        private readonly IFileService _fileService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUnitOfWork unitOfWork,
            IPagoService pagoService,
            ILiberacionService liberacionService,
            IFileService fileService,
            ILogger<AdminController> logger)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
            _liberacionService = liberacionService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard principal del administrador (MODIFICADO)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();
                var vendedores = await _unitOfWork.VendedorRepository.GetAllAsync();
                var compradores = await _unitOfWork.CompradorRepository.GetAllAsync();
                var tortas = await _unitOfWork.TortaRepository.GetAllAsync();
                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var hoy = DateTime.Today;

                var pagosCompletados = pagos.Where(p => p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado);
                var pagosMes = pagosCompletados.Where(p => p.FechaPago >= inicioMes);
                var pagosSemana = pagosCompletados.Where(p => p.FechaPago >= inicioSemana);
                var pagosHoy = pagosCompletados.Where(p => p.FechaPago.Date == hoy);

                // Acciones pendientes
                var pagosEnRevision = await _unitOfWork.PagoRepository.CountEnRevisionAsync();
                var liberacionesPendientesCount = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.ListoParaLiberar);
                var disputasAbiertasCount = await _unitOfWork.Disputas.CountAbiertasAsync();
                var vendedoresPendientesCount = vendedores.Count(v => !v.Verificado && v.Activo);

                // Datos para gráficos
                var ventasPorDiaList = new List<DatoGraficoDTO>();
                var ingresosPorDiaList = new List<DatoGraficoDTO>();

                for (int i = 6; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddDays(-i).Date;
                    var pagosDelDia = pagosCompletados.Where(p => p.FechaPago.Date == fecha);
                    ventasPorDiaList.Add(new DatoGraficoDTO { Etiqueta = fecha.ToString("dd/MM"), Valor = pagosDelDia.Count() });
                    ingresosPorDiaList.Add(new DatoGraficoDTO { Etiqueta = fecha.ToString("dd/MM"), Valor = pagosDelDia.Sum(p => p.Monto) });
                }

                // Top vendedores
                var topVendedoresList = vendedores
                    .OrderByDescending(v => v.TotalVentas)
                    .Take(5)
                    .Select((v, index) => new TopVendedorDTO
                    {
                        Id = v.Id,
                        NombreComercial = v.NombreComercial,
                        Avatar = v.Persona?.Avatar,
                        TotalVentas = v.TotalVentas,
                        Calificacion = v.Calificacion,
                        Verificado = v.Verificado,
                        Posicion = index + 1
                    }).ToList();

                var viewModel = new AdminDashboardViewModel
                {
                    // Resumen general
                    Resumen = new ResumenGeneralViewModel
                    {
                        // Hoy
                        VentasHoy = pagosHoy.Count(),
                        IngresosHoy = pagosHoy.Sum(p => p.Monto),
                        ComisionesHoy = pagosHoy.Sum(p => p.ComisionPlataforma),
                        NuevosUsuariosHoy = personas.Count(p => p.FechaRegistro.Date == hoy),

                        // Semana
                        VentasSemana = pagosSemana.Count(),
                        IngresosSemana = pagosSemana.Sum(p => p.Monto),
                        ComisionesSemana = pagosSemana.Sum(p => p.ComisionPlataforma),

                        // Mes
                        VentasMes = pagosMes.Count(),
                        IngresosMes = pagosMes.Sum(p => p.Monto),
                        ComisionesMes = pagosMes.Sum(p => p.ComisionPlataforma),

                        // Totales
                        TotalVentas = pagosCompletados.Count(),
                        TotalIngresos = pagosCompletados.Sum(p => p.Monto),
                        TotalComisiones = pagosCompletados.Sum(p => p.ComisionPlataforma),

                        // Usuarios
                        TotalUsuarios = personas.Count(),
                        TotalCompradores = compradores.Count(),
                        TotalVendedores = vendedores.Count(),
                        VendedoresActivos = vendedores.Count(v => v.Activo),
                        VendedoresVerificados = vendedores.Count(v => v.Verificado),

                        // Productos
                        TotalTortas = tortas.Count(),
                        TortasDisponibles = tortas.Count(t => t.Disponible && t.Stock > 0)
                    },

                    // Acciones pendientes (contadores)
                    TotalPagosPendientes = pagosEnRevision,
                    TotalLiberacionesPendientes = liberacionesPendientesCount,
                    TotalDisputasAbiertas = disputasAbiertasCount,
                    TotalVendedoresPendientes = vendedoresPendientesCount,

                    // Gráficos
                    VentasPorDia = ventasPorDiaList,
                    IngresosPorDia = ingresosPorDiaList,

                    // Top vendedores
                    TopVendedores = topVendedoresList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard de admin");
                TempData["Error"] = "Error al cargar el dashboard";
                return View(new AdminDashboardViewModel());
            }
        }


        // ══════════════════════════════════════════════
        // ✅ NUEVO: GESTIÓN DE PAGOS
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de gestión de pagos
        /// </summary>
        public async Task<IActionResult> Pagos(string estado = "", int pagina = 1)
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPago>(estado, out var estadoEnum))
                    pagos = pagos.Where(p => p.Estado == estadoEnum);

                var totalItems = pagos.Count();
                var tamanioPagina = 15;
                var pagosPaginados = pagos
                    .OrderByDescending(p => p.FechaPago)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                ViewBag.FiltroEstado = estado;
                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina);
                ViewBag.Estadisticas = await _pagoService.GetEstadisticasAsync();

                return View(pagosPaginados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pagos");
                TempData["Error"] = "Error al cargar los pagos";
                return View(new List<Pago>());
            }
        }

        /// <summary>
        /// Ver detalle de un pago para verificación
        /// </summary>
        public async Task<IActionResult> VerificarPago(int id)
        {
            var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
            if (pago == null)
            {
                TempData["Error"] = "Pago no encontrado";
                return RedirectToAction(nameof(Pagos));
            }

            ViewBag.UrlComprobante = !string.IsNullOrEmpty(pago.ArchivoComprobante)
                ? _fileService.GetFileUrl(pago.ArchivoComprobante) : null;

            return View(pago);
        }

        /// <summary>
        /// Procesar verificación de pago
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarVerificacion(int pagoId, bool aprobado, string? observaciones, string? motivoRechazo)
        {
            try
            {
                var adminEmail = User.Identity?.Name;
                var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(adminEmail ?? "");
                if (persona == null)
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Pagos));
                }

                var result = await _pagoService.VerificarPagoAsync(pagoId, persona.Id, aprobado, observaciones, motivoRechazo);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Pagos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar pago {PagoId}", pagoId);
                TempData["Error"] = "Error al procesar la verificación";
                return RedirectToAction(nameof(Pagos));
            }
        }

        // ══════════════════════════════════════════════
        // ✅ NUEVO: GESTIÓN DE LIBERACIONES
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de gestión de liberaciones de fondos
        /// </summary>
        public async Task<IActionResult> Liberaciones(string estado = "", int pagina = 1)
        {
            try
            {
                IEnumerable<LiberacionFondos> liberaciones;

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoLiberacion>(estado, out var estadoEnum))
                    liberaciones = await _unitOfWork.Liberaciones.GetByEstadoAsync(estadoEnum);
                else
                    liberaciones = await _unitOfWork.Liberaciones.GetAllAsync(pagina, 20);

                var totalItems = await _unitOfWork.Liberaciones.CountAsync();
                var tamanioPagina = 20;

                ViewBag.FiltroEstado = estado;
                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina);
                ViewBag.Estadisticas = await _liberacionService.GetEstadisticasAsync();
                ViewBag.PendientesLiberar = await _liberacionService.GetLiberacionesPendientesAsync();

                return View(liberaciones.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar liberaciones");
                TempData["Error"] = "Error al cargar las liberaciones";
                return View(new List<LiberacionFondos>());
            }
        }

        /// <summary>
        /// Ver detalle de liberación para procesar transferencia
        /// </summary>
        public async Task<IActionResult> ProcesarLiberacion(int id)
        {
            var liberacion = await _unitOfWork.Liberaciones.GetByIdWithDetallesAsync(id);
            if (liberacion == null)
            {
                TempData["Error"] = "Liberación no encontrada";
                return RedirectToAction(nameof(Liberaciones));
            }

            return View(liberacion);
        }

        /// <summary>
        /// Registrar transferencia a vendedor
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarTransferencia(int liberacionId, string numeroOperacion, IFormFile comprobante)
        {
            try
            {
                if (comprobante == null || comprobante.Length == 0)
                {
                    TempData["Error"] = "Debe adjuntar comprobante de transferencia";
                    return RedirectToAction(nameof(ProcesarLiberacion), new { id = liberacionId });
                }

                var adminEmail = User.Identity?.Name;
                var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(adminEmail ?? "");
                if (persona == null)
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Liberaciones));
                }

                var archivoComprobante = await _fileService.SaveFileAsync(comprobante, "liberaciones");
                var result = await _liberacionService.ProcesarLiberacionAsync(
                    liberacionId, persona.Id, numeroOperacion, archivoComprobante);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Liberaciones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar transferencia para liberación {Id}", liberacionId);
                TempData["Error"] = "Error al registrar la transferencia";
                return RedirectToAction(nameof(Liberaciones));
            }
        }

        // ══════════════════════════════════════════════
        // ✅ NUEVO: GESTIÓN DE DISPUTAS
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de gestión de disputas
        /// </summary>
        public async Task<IActionResult> Disputas(string estado = "", int pagina = 1)
        {
            try
            {
                IEnumerable<Disputa> disputas;

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDisputa>(estado, out var estadoEnum))
                    disputas = await _unitOfWork.Disputas.GetByEstadoAsync(estadoEnum);
                else
                    disputas = await _unitOfWork.Disputas.GetAllAsync(pagina, 20);

                var totalItems = await _unitOfWork.Disputas.CountAsync();
                var abiertas = await _unitOfWork.Disputas.CountAbiertasAsync();

                ViewBag.FiltroEstado = estado;
                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, 20);
                ViewBag.DisputasAbiertas = abiertas;
                ViewBag.SinAsignar = (await _unitOfWork.Disputas.GetSinAsignarAsync()).Count();

                return View(disputas.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar disputas");
                TempData["Error"] = "Error al cargar las disputas";
                return View(new List<Disputa>());
            }
        }

        /// <summary>
        /// Ver detalle de disputa
        /// </summary>
        public async Task<IActionResult> VerDisputa(int id)
        {
            var disputa = await _unitOfWork.Disputas.GetByIdWithMensajesAsync(id);
            if (disputa == null)
            {
                TempData["Error"] = "Disputa no encontrada";
                return RedirectToAction(nameof(Disputas));
            }

            return View(disputa);
        }

        /// <summary>
        /// Asignarse una disputa
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarDisputa(int disputaId)
        {
            try
            {
                var adminEmail = User.Identity?.Name;
                var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(adminEmail ?? "");
                if (persona == null)
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Disputas));
                }

                await _unitOfWork.Disputas.AsignarAdminAsync(disputaId, persona.Id);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Disputa asignada correctamente";
                return RedirectToAction(nameof(VerDisputa), new { id = disputaId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar disputa {Id}", disputaId);
                TempData["Error"] = "Error al asignar la disputa";
                return RedirectToAction(nameof(Disputas));
            }
        }

        /// <summary>
        /// Resolver disputa
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolverDisputa(int disputaId, ResolucionDisputa resolucion, string detalleResolucion, decimal? montoResolucion)
        {
            try
            {
                await _unitOfWork.Disputas.ResolverAsync(disputaId, resolucion, detalleResolucion, montoResolucion);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Disputa resuelta correctamente";
                return RedirectToAction(nameof(Disputas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver disputa {Id}", disputaId);
                TempData["Error"] = "Error al resolver la disputa";
                return RedirectToAction(nameof(VerDisputa), new { id = disputaId });
            }
        }

        // ══════════════════════════════════════════════
        // ✅ NUEVO: CONFIGURACIÓN DE PLATAFORMA
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de configuración de la plataforma
        /// </summary>
        public async Task<IActionResult> Configuracion()
        {
            try
            {
                var config = await _unitOfWork.Configuracion.GetOrCreateAsync();
                ViewBag.UrlImagenQR = !string.IsNullOrEmpty(config.ImagenQR)
                    ? _fileService.GetFileUrl(config.ImagenQR) : null;
                return View(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración");
                TempData["Error"] = "Error al cargar la configuración";
                return View(new ConfiguracionPlataforma());
            }
        }

        /// <summary>
        /// Guardar configuración
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionPlataforma config, IFormFile? imagenQR)
        {
            try
            {
                var configActual = await _unitOfWork.Configuracion.GetOrCreateAsync();

                // Actualizar campos
                configActual.NombrePlataforma = config.NombrePlataforma;
                configActual.AliasCBU = config.AliasCBU;
                configActual.CBU = config.CBU;
                configActual.Banco = config.Banco;
                configActual.TitularCuenta = config.TitularCuenta;
                configActual.CUIT = config.CUIT;
                configActual.ComisionPorcentaje = config.ComisionPorcentaje;
                configActual.DiasParaLiberarFondos = config.DiasParaLiberarFondos;
                configActual.MaxIntentosRechazados = config.MaxIntentosRechazados;
                configActual.DiasLimitePago = config.DiasLimitePago;
                configActual.InstruccionesPago = config.InstruccionesPago;
                configActual.PlataformaActiva = config.PlataformaActiva;
                configActual.EmailNotificaciones = config.EmailNotificaciones;
                configActual.TelefonoContacto = config.TelefonoContacto;

                // Imagen QR
                if (imagenQR != null && imagenQR.Length > 0)
                {
                    configActual.ImagenQR = await _fileService.SaveFileAsync(imagenQR, "qr-plataforma");
                }

                await _unitOfWork.Configuracion.UpdateAsync(configActual);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Configuración guardada correctamente";
                return RedirectToAction(nameof(Configuracion));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración");
                TempData["Error"] = "Error al guardar la configuración";
                return RedirectToAction(nameof(Configuracion));
            }
        }

        // ══════════════════════════════════════════════
        // MÉTODOS EXISTENTES (sin cambios)
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Usuarios(int pagina = 1, string busqueda = "", string filtroRol = "", bool? activo = null)
        {
            try
            {
                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(busqueda))
                    personas = personas.Where(p =>
                        p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(filtroRol))
                {
                    personas = filtroRol switch
                    {
                        "Vendedor" => personas.Where(p => p.Vendedor != null),
                        "Comprador" => personas.Where(p => p.Comprador != null),
                        "Admin" => personas.Where(p => p.Rol == "Admin"),
                        "SinRol" => personas.Where(p => p.Vendedor == null && p.Comprador == null && p.Rol != "Admin"),
                        _ => personas
                    };
                }

                if (activo.HasValue)
                    personas = personas.Where(p => p.Activo == activo.Value);

                var totalItems = personas.Count();
                var tamanioPagina = 20;
                var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamanioPagina);

                var personasPaginadas = personas
                    .OrderByDescending(p => p.FechaRegistro)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Convertir Persona a UsuarioListaDTO
                var usuariosDTO = personasPaginadas.Select(p => new UsuarioListaDTO
                {
                    PersonaId = p.Id,
                    VendedorId = p.Vendedor?.Id,
                    CompradorId = p.Comprador?.Id,
                    Nombre = p.Nombre,
                    NombreCompleto = p.NombreCompleto,
                    Email = p.Email,
                    Avatar = p.Avatar,
                    Rol = p.Rol == "Admin" ? "Admin"
                        : p.Vendedor != null ? "Vendedor"
                        : p.Comprador != null ? "Comprador"
                        : "SinRol",
                    NombreComercial = p.Vendedor?.NombreComercial,
                    Activo = p.Activo && (p.Vendedor?.Activo ?? true) && (p.Comprador?.Activo ?? true),
                    Verificado = p.Vendedor?.Verificado ?? false,
                    FechaRegistro = p.FechaRegistro,
                    UltimoAcceso = p.UltimoAcceso
                }).ToList();

                // Estadísticas
                var todasPersonas = await _unitOfWork.PersonaRepository.GetAllAsync();
                var estadisticas = new EstadisticasUsuariosDTO
                {
                    TotalUsuarios = todasPersonas.Count(),
                    TotalCompradores = todasPersonas.Count(p => p.Comprador != null),
                    TotalVendedores = todasPersonas.Count(p => p.Vendedor != null),
                    TotalAdmins = todasPersonas.Count(p => p.Rol == "Admin"),
                    UsuariosActivos = todasPersonas.Count(p => p.Activo),
                    UsuariosInactivos = todasPersonas.Count(p => !p.Activo),
                    NuevosHoy = todasPersonas.Count(p => p.FechaRegistro.Date == DateTime.Today),
                    NuevosEstaSemana = todasPersonas.Count(p => p.FechaRegistro >= DateTime.Today.AddDays(-7))
                };

                var viewModel = new AdminUsuariosViewModel
                {
                    Usuarios = usuariosDTO,
                    Estadisticas = estadisticas,
                    Busqueda = busqueda,
                    FiltroRol = filtroRol,
                    FiltroActivo = activo,
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalRegistros = totalItems,
                    TamanioPagina = tamanioPagina
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios");
                TempData["Error"] = "Error al cargar los usuarios";
                return View(new AdminUsuariosViewModel());
            }
        }

        public async Task<IActionResult> Ventas(int pagina = 1, string estado = "")
        {
            try
            {
                var ventas = await _unitOfWork.Ventas.GetAllAsync();

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoVenta>(estado, out var estadoEnum))
                    ventas = ventas.Where(v => v.Estado == estadoEnum);

                var totalItems = ventas.Count();
                var tamanioPagina = 15;
                var ventasPaginadas = ventas
                    .OrderByDescending(v => v.FechaVenta)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                ViewBag.FiltroEstado = estado;
                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina);

                return View(ventasPaginadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar ventas");
                TempData["Error"] = "Error al cargar las ventas";
                return View(new List<Venta>());
            }
        }

        public async Task<IActionResult> Reportes(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
                var fin = fechaFin ?? DateTime.Now;

                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();
                var pagosEnRango = pagos.Where(p =>
                    p.FechaPago >= inicio &&
                    p.FechaPago <= fin.AddDays(1) &&
                    (p.Estado == EstadoPago.Completado || p.Estado == EstadoPago.Verificado)).ToList();

                var viewModel = new AdminReporteViewModel
                {
                    FechaInicio = inicio,
                    FechaFin = fin,
                    TotalVentas = pagosEnRango.Count,
                    TotalIngresos = pagosEnRango.Sum(p => p.Monto),
                    TotalComisiones = pagosEnRango.Sum(p => p.ComisionPlataforma),
                    PromedioVenta = pagosEnRango.Any() ? pagosEnRango.Average(p => p.Monto) : 0
                };

                viewModel.VentasPorVendedor = pagosEnRango
                    .SelectMany(p => p.Venta.Detalles)
                    .GroupBy(d => d.VendedorId)
                    .Select(g => new ReporteVendedorDTO
                    {
                        VendedorId = g.Key,
                        NombreVendedor = g.First().Vendedor?.NombreComercial ?? "N/A",
                        CantidadVentas = g.Select(d => d.VentaId).Distinct().Count(),
                        TotalIngresos = g.Sum(d => d.Subtotal),
                        ProductosVendidos = g.Sum(d => d.Cantidad)
                    })
                    .OrderByDescending(x => x.TotalIngresos)
                    .ToList();

                viewModel.ProductosMasVendidos = pagosEnRango
                    .SelectMany(p => p.Venta.Detalles)
                    .GroupBy(d => d.TortaId)
                    .Select(g => new ReporteProductoDTO
                    {
                        TortaId = g.Key,
                        NombreTorta = g.First().Torta?.Nombre ?? "N/A",
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalIngresos = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(10)
                    .ToList();

                viewModel.VentasPorDia = pagosEnRango
                    .GroupBy(p => p.FechaPago.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => new ReporteDiarioDTO
                        {
                            CantidadVentas = g.Count(),
                            Ingresos = g.Sum(p => p.Monto),
                            Comisiones = g.Sum(p => p.ComisionPlataforma),
                            ProductosVendidos = g.SelectMany(p => p.Venta.Detalles).Sum(d => d.Cantidad)
                        }
                    );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reportes");
                TempData["Error"] = "Error al cargar los reportes";
                return View(new AdminReporteViewModel());
            }
        }
    }
}