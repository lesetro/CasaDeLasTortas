using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CasaDeLasTortas.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FileService _fileService;

        public PagoController(IUnitOfWork unitOfWork, FileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        // GET: Pago
        public async Task<IActionResult> Index(int pagina = 1, int tamanioPagina = 15, string busqueda = "", EstadoPago? filtroEstado = null, MetodoPago? filtroMetodoPago = null, int? filtroVendedorId = null, int? filtroCompradorId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string ordenarPor = "FechaPago_desc")
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetAllWithDetailsAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(busqueda))
                {
                    pagos = pagos.Where(p => 
                        p.Torta.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Comprador.Persona.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Vendedor.NombreComercial.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        (p.NumeroTransaccion != null && p.NumeroTransaccion.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                    );
                }

                if (filtroEstado.HasValue)
                {
                    pagos = pagos.Where(p => p.Estado == filtroEstado.Value);
                }

                if (filtroMetodoPago.HasValue)
                {
                    pagos = pagos.Where(p => p.MetodoPago == filtroMetodoPago.Value);
                }

                if (filtroVendedorId.HasValue)
                {
                    pagos = pagos.Where(p => p.VendedorId == filtroVendedorId.Value);
                }

                if (filtroCompradorId.HasValue)
                {
                    pagos = pagos.Where(p => p.CompradorId == filtroCompradorId.Value);
                }

                if (fechaDesde.HasValue)
                {
                    pagos = pagos.Where(p => p.FechaPago.Date >= fechaDesde.Value.Date);
                }

                if (fechaHasta.HasValue)
                {
                    pagos = pagos.Where(p => p.FechaPago.Date <= fechaHasta.Value.Date);
                }

                // Aplicar ordenamiento
                pagos = ordenarPor switch
                {
                    "Monto" => pagos.OrderBy(p => p.Monto),
                    "Monto_desc" => pagos.OrderByDescending(p => p.Monto),
                    "Estado" => pagos.OrderBy(p => p.Estado),
                    "Comprador" => pagos.OrderBy(p => p.Comprador.Persona.Nombre),
                    "Vendedor" => pagos.OrderBy(p => p.Vendedor.NombreComercial),
                    "Torta" => pagos.OrderBy(p => p.Torta.Nombre),
                    "FechaPago" => pagos.OrderBy(p => p.FechaPago),
                    _ => pagos.OrderByDescending(p => p.FechaPago)
                };

                // Calcular paginación
                var totalItems = pagos.Count();
                var pagosPaginados = pagos
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Mapear a ViewModels
                var pagosViewModel = pagosPaginados.Select(p => new PagoViewModel
                {
                    Id = p.Id,
                    TortaId = p.TortaId,
                    CompradorId = p.CompradorId,
                    VendedorId = p.VendedorId,
                    Monto = p.Monto,
                    PrecioUnitario = p.PrecioUnitario,
                    Cantidad = p.Cantidad,
                    Subtotal = p.Subtotal,
                    Descuento = p.Descuento,
                    FechaPago = p.FechaPago,
                    Estado = p.Estado,
                    MetodoPago = p.MetodoPago,
                    ArchivoComprobante = p.ArchivoComprobante,
                    NumeroTransaccion = p.NumeroTransaccion,
                    Observaciones = p.Observaciones,
                    DireccionEntrega = p.DireccionEntrega,
                    FechaEntrega = p.FechaEntrega,
                    NombreTorta = p.Torta.Nombre,
                    NombreComprador = p.Comprador.Persona.Nombre,
                    NombreVendedor = p.Vendedor.NombreComercial,
                    ImagenTorta = p.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    TelefonoComprador = p.Comprador.Telefono,
                    EmailComprador = p.Comprador.Persona.Email
                }).ToList();

                // Calcular estadísticas
                var estadisticas = new Dictionary<EstadoPago, int>();
                foreach (EstadoPago estado in Enum.GetValues<EstadoPago>())
                {
                    estadisticas[estado] = pagos.Count(p => p.Estado == estado);
                }

                var viewModel = new PagoListViewModel
                {
                    Pagos = pagosViewModel,
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                    FiltroEstado = filtroEstado,
                    FiltroMetodoPago = filtroMetodoPago,
                    FiltroVendedorId = filtroVendedorId,
                    FiltroCompradorId = filtroCompradorId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Busqueda = busqueda,
                    OrdenarPor = ordenarPor,
                    TotalMonto = pagos.Sum(p => p.Monto),
                    TotalPagos = totalItems,
                    PagosPorEstado = estadisticas
                };

                // Cargar datos para filtros
                await CargarDatosParaFiltros();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los pagos: {ex.Message}";
                return View(new PagoListViewModel { Pagos = new List<PagoViewModel>() });
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
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PagoDetalleViewModel
                {
                    Pago = new PagoViewModel
                    {
                        Id = pago.Id,
                        TortaId = pago.TortaId,
                        CompradorId = pago.CompradorId,
                        VendedorId = pago.VendedorId,
                        Monto = pago.Monto,
                        PrecioUnitario = pago.PrecioUnitario,
                        Cantidad = pago.Cantidad,
                        Subtotal = pago.Subtotal,
                        Descuento = pago.Descuento,
                        FechaPago = pago.FechaPago,
                        Estado = pago.Estado,
                        MetodoPago = pago.MetodoPago,
                        ArchivoComprobante = pago.ArchivoComprobante,
                        NumeroTransaccion = pago.NumeroTransaccion,
                        Observaciones = pago.Observaciones,
                        DireccionEntrega = pago.DireccionEntrega,
                        FechaEntrega = pago.FechaEntrega
                    },
                    Torta = new TortaViewModel
                    {
                        Id = pago.Torta.Id,
                        Nombre = pago.Torta.Nombre,
                        Descripcion = pago.Torta.Descripcion,
                        Precio = pago.Torta.Precio,
                        Categoria = pago.Torta.Categoria,
                        ImagenPrincipal = pago.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    },
                    Comprador = new CompradorViewModel
                    {
                        Id = pago.Comprador.Id,
                        NombrePersona = pago.Comprador.Persona.Nombre,
                        Email = pago.Comprador.Persona.Email,
                        Telefono = pago.Comprador.Telefono,
                        Direccion = pago.Comprador.Direccion,
                        Ciudad = pago.Comprador.Ciudad
                    },
                    Vendedor = new VendedorViewModel
                    {
                        Id = pago.Vendedor.Id,
                        NombreComercial = pago.Vendedor.NombreComercial,
                        Especialidad = pago.Vendedor.Especialidad,
                        NombrePersona = pago.Vendedor.Persona.Nombre,
                        Telefono = pago.Vendedor.Persona.Telefono
                    }
                };

                // Determinar permisos
                viewModel.PuedeEditar = await PuedeEditarPago(pago);
                viewModel.PuedeCancelar = pago.Estado == EstadoPago.Pendiente;
                viewModel.PuedeConfirmar = pago.Estado == EstadoPago.Pendiente && await EsVendedorDelPago(pago);
                viewModel.PuedeSubirComprobante = pago.Estado == EstadoPago.Pendiente && await EsCompradorDelPago(pago);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Pago/Create
        [Authorize(Roles = "Comprador,Admin")]
        public async Task<IActionResult> Create(int? tortaId = null)
        {
            try
            {
                var viewModel = new PagoCreateViewModel();
                
                if (tortaId.HasValue)
                {
                    var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesAsync(tortaId.Value);
                    if (torta != null && torta.Disponible && torta.Stock > 0)
                    {
                        viewModel.TortaId = torta.Id;
                        viewModel.VendedorId = torta.VendedorId;
                        viewModel.PrecioUnitario = torta.Precio;
                        viewModel.Cantidad = 1;
                        viewModel.Subtotal = torta.Precio;
                        viewModel.MontoTotal = torta.Precio;
                        
                        viewModel.Torta = new TortaViewModel
                        {
                            Id = torta.Id,
                            Nombre = torta.Nombre,
                            Precio = torta.Precio,
                            Stock = torta.Stock,
                            ImagenPrincipal = torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                        };
                    }
                }

                await CargarDatosParaCrearPago();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction("Index", "Torta");
            }
        }

        // POST: Pago/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Comprador,Admin")]
        public async Task<IActionResult> Create(PagoCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosParaCrearPago();
                return View(viewModel);
            }

            try
            {
                // Verificar que la torta existe y está disponible
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(viewModel.TortaId);
                if (torta == null || !torta.Disponible || torta.Stock < viewModel.Cantidad)
                {
                    ModelState.AddModelError("TortaId", "La torta no está disponible o no hay suficiente stock.");
                    await CargarDatosParaCrearPago();
                    return View(viewModel);
                }

                // Calcular montos
                var precioUnitario = torta.Precio;
                var subtotal = precioUnitario * viewModel.Cantidad;
                var descuento = viewModel.Descuento;
                var montoTotal = subtotal - descuento;

                var pago = new Pago
                {
                    TortaId = viewModel.TortaId,
                    CompradorId = viewModel.CompradorId,
                    VendedorId = torta.VendedorId,
                    Monto = montoTotal,
                    PrecioUnitario = precioUnitario,
                    Cantidad = viewModel.Cantidad,
                    Subtotal = subtotal,
                    Descuento = descuento,
                    FechaPago = DateTime.Now,
                    Estado = EstadoPago.Pendiente,
                    MetodoPago = viewModel.MetodoPago,
                    NumeroTransaccion = viewModel.NumeroTransaccion,
                    Observaciones = viewModel.Observaciones,
                    DireccionEntrega = viewModel.DireccionEntrega,
                    FechaEntrega = viewModel.FechaEntrega
                };

                // Manejar comprobante si se subió
                if (viewModel.ArchivoComprobante != null && viewModel.ArchivoComprobante.Length > 0)
                {
                    var urlComprobante = await _fileService.UploadFileAsync(viewModel.ArchivoComprobante, "comprobantes");
                    pago.ArchivoComprobante = urlComprobante;
                }

                await _unitOfWork.PagoRepository.AddAsync(pago);

                // Actualizar stock de la torta
                torta.Stock -= viewModel.Cantidad;
                await _unitOfWork.TortaRepository.UpdateAsync(torta);

                // Actualizar contador de compras del comprador
                var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(viewModel.CompradorId);
                if (comprador != null)
                {
                    comprador.TotalCompras++;
                    await _unitOfWork.CompradorRepository.UpdateAsync(comprador);
                }

                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Pago creado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = pago.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el pago: {ex.Message}";
                await CargarDatosParaCrearPago();
                return View(viewModel);
            }
        }

        // GET: Pago/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarPago(pago))
                {
                    TempData["Error"] = "No tiene permisos para editar este pago.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = new PagoEditViewModel
                {
                    Id = pago.Id,
                    Estado = pago.Estado,
                    MetodoPago = pago.MetodoPago,
                    NumeroTransaccion = pago.NumeroTransaccion,
                    Observaciones = pago.Observaciones,
                    DireccionEntrega = pago.DireccionEntrega,
                    FechaEntrega = pago.FechaEntrega,
                    ComprobanteActual = pago.ArchivoComprobante,
                    NombreTorta = pago.Torta.Nombre,
                    NombreComprador = pago.Comprador.Persona.Nombre,
                    Cantidad = pago.Cantidad,
                    MontoTotal = pago.Monto,
                    FechaPago = pago.FechaPago
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pago/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PagoEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de pago no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarPago(pago))
                {
                    TempData["Error"] = "No tiene permisos para editar este pago.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var estadoAnterior = pago.Estado;

                // Actualizar datos
                pago.Estado = viewModel.Estado;
                pago.MetodoPago = viewModel.MetodoPago;
                pago.NumeroTransaccion = viewModel.NumeroTransaccion;
                pago.Observaciones = viewModel.Observaciones;
                pago.DireccionEntrega = viewModel.DireccionEntrega;
                pago.FechaEntrega = viewModel.FechaEntrega;
                pago.FechaActualizacion = DateTime.Now;

                // Manejar nuevo comprobante
                if (viewModel.NuevoComprobante != null && viewModel.NuevoComprobante.Length > 0)
                {
                    // Eliminar comprobante anterior si existe
                    if (!string.IsNullOrEmpty(pago.ArchivoComprobante))
                    {
                        await _fileService.DeleteFileAsync(pago.ArchivoComprobante);
                    }

                    // Subir nuevo comprobante
                    var nuevoComprobante = await _fileService.UploadFileAsync(viewModel.NuevoComprobante, "comprobantes");
                    pago.ArchivoComprobante = nuevoComprobante;
                }

                // Manejar cambios de estado
                if (estadoAnterior != viewModel.Estado)
                {
                    await ProcesarCambioEstado(pago, estadoAnterior, viewModel.Estado);
                }

                await _unitOfWork.PagoRepository.UpdateAsync(pago);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Pago actualizado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = pago.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Pago/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEliminarPago(pago))
                {
                    TempData["Error"] = "No tiene permisos para eliminar este pago o el pago no puede ser eliminado.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = new PagoViewModel
                {
                    Id = pago.Id,
                    Monto = pago.Monto,
                    Cantidad = pago.Cantidad,
                    FechaPago = pago.FechaPago,
                    Estado = pago.Estado,
                    NombreTorta = pago.Torta.Nombre,
                    NombreComprador = pago.Comprador.Persona.Nombre,
                    NombreVendedor = pago.Vendedor.NombreComercial
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pago/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEliminarPago(pago))
                {
                    TempData["Error"] = "No tiene permisos para eliminar este pago o el pago no puede ser eliminado.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Restaurar stock si el pago estaba pendiente o completado
                if (pago.Estado == EstadoPago.Pendiente || pago.Estado == EstadoPago.Completado)
                {
                    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                    if (torta != null)
                    {
                        torta.Stock += pago.Cantidad;
                        await _unitOfWork.TortaRepository.UpdateAsync(torta);
                    }
                }

                // Eliminar comprobante si existe
                if (!string.IsNullOrEmpty(pago.ArchivoComprobante))
                {
                    await _fileService.DeleteFileAsync(pago.ArchivoComprobante);
                }

                await _unitOfWork.PagoRepository.DeleteAsync(pago);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Pago eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Pago/Confirmar/5
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Confirmar(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    TempData["Error"] = "Pago no encontrado o no puede ser confirmado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ConfirmarPagoViewModel
                {
                    PagoId = pago.Id,
                    Monto = pago.Monto,
                    NombreTorta = pago.Torta.Nombre,
                    Cantidad = pago.Cantidad,
                    NumeroTransaccion = pago.NumeroTransaccion ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pago/Confirmar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Confirmar(ConfirmarPagoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(viewModel.PagoId);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    TempData["Error"] = "Pago no encontrado o no puede ser confirmado.";
                    return RedirectToAction(nameof(Index));
                }

                pago.Estado = EstadoPago.Completado;
                pago.NumeroTransaccion = viewModel.NumeroTransaccion;
                pago.FechaActualizacion = DateTime.Now;

                if (!string.IsNullOrEmpty(viewModel.NotasConfirmacion))
                {
                    pago.Observaciones = (pago.Observaciones ?? "") + $"\nConfirmación: {viewModel.NotasConfirmacion}";
                }

                // Manejar comprobante adicional
                if (viewModel.ArchivoComprobante != null && viewModel.ArchivoComprobante.Length > 0)
                {
                    var urlComprobante = await _fileService.UploadFileAsync(viewModel.ArchivoComprobante, "comprobantes");
                    pago.ArchivoComprobante = urlComprobante;
                }

                // Actualizar estadísticas del vendedor
                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(pago.VendedorId);
                if (vendedor != null)
                {
                    vendedor.TotalVentas++;
                    await _unitOfWork.VendedorRepository.UpdateAsync(vendedor);
                }

                // Actualizar estadísticas de la torta
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                if (torta != null)
                {
                    torta.VecesVendida++;
                    await _unitOfWork.TortaRepository.UpdateAsync(torta);
                }

                await _unitOfWork.PagoRepository.UpdateAsync(pago);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Pago confirmado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = pago.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al confirmar el pago: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Pago/Cancelar/5
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(id);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    TempData["Error"] = "Pago no encontrado o no puede ser cancelado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CancelarPagoViewModel
                {
                    PagoId = pago.Id,
                    Monto = pago.Monto,
                    NombreTorta = pago.Torta.Nombre,
                    NombreComprador = pago.Comprador.Persona.Nombre
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pago/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(CancelarPagoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithDetallesAsync(viewModel.PagoId);
                if (pago == null || pago.Estado != EstadoPago.Pendiente)
                {
                    TempData["Error"] = "Pago no encontrado o no puede ser cancelado.";
                    return RedirectToAction(nameof(Index));
                }

                pago.Estado = EstadoPago.Cancelado;
                pago.Observaciones = (pago.Observaciones ?? "") + $"\nCancelado: {viewModel.MotivoCancelacion}";
                pago.FechaActualizacion = DateTime.Now;

                // Restaurar stock
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                if (torta != null)
                {
                    torta.Stock += pago.Cantidad;
                    await _unitOfWork.TortaRepository.UpdateAsync(torta);
                }

                await _unitOfWork.PagoRepository.UpdateAsync(pago);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Pago cancelado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = pago.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cancelar el pago: {ex.Message}";
                return View(viewModel);
            }
        }

        // AJAX: Calcular totales
        [HttpPost]
        public IActionResult CalcularTotales(decimal precioUnitario, int cantidad, decimal descuento = 0)
        {
            try
            {
                var subtotal = precioUnitario * cantidad;
                var montoTotal = subtotal - descuento;

                return Json(new
                {
                    subtotal = subtotal,
                    montoTotal = montoTotal,
                    success = true
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "Error al calcular totales" });
            }
        }

        // Métodos helper privados
        private async Task<bool> PuedeEditarPago(Pago pago)
        {
            // Implementar lógica de permisos
            return true; // Placeholder
        }

        private async Task<bool> PuedeEliminarPago(Pago pago)
        {
            // Solo se pueden eliminar pagos pendientes
            return pago.Estado == EstadoPago.Pendiente;
        }

        private async Task<bool> EsVendedorDelPago(Pago pago)
        {
            // Implementar verificación de usuario actual
            return true; // Placeholder
        }

        private async Task<bool> EsCompradorDelPago(Pago pago)
        {
            // Implementar verificación de usuario actual
            return true; // Placeholder
        }

        private async Task CargarDatosParaFiltros()
        {
            var vendedores = await _unitOfWork.VendedorRepository.GetAllWithPersonaAsync();
            var compradores = await _unitOfWork.CompradorRepository.GetAllWithPersonaAsync();

            ViewBag.Vendedores = new SelectList(vendedores.Select(v => new { 
                Id = v.Id, 
                Texto = v.NombreComercial 
            }), "Id", "Texto");

            ViewBag.Compradores = new SelectList(compradores.Select(c => new { 
                Id = c.Id, 
                Texto = c.Persona.Nombre 
            }), "Id", "Texto");
        }

        private async Task CargarDatosParaCrearPago()
        {
            var tortas = await _unitOfWork.TortaRepository.GetTortasDisponiblesAsync();
            var compradores = await _unitOfWork.CompradorRepository.GetAllWithPersonaAsync();

            ViewBag.TortaId = new SelectList(tortas.Select(t => new { 
                Id = t.Id, 
                Texto = $"{t.Nombre} - ${t.Precio} ({t.Stock} disponibles)" 
            }), "Id", "Texto");

            ViewBag.CompradorId = new SelectList(compradores.Select(c => new { 
                Id = c.Id, 
                Texto = $"{c.Persona.Nombre} - {c.Persona.Email}" 
            }), "Id", "Texto");
        }

        private async Task ProcesarCambioEstado(Pago pago, EstadoPago estadoAnterior, EstadoPago nuevoEstado)
        {
            // Lógica específica para cambios de estado
            if (estadoAnterior == EstadoPago.Pendiente && nuevoEstado == EstadoPago.Completado)
            {
                // Actualizar estadísticas cuando se completa un pago
                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(pago.VendedorId);
                if (vendedor != null)
                {
                    vendedor.TotalVentas++;
                    await _unitOfWork.VendedorRepository.UpdateAsync(vendedor);
                }

                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                if (torta != null)
                {
                    torta.VecesVendida++;
                    await _unitOfWork.TortaRepository.UpdateAsync(torta);
                }
            }
            else if (nuevoEstado == EstadoPago.Cancelado && estadoAnterior != EstadoPago.Cancelado)
            {
                // Restaurar stock cuando se cancela
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pago.TortaId);
                if (torta != null)
                {
                    torta.Stock += pago.Cantidad;
                    await _unitOfWork.TortaRepository.UpdateAsync(torta);
                }
            }
        }
    }
}