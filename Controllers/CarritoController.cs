using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Models.Entities;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(
            ICarritoService carritoService,
            IUnitOfWork unitOfWork,
            ILogger<CarritoController> logger)
        {
            _carritoService = carritoService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: Carrito
        public async Task<IActionResult> Index()
        {
            try
            {
                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                
                // Agrupar por vendedor para mostrar en la vista
                var itemsPorVendedor = carrito.Items
                    .GroupBy(i => i.VendedorId)
                    .Select(g => new CarritoPorVendedorViewModel
                    {
                        VendedorId = g.Key,
                        NombreVendedor = g.First().NombreVendedor,
                        Items = g.ToList(),
                        Subtotal = g.Sum(i => i.Subtotal),
                        Descuento = g.Sum(i => i.Descuento),
                        Total = g.Sum(i => i.Total)
                    })
                    .ToList();

                var viewModel = new CarritoViewModel
                {
                    Items = carrito.Items,
                    ItemsPorVendedor = itemsPorVendedor,
                    Subtotal = carrito.Subtotal,
                    DescuentoTotal = carrito.DescuentoTotal,
                    Total = carrito.Total,
                    TotalItems = carrito.TotalItems
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar carrito");
                TempData["Error"] = "Error al cargar el carrito";
                return View(new CarritoViewModel());
            }
        }

        // POST: Carrito/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar(int tortaId, int cantidad = 1, string? notas = null)
        {
            try
            {
                var resultado = await _carritoService.AgregarItem(tortaId, cantidad, notas);
                
                if (resultado)
                {
                    TempData["Success"] = "Producto agregado al carrito";
                }
                else
                {
                    TempData["Error"] = "No se pudo agregar el producto al carrito";
                }

                // Si es petición AJAX, devolver JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var carrito = await _carritoService.ObtenerCarritoConDetalles();
                    return Json(new 
                    { 
                        success = resultado, 
                        totalItems = carrito.TotalItems,
                        subtotal = carrito.Subtotal.ToString("C")
                    });
                }

                return RedirectToAction("Details", "Torta", new { id = tortaId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar item al carrito");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error al agregar al carrito" });
                }
                
                TempData["Error"] = "Error al agregar al carrito";
                return RedirectToAction("Details", "Torta", new { id = tortaId });
            }
        }

        // POST: Carrito/Quitar
        [HttpPost]
        public async Task<IActionResult> Quitar(int tortaId)
        {
            try
            {
                var resultado = await _carritoService.QuitarItem(tortaId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var carrito = await _carritoService.ObtenerCarritoConDetalles();
                    return Json(new 
                    { 
                        success = resultado, 
                        totalItems = carrito.TotalItems,
                        subtotal = carrito.Subtotal.ToString("C")
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar item del carrito");
                return Json(new { success = false });
            }
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int tortaId, int cantidad)
        {
            try
            {
                var resultado = await _carritoService.ActualizarCantidad(tortaId, cantidad);
                
                if (resultado)
                {
                    var carrito = await _carritoService.ObtenerCarritoConDetalles();
                    var item = carrito.Items.FirstOrDefault(i => i.TortaId == tortaId);
                    
                    return Json(new 
                    { 
                        success = true,
                        itemSubtotal = item?.Total.ToString("C"),
                        subtotal = carrito.Subtotal.ToString("C"),
                        total = carrito.Total.ToString("C"),
                        totalItems = carrito.TotalItems
                    });
                }
                
                return Json(new { success = false, message = "No hay suficiente stock" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cantidad");
                return Json(new { success = false, message = "Error al actualizar" });
            }
        }

        // POST: Carrito/ActualizarNotas
        [HttpPost]
        public async Task<IActionResult> ActualizarNotas(int tortaId, string? notas)
        {
            try
            {
                var resultado = await _carritoService.ActualizarNotas(tortaId, notas);
                return Json(new { success = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar notas");
                return Json(new { success = false });
            }
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        public IActionResult Vaciar()
        {
            _carritoService.VaciarCarrito();
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, totalItems = 0 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Carrito/Checkout
        [Authorize(Roles = "Comprador,Admin")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var carrito = await _carritoService.ObtenerCarritoConDetalles();
                
                if (!carrito.Items.Any())
                {
                    TempData["Info"] = "Tu carrito está vacío";
                    return RedirectToAction(nameof(Index));
                }

                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Carrito/Checkout" });
                }

                var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(compradorId.Value);
                
                var viewModel = new CheckoutViewModel
                {
                    Carrito = carrito,
                    DireccionEntrega = comprador?.Direccion ?? "",
                    Ciudad = comprador?.Ciudad,
                    Provincia = comprador?.Provincia,
                    CodigoPostal = comprador?.CodigoPostal,
                    TelefonoContacto = comprador?.Telefono,
                    EmailContacto = comprador?.Persona.Email,
                    Notas = "",
                    MetodoPago = MetodoPago.Transferencia,
                    UsarDireccionPerfil = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en checkout");
                TempData["Error"] = "Error al procesar el checkout";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Carrito/ProcesarCheckout
        [HttpPost]
        [Authorize(Roles = "Comprador,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarCheckout(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Checkout", model);
                }

                var compradorId = await ObtenerCompradorIdActual();
                if (compradorId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Crear la venta a partir del carrito
                var venta = await _carritoService.ConvertirAVenta(
                    compradorId.Value, 
                    model.DireccionEntregaCompleta,
                    model.Notas
                );

                if (venta == null)
                {
                    TempData["Error"] = "No se pudo crear la venta. El carrito podría estar vacío.";
                    return RedirectToAction(nameof(Index));
                }

                // Redirigir al pago
                return RedirectToAction("Procesar", "Pago", new { ventaId = venta.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación en checkout");
                ModelState.AddModelError("", ex.Message);
                return View("Checkout", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar checkout");
                TempData["Error"] = "Error al procesar la compra";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Carrito/Contador (para el badge del header)
        [HttpGet]
        public IActionResult Contador()
        {
            var total = _carritoService.GetTotalItems();
            return Json(new { total });
        }

        // Helper: Obtener CompradorId del usuario actual
        private async Task<int?> ObtenerCompradorIdActual()
        {
            if (!User.Identity?.IsAuthenticated == true)
                return null;

            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Comprador?.Id;
        }
    }
}