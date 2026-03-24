using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;

namespace CasaDeLasTortas.Controllers
{
    
    // VENTA CONTROLLER
  
    
    [Authorize]
    public class VentaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VentaController> _logger;

        public VentaController(IUnitOfWork unitOfWork, ILogger<VentaController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: Venta/MisCompras (Para compradores)
        [Authorize(Roles = "Comprador,Admin")]
        public async Task<IActionResult> MisCompras(int pagina = 1, int tamanioPagina = 10)
        {
            try
            {
                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(compradorId.Value);
                
                // Ordenar por fecha descendente
                ventas = ventas.OrderByDescending(v => v.FechaVenta);

                // Paginación
                var totalItems = ventas.Count();
                var ventasPaginadas = ventas
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                var viewModel = new MisComprasViewModel
                {
                    Ventas = ventasPaginadas.Select(v => new VentaResumenViewModel
                    {
                        Id = v.Id,
                        NumeroOrden = v.NumeroOrden,
                        FechaVenta = v.FechaVenta,
                        Estado = v.Estado,
                        Total = v.Total,
                        TotalItems = v.Detalles.Sum(d => d.Cantidad),
                        ImagenPrincipal = v.Detalles.FirstOrDefault()?.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    }).ToList(),
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compras");
                TempData["Error"] = "Error al cargar el historial de compras";
                return View(new MisComprasViewModel());
            }
        }

        // GET: Venta/MisPedidos (Para vendedores)
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> MisPedidos(int pagina = 1, int tamanioPagina = 10, string? filtroEstado = null)
        {
            try
            {
                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null)
                {
                    return RedirectToAction("Create", "Vendedor");
                }

                var detalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(vendedorId.Value);
                
                // Filtrar por estado si se especifica
                if (!string.IsNullOrEmpty(filtroEstado) && Enum.TryParse<EstadoDetalleVenta>(filtroEstado, out var estado))
                {
                    detalles = detalles.Where(d => d.Estado == estado);
                }

                // Ordenar por fecha de venta descendente
                detalles = detalles.OrderByDescending(d => d.Venta.FechaVenta);

                // Paginación
                var totalItems = detalles.Count();
                var detallesPaginados = detalles
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                var viewModel = new MisPedidosViewModel
                {
                    Pedidos = detallesPaginados.Select(d => new PedidoVendedorViewModel
                    {
                        DetalleId = d.Id,
                        VentaId = d.VentaId,
                        NumeroOrden = d.Venta.NumeroOrden,
                        FechaVenta = d.Venta.FechaVenta,
                        TortaId = d.TortaId,
                        NombreTorta = d.Torta.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Estado = d.Estado,
                        NotasPersonalizacion = d.NotasPersonalizacion,
                        ImagenTorta = d.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                        DireccionEntrega = d.Venta.DireccionEntrega,
                        NombreComprador = d.Venta.Comprador?.Persona?.Nombre ?? "Cliente",
                        TelefonoComprador = d.Venta.Comprador?.Telefono ?? string.Empty,
                        FechaEntregaEstimada = d.FechaEstimadaPreparacion
                    }).ToList(),
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                    FiltroEstado = filtroEstado
                };

                // Estadísticas rápidas (sobre todos los detalles, no solo paginados)
                var allDetalles = await _unitOfWork.DetallesVenta.GetByVendedorIdWithVentaAsync(vendedorId.Value);
                viewModel.PendientesCount = allDetalles.Count(d => d.Estado == EstadoDetalleVenta.Pendiente);
                viewModel.EnPreparacionCount = allDetalles.Count(d => d.Estado == EstadoDetalleVenta.EnPreparacion);
                viewModel.ListosCount = allDetalles.Count(d => d.Estado == EstadoDetalleVenta.Listo);
                viewModel.EntregadosCount = allDetalles.Count(d => d.Estado == EstadoDetalleVenta.Entregado);
                viewModel.CanceladosCount = allDetalles.Count(d => d.Estado == EstadoDetalleVenta.Cancelado);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pedidos");
                TempData["Error"] = "Error al cargar los pedidos";
                return View(new MisPedidosViewModel());
            }
        }

        // GET: Venta/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(id);
                if (venta == null)
                {
                    TempData["Error"] = "Venta no encontrada";
                    return RedirectToAction(nameof(MisCompras));
                }

                // Verificar que el usuario tiene permiso para ver esta venta
                if (!await TienePermisoVerVenta(venta))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // ✅ CORREGIDO: Usar PagoPrincipal (propiedad calculada en Venta.cs)
                var pagoPrincipal = venta.PagoPrincipal;

                // ✅ CORREGIDO: Usar VentaDetalleViewModel
                var viewModel = new VentaDetalleViewModel
                {
                    Id = venta.Id,
                    NumeroOrden = venta.NumeroOrden,
                    FechaVenta = venta.FechaVenta,
                    Estado = venta.Estado,
                    Subtotal = venta.Subtotal,
                    DescuentoTotal = venta.DescuentoTotal,
                    Total = venta.Total,
                    DireccionEntrega = venta.DireccionEntrega,
                    Ciudad = venta.Ciudad,
                    Provincia = venta.Provincia,
                    CodigoPostal = venta.CodigoPostal,
                    NotasCliente = venta.NotasCliente,
                    FechaEntregaEstimada = venta.FechaEntregaEstimada,
                    FechaEntregaReal = venta.FechaEntregaReal,
                    RequiereFactura = venta.RequiereFactura,
                    CUITCliente = venta.CUITCliente,
                    RazonSocial = venta.RazonSocial,
                    
                    // Datos del comprador
                    CompradorId = venta.CompradorId,
                    NombreComprador = venta.Comprador?.Persona?.Nombre ?? "Cliente",
                    EmailComprador = venta.Comprador?.Persona?.Email ?? string.Empty,
                    TelefonoComprador = venta.Comprador?.Telefono ?? string.Empty,
                    
                    // Campos de comisiones
                    PorcentajeComision = venta.PorcentajeComision,
                    ComisionPlataforma = venta.ComisionPlataforma,
                    MontoVendedores = venta.MontoVendedores,
                    FondosLiberados = venta.FondosLiberados,
                    
                    // ✅ CORREGIDO: Usar PagoPrincipal en lugar de Pago
                    PagoId = pagoPrincipal?.Id,
                    EstadoPago = pagoPrincipal?.Estado ?? EstadoPago.Pendiente,
                    FechaPago = pagoPrincipal?.FechaPago,
                    MontoPagado = pagoPrincipal?.Monto ?? 0,
                    
                    // Detalles de productos
                    Detalles = venta.Detalles.Select(d => new DetalleVentaViewModel
                    {
                        Id = d.Id,
                        TortaId = d.TortaId,
                        NombreTorta = d.Torta?.Nombre ?? "Producto",
                        VendedorId = d.VendedorId,
                        NombreVendedor = d.Vendedor?.NombreComercial ?? "Vendedor",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Descuento = d.Descuento,
                        Subtotal = d.Subtotal,
                        Estado = d.Estado,
                        NotasPersonalizacion = d.NotasPersonalizacion,
                        FechaEstimadaPreparacion = d.FechaEstimadaPreparacion,
                        FechaRealPreparacion = d.FechaRealPreparacion,
                        ImagenTorta = d.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    }).ToList(),
                    
                    // Lista de todos los pagos
                    Pagos = venta.Pagos.Select(p => new PagoResumenViewModel
                    {
                        Id = p.Id,
                        Monto = p.Monto,
                        FechaPago = p.FechaPago,
                        Estado = p.Estado,
                        MetodoPago = p.MetodoPago,
                        NumeroTransaccion = p.NumeroTransaccion,
                        ArchivoComprobante = p.ArchivoComprobante
                    }).ToList()
                };

                // Determinar permisos
                viewModel.PuedeCancelar = venta.Estado == EstadoVenta.Pendiente;
                viewModel.PuedePagar = venta.Estado == EstadoVenta.Pendiente && !venta.Pagos.Any(p => p.Estado == EstadoPago.Completado);
                viewModel.PuedeVerFactura = venta.Estado == EstadoVenta.Pagada || venta.Estado == EstadoVenta.Entregada;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle de venta {VentaId}", id);
                TempData["Error"] = "Error al cargar los detalles de la venta";
                return RedirectToAction(nameof(MisCompras));
            }
        }

        // POST: Venta/Cancelar/5
        [HttpPost]
        [Authorize(Roles = "Comprador,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdWithDetallesAsync(id);
                if (venta == null)
                {
                    return Json(new { success = false, message = "Venta no encontrada" });
                }

                if (venta.Estado != EstadoVenta.Pendiente)
                {
                    return Json(new { success = false, message = "Solo se pueden cancelar ventas pendientes" });
                }

                // Restaurar stock de los productos
                foreach (var detalle in venta.Detalles)
                {
                    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(detalle.TortaId);
                    if (torta != null)
                    {
                        torta.Stock += detalle.Cantidad;
                        await _unitOfWork.TortaRepository.UpdateAsync(torta);
                    }
                }

                venta.Estado = EstadoVenta.Cancelada;
                venta.NotasInternas = (venta.NotasInternas ?? "") + $"\nCancelada por cliente: {motivo}";
                venta.FechaActualizacion = DateTime.Now;

                await _unitOfWork.Ventas.UpdateAsync(venta);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Venta cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar venta {VentaId}", id);
                return Json(new { success = false, message = "Error al cancelar la venta" });
            }
        }

        // POST: Venta/ActualizarEstadoDetalle (Para vendedores)
        [HttpPost]
        [Authorize(Roles = "Vendedor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoDetalle(int detalleId, EstadoDetalleVenta nuevoEstado)
        {
            try
            {
                var detalle = await _unitOfWork.DetallesVenta.GetByIdWithTodoAsync(detalleId);
                if (detalle == null)
                {
                    return Json(new { success = false, message = "Detalle no encontrado" });
                }

                // Verificar que el vendedor es el dueño del detalle
                var vendedorId = await ObtenerVendedorIdActual();
                if (vendedorId == null || detalle.VendedorId != vendedorId.Value)
                {
                    return Json(new { success = false, message = "No tiene permiso para actualizar este pedido" });
                }

                detalle.Estado = nuevoEstado;

                if (nuevoEstado == EstadoDetalleVenta.EnPreparacion)
                {
                    detalle.FechaRealPreparacion = DateTime.Now;
                }
                else if (nuevoEstado == EstadoDetalleVenta.Entregado)
                {
                    // Verificar si todos los detalles de la venta están entregados
                    var venta = await _unitOfWork.Ventas.GetByIdWithDetallesAsync(detalle.VentaId);
                    if (venta != null)
                    {
                        // Actualizar el detalle actual en memoria
                        var detalleEnVenta = venta.Detalles.FirstOrDefault(d => d.Id == detalleId);
                        if (detalleEnVenta != null)
                        {
                            detalleEnVenta.Estado = nuevoEstado;
                        }
                        
                        // Verificar si todos están entregados
                        if (venta.Detalles.All(d => d.Estado == EstadoDetalleVenta.Entregado))
                        {
                            venta.Estado = EstadoVenta.Entregada;
                            venta.FechaEntregaReal = DateTime.Now;
                            await _unitOfWork.Ventas.UpdateAsync(venta);
                        }
                    }
                }

                await _unitOfWork.DetallesVenta.UpdateAsync(detalle);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Estado actualizado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de detalle {DetalleId}", detalleId);
                return Json(new { success = false, message = "Error al actualizar el estado" });
            }
        }

        // GET: Venta/ImprimirFactura/5
        [Authorize]
        public async Task<IActionResult> ImprimirFactura(int id)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdWithTodoAsync(id);
                if (venta == null)
                {
                    TempData["Error"] = "Venta no encontrada";
                    return RedirectToAction(nameof(MisCompras));
                }

                if (!await TienePermisoVerVenta(venta))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Retornar vista de factura
                return View("Factura", venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir factura de venta {VentaId}", id);
                TempData["Error"] = "Error al generar la factura";
                return RedirectToAction(nameof(Details), new { id });
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

        private async Task<bool> TienePermisoVerVenta(Venta venta)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Comprador"))
            {
                var compradorId = await ObtenerCompradorIdActual();
                return compradorId == venta.CompradorId;
            }

            if (User.IsInRole("Vendedor"))
            {
                var vendedorId = await ObtenerVendedorIdActual();
                return vendedorId != null && venta.Detalles.Any(d => d.VendedorId == vendedorId);
            }

            return false;
        }
    }
}