using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Services;
using CasaDeLasTortas.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using CasaDeLasTortas.Hubs;


namespace CasaDeLasTortas.Controllers
{
    /// <summary>
    /// Controller de Administración - Acceso restringido a rol Admin
    /// Incluye gestión de pagos, liberaciones y disputas
    /// </summary>
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;
        private readonly ILiberacionService _liberacionService;
        private readonly IFileService _fileService;
        private readonly ILogger<AdminController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(
            IUnitOfWork unitOfWork,
            IPagoService pagoService,
            ILiberacionService liberacionService,
            IFileService fileService,
            ILogger<AdminController> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
            _liberacionService = liberacionService;
            _fileService = fileService;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Dashboard principal del administrador
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
                var liberacionesPendientesList = await _liberacionService.GetLiberacionesPendientesAsync();
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

                // Top vendedores — ingresos calculados desde pagos reales
                var ingresosPorVendedor = pagosCompletados
                    .Where(p => p.Venta?.Detalles != null)
                    .SelectMany(p => p.Venta!.Detalles.Select(d => new { d.VendedorId, d.Subtotal }))
                    .GroupBy(x => x.VendedorId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Subtotal));

                var ventasPorVendedor = pagosCompletados
                    .Where(p => p.Venta?.Detalles != null)
                    .SelectMany(p => p.Venta!.Detalles.Select(d => new { d.VendedorId, d.VentaId }))
                    .GroupBy(x => x.VendedorId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.VentaId).Distinct().Count());

                var topVendedoresList = vendedores
                    .OrderByDescending(v => ingresosPorVendedor.GetValueOrDefault(v.Id, 0))
                    .Take(5)
                    .Select((v, index) => new TopVendedorDTO
                    {
                        Id = v.Id,
                        NombreComercial = v.NombreComercial,
                        Avatar = v.Persona?.Avatar,
                        TotalVentas = ventasPorVendedor.GetValueOrDefault(v.Id, 0),
                        TotalIngresos = ingresosPorVendedor.GetValueOrDefault(v.Id, 0),
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
                    LiberacionesPendientes = liberacionesPendientesList.ToList(),
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
        //  GESTIÓN DE PAGOS
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Pagos(string estado = "", int pagina = 1)
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPago>(estado, out var estadoEnum))
                    pagos = pagos.Where(p => p.Estado == estadoEnum);

                var totalItems = pagos.Count();
                var tamanioPagina = 15;
                var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamanioPagina);

                var pagosPaginados = pagos
                    .OrderByDescending(p => p.FechaPago)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                var viewModel = new AdminPagosViewModel
                {
                    PagosPendientes = pagosPaginados.Select(p =>
                    {
                        var fechaRef = p.FechaComprobante ?? p.FechaPago;
                        return new PagoPendienteDTO
                        {
                            Id = p.Id,
                            VentaId = p.VentaId,
                            NumeroOrden = p.Venta?.NumeroOrden ?? "",
                            NombreComprador = p.Comprador?.Persona?.Nombre ?? "",
                            EmailComprador = p.Comprador?.Persona?.Email ?? "",
                            Monto = p.Monto,
                            Estado = p.Estado,
                            FechaPago = fechaRef,
                            FechaComprobante = p.FechaComprobante,
                            HorasEsperando = fechaRef == default
                                ? 0
                                : (int)(DateTime.Now - fechaRef).TotalHours,
                            MetodoPago = p.MetodoPago?.ToString() ?? "Transferencia",
                            NumeroTransaccion = p.NumeroTransaccion,
                            ArchivoComprobante = p.ArchivoComprobante,
                            IntentosRechazados = p.IntentosRechazados
                        };
                    }).ToList(),
                    Estadisticas = await _pagoService.GetEstadisticasAsync(),
                    FiltroEstado = estado,
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalRegistros = totalItems
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pagos");
                TempData["Error"] = "Error al cargar los pagos";
                return View(new AdminPagosViewModel());
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
                var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(personaIdClaim, out int adminPersonaId))
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Pagos));
                }
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(adminPersonaId);
                if (persona == null)
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Pagos));
                }

                var result = await _pagoService.VerificarPagoAsync(pagoId, persona.Id, aprobado, observaciones, motivoRechazo);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;

                    // Notificar al comprador por SignalR
                    var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(pagoId);
                    var compradorPersonaId = pago?.Comprador?.PersonaId.ToString();
                    if (!string.IsNullOrEmpty(compradorPersonaId))
                    {
                        if (aprobado)
                        {
                            await _hubContext.Clients.User(compradorPersonaId).SendAsync("PagoVerificado", new
                            {
                                tipo        = "pago_verificado",
                                pagoId,
                                numeroOrden = pago!.Venta?.NumeroOrden,
                                monto       = pago.Monto,
                                mensaje     = $"¡Tu pago fue aprobado! Ya podés esperar la preparación de tu pedido #{pago.Venta?.NumeroOrden}.",
                                timestamp   = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            await _hubContext.Clients.User(compradorPersonaId).SendAsync("PagoRechazado", new
                            {
                                tipo        = "pago_rechazado",
                                pagoId,
                                numeroOrden = pago!.Venta?.NumeroOrden,
                                motivo      = motivoRechazo,
                                mensaje     = $"Tu comprobante de la orden #{pago.Venta?.NumeroOrden} fue rechazado: {motivoRechazo}",
                                timestamp   = DateTime.UtcNow
                            });
                        }
                    }
                }
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

        /// <summary>
        /// Marca un pago rechazado como pendiente de reembolso,
        /// para que aparezca en la sección Reembolsos y el admin pueda devolver el dinero.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarParaReembolso(int pagoId, string motivoReembolso)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(pagoId);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction(nameof(Pagos));
                }

                if (pago.Estado != EstadoPago.Rechazado)
                {
                    TempData["Error"] = "Solo se puede marcar para reembolso un pago rechazado";
                    return RedirectToAction(nameof(VerificarPago), new { id = pagoId });
                }

                pago.Estado = EstadoPago.ReembolsoPendiente;
                pago.MotivoReembolso = motivoReembolso;
                _unitOfWork.PagoRepository.Update(pago);
                await _unitOfWork.SaveChangesAsync();

                // Notificar al comprador
                var compradorPersonaId = pago.Comprador?.PersonaId.ToString();
                if (!string.IsNullOrEmpty(compradorPersonaId))
                {
                    await _hubContext.Clients.User(compradorPersonaId).SendAsync("ReembolsoPendiente", new
                    {
                        tipo        = "reembolso_pendiente",
                        pagoId,
                        numeroOrden = pago.Venta?.NumeroOrden,
                        monto       = pago.Monto,
                        mensaje     = $"Tu pago de la orden #{pago.Venta?.NumeroOrden} será reembolsado. Nos contactaremos para coordinar la devolución.",
                        timestamp   = DateTime.UtcNow
                    });
                }

                TempData["Success"] = "Pago marcado para reembolso. Aparecerá en la sección Reembolsos.";
                return RedirectToAction(nameof(Reembolsos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar pago {PagoId} para reembolso", pagoId);
                TempData["Error"] = "Error al procesar el reembolso";
                return RedirectToAction(nameof(Pagos));
            }
        }

        /// <summary>
        /// Vista de gestión de liberaciones de fondos
        /// </summary>
        public async Task<IActionResult> Liberaciones(string estado = "", int pagina = 1)
        {
            try
            {
                // ── Ventas que requieren acción del admin ──────────────────────
                // Incluye: pago verificado, fondos aún no liberados, estado relevante
                var estadosRelevantes = new[]
                {
                    EstadoVenta.Pagada, EstadoVenta.EnPreparacion,
                    EstadoVenta.ListaParaRetiro, EstadoVenta.Enviada
                };
                var todasVentas = await _unitOfWork.Ventas.GetAllWithDetailsAsync();
                var ventasPendientesEntrega = todasVentas
                    .Where(v => !v.FondosLiberados && estadosRelevantes.Contains(v.Estado))
                    .OrderBy(v => v.FechaVenta)
                    .ToList();

                // ── Liberaciones listas para transferir ───────────────────────
                var liberacionesListas = (await _unitOfWork.Liberaciones.GetListasParaLiberarAsync()).ToList();

                // ── KPIs ──────────────────────────────────────────────────────
                var countPendiente = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Pendiente)
                                   + await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.ListoParaLiberar);
                var totalPendiente = await _unitOfWork.Liberaciones.GetMontoPendienteLiberacionAsync();
                var countLiberado  = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Confirmado)
                                   + await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Transferido);
                var totalLiberado  = await _unitOfWork.Liberaciones.GetMontoTotalLiberadoAsync();

                // ── Historial (Transferido + Confirmado) ──────────────────────
                var historialLibs = (await _unitOfWork.Liberaciones.GetByEstadoAsync(EstadoLiberacion.Confirmado))
                    .Concat(await _unitOfWork.Liberaciones.GetByEstadoAsync(EstadoLiberacion.Transferido))
                    .OrderByDescending(l => l.FechaTransferencia ?? l.FechaConfirmacion)
                    .ToList();

                ViewBag.CountPendiente          = countPendiente;
                ViewBag.TotalPendiente          = totalPendiente;
                ViewBag.CountLiberado           = countLiberado;
                ViewBag.TotalLiberado           = totalLiberado;
                ViewBag.VentasPendientesEntrega = ventasPendientesEntrega;
                ViewBag.VentasEntregadas        = new List<Venta>();
                ViewBag.LiberacionesListas      = liberacionesListas;
                ViewBag.HistorialLiberaciones   = historialLibs;

                return View(new List<LiberacionFondos>());
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

                var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(personaIdClaim, out int adminPersonaId))
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Liberaciones));
                }
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(adminPersonaId);
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
        //  GESTIÓN DE DISPUTAS
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de gestión de disputas
        /// </summary>
        public async Task<IActionResult> Disputas(string estado = "", int pagina = 1)
        {
            try
            {
                // Traer todas con todos los datos relacionados
                var todasDisputas = (await _unitOfWork.Disputas.GetAllWithDetallesAsync()).ToList();

                // Filtrar usando los nombres simples que envían los botones de la vista
                IEnumerable<Disputa> disputasFiltradas = estado switch
                {
                    "Abierta"   => todasDisputas.Where(d => d.Estado == EstadoDisputa.Abierta),
                    "EnProceso" => todasDisputas.Where(d =>
                                        d.Estado == EstadoDisputa.EnInvestigacion ||
                                        d.Estado == EstadoDisputa.EsperandoVendedor ||
                                        d.Estado == EstadoDisputa.EsperandoComprador),
                    "Resuelta"  => todasDisputas.Where(d =>
                                        d.Estado == EstadoDisputa.ResueltaFavorComprador ||
                                        d.Estado == EstadoDisputa.ResueltaFavorVendedor ||
                                        d.Estado == EstadoDisputa.ResueltaAcuerdo ||
                                        d.Estado == EstadoDisputa.Cerrada ||
                                        d.Estado == EstadoDisputa.Cancelada),
                    _           => todasDisputas
                };

                // KPIs con los nombres que lee la vista
                ViewBag.CountAbiertas  = todasDisputas.Count(d => d.Estado == EstadoDisputa.Abierta);
                ViewBag.CountResueltas = todasDisputas.Count(d =>
                    d.Estado == EstadoDisputa.EnInvestigacion ||
                    d.Estado == EstadoDisputa.EsperandoVendedor ||
                    d.Estado == EstadoDisputa.EsperandoComprador);
                ViewBag.CountCerradas  = todasDisputas.Count(d =>
                    d.Estado == EstadoDisputa.ResueltaFavorComprador ||
                    d.Estado == EstadoDisputa.ResueltaFavorVendedor ||
                    d.Estado == EstadoDisputa.ResueltaAcuerdo ||
                    d.Estado == EstadoDisputa.Cerrada ||
                    d.Estado == EstadoDisputa.Cancelada);

                ViewBag.FiltroEstado = estado;

                // Shape rico para la vista — incluye datos de venta, productos y pago
                ViewBag.Disputas = disputasFiltradas.Select(d =>
                {
                    var detalles   = d.Venta?.Detalles?.ToList() ?? new List<DetalleVenta>();
                    var pagos      = d.Venta?.Pagos?.ToList()    ?? new List<Pago>();
                    var pagoPrincipal = pagos.OrderByDescending(p => p.FechaComprobante ?? p.FechaPago).FirstOrDefault();

                    // Nombre(s) de vendedor(es) involucrados
                    var vendedores = detalles
                        .Where(det => det.Vendedor != null)
                        .Select(det => det.Vendedor!.NombreComercial)
                        .Distinct()
                        .ToList();

                    return (object)new
                    {
                        Id              = d.Id,
                        NumeroOrden     = d.Venta?.NumeroOrden ?? "—",
                        FechaVenta      = d.Venta?.FechaVenta,
                        NombreComprador = d.Iniciador?.Nombre ?? "—",
                        EmailComprador  = d.Iniciador?.Email  ?? "—",
                        NombreVendedor  = vendedores.Any() ? string.Join(", ", vendedores) : "—",
                        Motivo          = d.Titulo ?? d.Descripcion ?? "Sin descripción",
                        Descripcion     = d.Descripcion ?? "—",
                        Estado          = MapearEstadoSimpleDisputa(d.Estado),
                        FechaCreacion   = (DateTime?)d.FechaCreacion,

                        // Venta
                        TotalVenta      = d.Venta?.Total ?? 0m,
                        EstadoVenta     = d.Venta?.Estado.ToString() ?? "—",
                        MontoInvolucrado= d.MontoInvolucrado ?? 0m,

                        // Productos
                        Productos       = detalles.Select(det => new
                        {
                            Nombre    = det.Torta?.Nombre ?? "Producto",
                            Vendedor  = det.Vendedor?.NombreComercial ?? "—",
                            Cantidad  = det.Cantidad,
                            Subtotal  = det.Subtotal,
                            Estado    = det.Estado.ToString()
                        }).ToList(),

                        // Comprador extra
                        TelefonoComprador  = d.Iniciador?.Telefono ?? "—",

                        // Pago
                        EstadoPago         = pagoPrincipal?.Estado.ToString() ?? "SinPago",
                        TieneComprobante   = pagoPrincipal?.ArchivoComprobante != null,
                        MontoPagado        = pagoPrincipal?.Monto ?? 0m,
                        FechaPago          = pagoPrincipal?.FechaPago,

                        // Resolución
                        Resolucion         = d.Resolucion.ToString(),
                        DetalleResolucion  = d.DetalleResolucion ?? "—",
                        FechaResolucion    = d.FechaResolucion
                    };
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar disputas");
                TempData["Error"] = "Error al cargar las disputas";
                ViewBag.Disputas       = new List<object>();
                ViewBag.CountAbiertas  = 0;
                ViewBag.CountResueltas = 0;
                ViewBag.CountCerradas  = 0;
                return View();
            }
        }

        private static string MapearEstadoSimpleDisputa(EstadoDisputa estado) => estado switch
        {
            EstadoDisputa.EnInvestigacion or
            EstadoDisputa.EsperandoVendedor or
            EstadoDisputa.EsperandoComprador    => "EnProceso",
            EstadoDisputa.ResueltaFavorComprador or
            EstadoDisputa.ResueltaFavorVendedor or
            EstadoDisputa.ResueltaAcuerdo       => "Resuelta",
            EstadoDisputa.Cerrada or
            EstadoDisputa.Cancelada             => "Cerrada",
            _                                   => "Abierta"
        };

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
        /// <summary>
        /// Gestionar disputa desde el modal de la vista Disputas.cshtml
        /// Acepta: disputaId, nuevoEstado (string), resolucion (texto), procesarReembolso (bool)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarDisputa(int disputaId, string nuevoEstado, string resolucion,
            bool procesarReembolso = false, string? aliasReembolso = null,
            string? numTransaccionReembolso = null, IFormFile? comprobanteReembolso = null)
        {
            try
            {
                if (!Enum.TryParse<EstadoDisputa>(nuevoEstado, ignoreCase: true, out var estadoEnum))
                {
                    // Mapear nombres simples de la vista a valores del enum
                    estadoEnum = nuevoEstado switch
                    {
                        "EnProceso"      => EstadoDisputa.EnInvestigacion,
                        "FavorComprador" => EstadoDisputa.ResueltaFavorComprador,
                        "FavorVendedor"  => EstadoDisputa.ResueltaFavorVendedor,
                        "Resuelta"       => EstadoDisputa.ResueltaAcuerdo,
                        "Cerrada"        => EstadoDisputa.Cerrada,
                        _                => EstadoDisputa.EnInvestigacion
                    };
                }

                var disputa = await _unitOfWork.Disputas.GetByIdAsync(disputaId);
                if (disputa == null)
                {
                    TempData["Error"] = "Disputa no encontrada";
                    return RedirectToAction(nameof(Disputas));
                }

                disputa.Estado = estadoEnum;
                disputa.DetalleResolucion = resolucion;
                disputa.FechaActualizacion = DateTime.Now;

                // Si se resuelve, registrar fecha y resolución formal
                if (estadoEnum == EstadoDisputa.ResueltaFavorComprador ||
                    estadoEnum == EstadoDisputa.ResueltaFavorVendedor ||
                    estadoEnum == EstadoDisputa.ResueltaAcuerdo ||
                    estadoEnum == EstadoDisputa.Cerrada)
                {
                    disputa.FechaResolucion = DateTime.Now;
                    disputa.Resolucion = estadoEnum switch
                    {
                        EstadoDisputa.ResueltaFavorComprador => procesarReembolso
                            ? ResolucionDisputa.ReembolsoTotal
                            : ResolucionDisputa.ReembolsoParcial,
                        EstadoDisputa.ResueltaFavorVendedor => ResolucionDisputa.SinAccion,
                        _ => ResolucionDisputa.OtroAcuerdo
                    };

                    // Si se resuelve a favor del comprador y hay reembolso, actualizar venta y pago
                    if (procesarReembolso)
                    {
                        var venta = await _unitOfWork.Ventas.GetByIdAsync(disputa.VentaId);
                        if (venta != null)
                        {
                            venta.Estado = EstadoVenta.Cancelada;
                            _unitOfWork.Ventas.Update(venta);
                        }

                        // Marcar el pago como pendiente de reembolso
                        var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(disputa.VentaId);
                        var pago  = pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault();
                        if (pago != null)
                        {
                            pago.Estado = EstadoPago.ReembolsoPendiente;
                            pago.MotivoReembolso = resolucion;
                            _unitOfWork.PagoRepository.Update(pago);
                        }
                    }
                    else
                    {
                        // Restaurar venta a Pagada si no hay reembolso
                        var venta = await _unitOfWork.Ventas.GetByIdAsync(disputa.VentaId);
                        if (venta != null && venta.Estado == EstadoVenta.EnDisputa)
                        {
                            venta.Estado = EstadoVenta.Pagada;
                            _unitOfWork.Ventas.Update(venta);
                        }
                    }
                }

                _unitOfWork.Disputas.Update(disputa);
                await _unitOfWork.SaveChangesAsync();

                // Si se proporcionó comprobante de reembolso, procesar inmediatamente
                if (procesarReembolso && comprobanteReembolso != null && !string.IsNullOrEmpty(numTransaccionReembolso))
                {
                    var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(disputa.VentaId);
                    var pago  = pagos.FirstOrDefault(p => p.Estado == EstadoPago.ReembolsoPendiente);
                    if (pago != null)
                    {
                        var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(adminIdClaim, out int adminId))
                        {
                            var ruta = await _fileService.SaveFileAsync(comprobanteReembolso, "reembolsos");
                            await _pagoService.ProcesarReembolsoAsync(pago.Id, adminId, ruta, numTransaccionReembolso);
                        }
                    }
                }

                TempData["Success"] = $"Disputa actualizada a: {nuevoEstado}";
                return RedirectToAction(nameof(Disputas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gestionar disputa {Id}", disputaId);
                TempData["Error"] = "Error al gestionar la disputa";
                return RedirectToAction(nameof(Disputas));
            }
        }

        // ==================== REEMBOLSOS ====================

        public async Task<IActionResult> Reembolsos()
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetReembolsosPendientesAsync();

                var lista = pagos.Select(p => new
                {
                    PagoId            = p.Id,
                    VentaId           = p.VentaId,
                    NumeroOrden       = p.Venta?.NumeroOrden ?? "—",
                    NombreComprador   = p.Comprador?.Persona?.Nombre + " " + p.Comprador?.Persona?.Apellido,
                    EmailComprador    = p.Comprador?.Persona?.Email ?? "—",
                    TelefonoComprador = p.Comprador?.Persona?.Telefono ?? "—",
                    Monto             = p.Monto,
                    FechaPago         = p.FechaPago,
                    MotivoReembolso   = p.MotivoReembolso ?? "—"
                }).ToList();

                ViewBag.Reembolsos          = lista;
                ViewBag.ReembolsosPendientes = lista.Count;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reembolsos pendientes");
                TempData["Error"] = "Error al cargar los reembolsos";
                ViewBag.Reembolsos = new List<object>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarReembolso(int pagoId, string numeroTransaccion,
            IFormFile? comprobanteReembolso)
        {
            try
            {
                if (comprobanteReembolso == null || string.IsNullOrWhiteSpace(numeroTransaccion))
                {
                    TempData["Error"] = "El comprobante y el número de transacción son obligatorios";
                    return RedirectToAction(nameof(Reembolsos));
                }

                var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                {
                    TempData["Error"] = "No se pudo identificar al administrador";
                    return RedirectToAction(nameof(Reembolsos));
                }

                var ruta = await _fileService.SaveFileAsync(comprobanteReembolso, "reembolsos");
                var result = await _pagoService.ProcesarReembolsoAsync(pagoId, adminId, ruta, numeroTransaccion);

                if (result.Success)
                    TempData["Success"] = "Reembolso registrado correctamente";
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Reembolsos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar reembolso del pago {PagoId}", pagoId);
                TempData["Error"] = "Error al procesar el reembolso";
                return RedirectToAction(nameof(Reembolsos));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarDisputa(int disputaId)
        {
            try
            {
                var personaIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(personaIdClaim, out int adminPersonaId))
                {
                    TempData["Error"] = "Sesión inválida";
                    return RedirectToAction(nameof(Disputas));
                }
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(adminPersonaId);
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

                //  Envolver en el ViewModel que espera la vista
                var viewModel = new AdminConfiguracionViewModel
                {
                    Configuracion = config
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración");
                TempData["Error"] = "Error al cargar la configuración";
                return View(new AdminConfiguracionViewModel());
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
        // MÉTODOS EXISTENTES 
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Usuarios(int pagina = 1, string busqueda = "", string filtroRol = "", bool? activo = null)
        {
            try
            {
                const int tamanioPagina = 20;

                // Paginación y filtros se ejecutan en la BD, no en memoria
                var personas = await _unitOfWork.PersonaRepository.GetAllConPerfilesAsync(
                    pagina, tamanioPagina, busqueda, filtroRol, activo);

                var totalItems = await _unitOfWork.PersonaRepository.CountConPerfilesAsync(
                    busqueda, filtroRol, activo);

                var usuariosDTO = personas.Select(p => new UsuarioListaDTO
                {
                    PersonaId = p.Id,
                    VendedorId = p.Vendedor?.Id,
                    CompradorId = p.Comprador?.Id,
                    Nombre = p.Nombre,
                    NombreCompleto = p.NombreCompleto,
                    Email = p.Email,
                    Avatar = p.Avatar,
                    Rol = p.Rol,
                    NombreComercial = p.Vendedor?.NombreComercial,
                    Activo = p.Activo && (p.Vendedor?.Activo ?? true) && (p.Comprador?.Activo ?? true),
                    Verificado = p.Vendedor?.Verificado ?? false,
                    FechaRegistro = p.FechaRegistro,
                    UltimoAcceso = p.UltimoAcceso
                }).ToList();

                // Estadísticas: conteos simples sin traer registros
                var estadisticas = new EstadisticasUsuariosDTO
                {
                    TotalUsuarios    = await _unitOfWork.PersonaRepository.CountAsync(),
                    TotalCompradores = await _unitOfWork.PersonaRepository.CountByRolAsync("Comprador"),
                    TotalVendedores  = await _unitOfWork.PersonaRepository.CountByRolAsync("Vendedor"),
                    TotalAdmins      = await _unitOfWork.PersonaRepository.CountByRolAsync("Admin"),
                };

                var viewModel = new AdminUsuariosViewModel
                {
                    Usuarios       = usuariosDTO,
                    Estadisticas   = estadisticas,
                    Busqueda       = busqueda,
                    FiltroRol      = filtroRol,
                    FiltroActivo   = activo,
                    PaginaActual   = pagina,
                    TotalPaginas   = (int)Math.Ceiling(totalItems / (double)tamanioPagina),
                    TotalRegistros = totalItems,
                    TamanioPagina  = tamanioPagina
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
                var ventas = await _unitOfWork.Ventas.GetAllWithDetailsAsync();

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
        /// <summary>
        /// Confirmar entrega de una venta (dispara la liberación de fondos)
        /// </summary>
        /// <remarks>
        /// Esta acción es llamada desde la vista Liberaciones cuando el admin 
        /// hace clic en "Confirmar entrega". Cambia el estado de las liberaciones
        /// de "Pendiente" a "ListoParaLiberar" para que el admin pueda transferir.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEntrega(int ventaId)
        {
            try
            {
                // Llamar al servicio de liberación para marcar la entrega como confirmada
                var result = await _liberacionService.MarcarEntregaConfirmadaAsync(ventaId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Liberaciones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar entrega de venta {VentaId}", ventaId);
                TempData["Error"] = "Error al confirmar la entrega";
                return RedirectToAction(nameof(Liberaciones));
            }
        }


    }
}