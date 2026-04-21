using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;        
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.SignalR;
using CasaDeLasTortas.Hubs;

namespace CasaDeLasTortas.Controllers
{
    
    
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<PagoController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PagoController(
            IUnitOfWork unitOfWork, 
            IFileService fileService,
            ILogger<PagoController> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
            _hubContext = hubContext;
        }

        // GET: Pago/Procesar/{ventaId}
        [Authorize(Roles = "Comprador,Admin")]
        public async Task<IActionResult> Procesar(int ventaId)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(ventaId);
                if (venta == null)
                {
                    TempData["Error"] = "Venta no encontrada";
                    return RedirectToAction("MisCompras", "Venta");
                }

                // Verificar que el comprador es el dueño de la venta
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null || venta.CompradorId != compradorId)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Verificar que la venta está pendiente y no tiene pagos completados
                if (venta.Estado != EstadoVenta.Pendiente || venta.Pagos.Any(p => p.Estado == EstadoPago.Completado))
                {
                    TempData["Info"] = "Esta venta ya no requiere pago";
                    return RedirectToAction("Details", "Venta", new { id = ventaId });
                }

                var viewModel = new ProcesarPagoViewModel
                {
                    VentaId = venta.Id,
                    NumeroOrden = venta.NumeroOrden,
                    MontoTotal = venta.Total,
                    MetodosDisponibles = Enum.GetValues<MetodoPago>().ToList(),
                    CompradorId = venta.CompradorId,
                    CompradorNombre = venta.Comprador?.Persona?.Nombre ?? "Cliente",
                    CompradorEmail = venta.Comprador?.Persona?.Email ?? string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar procesar pago para venta {VentaId}", ventaId);
                TempData["Error"] = "Error al cargar el pago";
                return RedirectToAction("MisCompras", "Venta");
            }
        }

        // POST: Pago/RealizarPago
        [HttpPost]
        [Authorize(Roles = "Comprador,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarPago(RealizarPagoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var venta = await _unitOfWork.Ventas.GetByIdAsync(model.VentaId);
                    if (venta != null)
                    {
                        var viewModel = new ProcesarPagoViewModel
                        {
                            VentaId = venta.Id,
                            NumeroOrden = venta.NumeroOrden,
                            MontoTotal = venta.Total,
                            MetodosDisponibles = Enum.GetValues<MetodoPago>().ToList()
                        };
                        return View("Procesar", viewModel);
                    }
                    return RedirectToAction("MisCompras", "Venta");
                }

                var ventaExistente = await _unitOfWork.Ventas.GetByIdWithTodoAsync(model.VentaId);
                if (ventaExistente == null)
                {
                    return Json(new { success = false, message = "Venta no encontrada" });
                }

                // Verificar que el comprador es el dueño
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null || ventaExistente.CompradorId != compradorId)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Obtener el comprador para los datos de notificación
                var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(compradorId.Value);

                // Crear el pago
                var pago = new Pago
                {
                    VentaId = model.VentaId,
                    CompradorId = ventaExistente.CompradorId,
                    Monto = ventaExistente.Total,
                    FechaPago = DateTime.Now,
                    Estado = model.MetodoPago == MetodoPago.Transferencia ? EstadoPago.Pendiente : EstadoPago.Completado,
                    MetodoPago = model.MetodoPago,
                    NumeroTransaccion = model.NumeroTransaccion,
                    Observaciones = model.Observaciones
                };

                // Subir comprobante si es transferencia
                if (model.MetodoPago == MetodoPago.Transferencia && model.ArchivoComprobante != null)
                {
                    pago.ArchivoComprobante = await _fileService.SaveFileAsync(model.ArchivoComprobante, "comprobantes");
                }

                await _unitOfWork.PagoRepository.AddAsync(pago);

                // Si el pago es inmediato, actualizar estado de la venta
                if (pago.Estado == EstadoPago.Completado)
                {
                    ventaExistente.Estado = EstadoVenta.Pagada;
                    ventaExistente.FechaActualizacion = DateTime.Now;
                    await _unitOfWork.Ventas.UpdateAsync(ventaExistente);

                    // Actualizar estadísticas de los vendedores
                    foreach (var detalle in ventaExistente.Detalles)
                    {
                        var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(detalle.VendedorId);
                        if (vendedor != null)
                        {
                            vendedor.TotalVentas++;
                            await _unitOfWork.VendedorRepository.UpdateAsync(vendedor);
                            
                            // Notificación al vendedor
                            await _hubContext.Clients.User(vendedor.Id.ToString())
                                .SendAsync("NuevoPedido", new
                                {
                                    pagoId = pago.Id,
                                    ventaId = ventaExistente.Id,
                                    numeroOrden = ventaExistente.NumeroOrden,
                                    compradorNombre = comprador?.Persona?.Nombre ?? "Cliente",
                                    tortaNombre = detalle.Torta?.Nombre ?? "Torta",
                                    cantidad = detalle.Cantidad,
                                    monto = detalle.Subtotal,
                                    fecha = DateTime.Now
                                });
                        }

                        var torta = await _unitOfWork.TortaRepository.GetByIdAsync(detalle.TortaId);
                        if (torta != null)
                        {
                            torta.VecesVendida += detalle.Cantidad;
                            await _unitOfWork.TortaRepository.UpdateAsync(torta);
                        }
                    }
                }

                // Notificación al comprador
                await _hubContext.Clients.User(ventaExistente.CompradorId.ToString())
                    .SendAsync("PedidoConfirmado", new
                    {
                        pagoId = pago.Id,
                        ventaId = ventaExistente.Id,
                        numeroOrden = ventaExistente.NumeroOrden,
                        monto = pago.Monto,
                        estado = pago.Estado.ToString(),
                        fecha = DateTime.Now,
                        mensaje = pago.Estado == EstadoPago.Completado 
                            ? "¡Tu pago ha sido confirmado!"
                            : "Pago registrado. Esperando confirmación del vendedor."
                    });

                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction("Confirmacion", new { pagoId = pago.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar pago");
                TempData["Error"] = "Error al procesar el pago";
                return RedirectToAction("MisCompras", "Venta");
            }
        }

        // GET: Pago/Confirmacion/{pagoId}
        public async Task<IActionResult> Confirmacion(int pagoId)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(pagoId);
                if (pago == null)
                {
                    return RedirectToAction("MisCompras", "Venta");
                }

                //  Usar ConfirmacionPagoViewModel (existe en PagoViewModel.cs línea 46)
                var viewModel = new ConfirmacionPagoViewModel
                {
                    PagoId = pago.Id,
                    VentaId = pago.VentaId,
                    NumeroOrden = pago.Venta?.NumeroOrden ?? string.Empty,
                    Monto = pago.Monto,
                    FechaPago = pago.FechaPago,
                    Estado = pago.Estado,
                    MetodoPago = pago.MetodoPago,
                    Mensaje = pago.Estado == EstadoPago.Completado 
                        ? "¡Pago confirmado! Tu compra está siendo procesada."
                        : "Pago registrado. Esperando confirmación del vendedor."
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar confirmación de pago {PagoId}", pagoId);
                TempData["Error"] = "Error al cargar la confirmación";
                return RedirectToAction("MisCompras", "Venta");
            }
        }

        // GET: Pago/ConfirmarPago/{pagoId} (Para vendedores)
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> ConfirmarPago(int pagoId)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(pagoId);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    TempData["Error"] = "Pago no encontrado o ya confirmado";
                    return RedirectToAction("MisPedidos", "Venta");
                }

                // Verificar que el vendedor tiene productos en esta venta
                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null || pago.Venta == null || !pago.Venta.Detalles.Any(d => d.VendedorId == vendedorId))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                //  Usar ConfirmarPagoViewModel (existe en PagoViewModel.cs línea 62)
                var viewModel = new ConfirmarPagoViewModel
                {
                    PagoId = pago.Id,
                    Monto = pago.Monto,
                    NombreTorta = pago.Venta.Detalles.FirstOrDefault()?.Torta?.Nombre ?? "Producto",
                    Cantidad = pago.Venta.Detalles.Sum(d => d.Cantidad),
                    NumeroTransaccion = pago.NumeroTransaccion
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar confirmación de pago {PagoId}", pagoId);
                TempData["Error"] = "Error al cargar la confirmación";
                return RedirectToAction("MisPedidos", "Venta");
            }
        }

        // POST: Pago/ConfirmarPago
        [HttpPost]
        [Authorize(Roles = "Vendedor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPago(ConfirmarPagoViewModel model)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(model.PagoId);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    return Json(new { success = false, message = "Pago no válido" });
                }

                // Verificar que el vendedor es el dueño de algún detalle
                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null)
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Actualizar estado del pago
                //  Usar campos reales de Pago.cs
                pago.Estado = EstadoPago.Verificado;  // Verificado por vendedor, no Completado aún
                pago.FechaVerificacion = DateTime.Now;
                pago.VerificadoPorId = vendedorId;
                pago.NumeroTransaccion = model.NumeroTransaccion;
                pago.FechaActualizacion = DateTime.Now;

                if (!string.IsNullOrEmpty(model.NotasConfirmacion))
                {
                    pago.Observaciones = (pago.Observaciones ?? "") + $"\nConfirmación: {model.NotasConfirmacion}";
                }

                // Manejar comprobante adicional
                if (model.ArchivoComprobante != null && model.ArchivoComprobante.Length > 0)
                {
                    pago.ArchivoComprobante = await _fileService.SaveFileAsync(model.ArchivoComprobante, "comprobantes");
                }

                // Actualizar estado de la venta
                var venta = pago.Venta;
                if (venta != null)
                {
                    venta.Estado = EstadoVenta.Pagada;
                    venta.FechaActualizacion = DateTime.Now;

                    // Actualizar estadísticas de los vendedores
                    foreach (var detalle in venta.Detalles)
                    {
                        var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(detalle.VendedorId);
                        if (vendedor != null)
                        {
                            vendedor.TotalVentas++;
                            await _unitOfWork.VendedorRepository.UpdateAsync(vendedor);
                        }

                        var torta = await _unitOfWork.TortaRepository.GetByIdAsync(detalle.TortaId);
                        if (torta != null)
                        {
                            torta.VecesVendida += detalle.Cantidad;
                            await _unitOfWork.TortaRepository.UpdateAsync(torta);
                        }
                    }

                    // Notificación al comprador
                    await _hubContext.Clients.User(venta.CompradorId.ToString())
                        .SendAsync("PagoConfirmado", new
                        {
                            pagoId = pago.Id,
                            ventaId = venta.Id,
                            numeroOrden = venta.NumeroOrden,
                            monto = pago.Monto,
                            fechaConfirmacion = pago.FechaVerificacion,  
                            mensaje = "Tu pago ha sido confirmado por el vendedor"
                        });

                    await _unitOfWork.Ventas.UpdateAsync(venta);
                }

                await _unitOfWork.PagoRepository.UpdateAsync(pago);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Pago confirmado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar pago");
                return Json(new { success = false, message = "Error al confirmar el pago" });
            }
        }

        // GET: Pago/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction("MisCompras", "Venta");
                }

                //  Usar PagoDetalleViewModel (existe en PagoViewModel.cs línea 188)
                var viewModel = new PagoDetalleViewModel
                {
                    
                    Pago = new PagoViewModel
                    {
                        Id = pago.Id,
                        VentaId = pago.VentaId,
                        CompradorId = pago.CompradorId,
                        Monto = pago.Monto,
                        FechaPago = pago.FechaPago,
                        Estado = pago.Estado,
                        MetodoPago = pago.MetodoPago,
                        ArchivoComprobante = pago.ArchivoComprobante,
                        NumeroTransaccion = pago.NumeroTransaccion,
                        Observaciones = pago.Observaciones,
                        // Mapear campos de entidad → ViewModel
                        FechaConfirmacion = pago.FechaVerificacion,      // Entidad: FechaVerificacion
                        ConfirmadoPorVendedorId = pago.VerificadoPorId,  // Entidad: VerificadoPorId
                        NombreComprador = pago.Comprador?.Persona?.Nombre,
                        EmailComprador = pago.Comprador?.Persona?.Email,
                        TelefonoComprador = pago.Comprador?.Telefono,
                        NumeroOrden = pago.Venta?.NumeroOrden
                    },
                    //  VentaResumenViewModel (existe en VentaViewModels.cs)
                    Venta = new VentaResumenViewModel
                    {
                        Id = pago.Venta?.Id ?? 0,
                        NumeroOrden = pago.Venta?.NumeroOrden ?? string.Empty,
                        FechaVenta = pago.Venta?.FechaVenta ?? DateTime.MinValue,
                        Estado = pago.Venta?.Estado ?? EstadoVenta.Pendiente,
                        Total = pago.Venta?.Total ?? 0,
                        TotalItems = pago.Venta?.Detalles?.Sum(d => d.Cantidad) ?? 0
                    },
                    //  CompradorViewModel (existe en CompradorViewModel.cs)
                    Comprador = new CompradorViewModel
                    {
                        Id = pago.Comprador?.Id ?? 0,
                        PersonaId = pago.Comprador?.PersonaId ?? 0,
                        NombrePersona = pago.Comprador?.Persona?.Nombre ?? string.Empty,
                        Email = pago.Comprador?.Persona?.Email ?? string.Empty,
                        Telefono = pago.Comprador?.Telefono ?? string.Empty,
                        Direccion = pago.Comprador?.Direccion ?? string.Empty,
                        Ciudad = pago.Comprador?.Ciudad,
                        Provincia = pago.Comprador?.Provincia,
                        CodigoPostal = pago.Comprador?.CodigoPostal,
                        TotalCompras = pago.Comprador?.TotalCompras ?? 0,
                        Activo = pago.Comprador?.Activo ?? true
                    }
                };

                // Determinar permisos
                viewModel.PuedeEditar = await PuedeEditarPago(pago);
                viewModel.PuedeCancelar = pago.Estado == EstadoPago.Pendiente;
                viewModel.PuedeConfirmar = pago.Estado == EstadoPago.Pendiente && User.IsInRole("Vendedor");
                viewModel.PuedeSubirComprobante = pago.Estado == EstadoPago.Pendiente && string.IsNullOrEmpty(pago.ArchivoComprobante);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de pago {PagoId}", id);
                TempData["Error"] = "Error al cargar los detalles";
                return RedirectToAction("MisCompras", "Venta");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ══════════════════════════════════════════════════════════════════════

        private async Task<int?> ObtenerCompradorIdActual()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Comprador?.Id;
        }

        private async Task<int?> ObtenerVendedorIdActual()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Vendedor?.Id;
        }

        private async Task<bool> PuedeEditarPago(Pago pago)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Comprador"))
            {
                var compradorId = await ObtenerCompradorIdActual();
                return compradorId == pago.CompradorId;
            }

            if (User.IsInRole("Vendedor"))
            {
                var vendedorId = await ObtenerVendedorIdActual();
                return vendedorId != null && pago.Venta?.Detalles?.Any(d => d.VendedorId == vendedorId) == true;
            }

            return false;
        }

        
    }
}