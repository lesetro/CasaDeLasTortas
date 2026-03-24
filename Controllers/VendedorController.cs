using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Controllers
{
    /// <summary>
    /// Controller MVC para Vendedores
    /// ✅ MODIFICADO: Incluye gestión de perfil y datos bancarios
    /// </summary>
    public class VendedorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILiberacionService _liberacionService;
        private readonly IFileService _fileService;
        private readonly ILogger<VendedorController> _logger;

        public VendedorController(
            IUnitOfWork unitOfWork,
            ILiberacionService liberacionService,
            IFileService fileService,
            ILogger<VendedorController> logger)
        {
            _unitOfWork = unitOfWork;
            _liberacionService = liberacionService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard Vue.js - La autenticación se maneja en el cliente
        /// </summary>
        [HttpGet]
        public IActionResult DashboardVue()
        {
            _logger.LogInformation("📍 Sirviendo vista DashboardVue (autenticación en cliente)");
            return View();
        }

        /// <summary>
        /// Dashboard tradicional (redirige a Vue)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("📍 Sirviendo vista Index");
            return View();
        }

        // ══════════════════════════════════════════════
        // ✅ NUEVO: PERFIL Y DATOS BANCARIOS (MVC)
        // ══════════════════════════════════════════════

        /// <summary>
        /// Vista de perfil del vendedor con datos bancarios
        /// Requiere autenticación por Cookie (para MVC tradicional)
        /// </summary>
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Perfil()
        {
            try
            {
                var vendedor = await ObtenerVendedorActual();
                if (vendedor == null)
                {
                    TempData["Error"] = "No tienes un perfil de vendedor";
                    return RedirectToAction("Index", "Home");
                }

                // Obtener resumen financiero
                var resumen = await _liberacionService.GetResumenVendedorAsync(vendedor.Id);
                ViewBag.ResumenFinanciero = resumen;

                // URL de imagen QR
                ViewBag.UrlImagenQR = !string.IsNullOrEmpty(vendedor.ImagenQR)
                    ? _fileService.GetFileUrl(vendedor.ImagenQR) : null;

                // Comisión de la plataforma
                ViewBag.ComisionPorcentaje = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();

                return View(vendedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar perfil de vendedor");
                TempData["Error"] = "Error al cargar el perfil";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Guardar datos bancarios del vendedor
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> GuardarDatosBancarios(
            string aliasCBU, 
            string cbu, 
            string banco, 
            string titularCuenta, 
            string? cuit,
            IFormFile? imagenQR)
        {
            try
            {
                var vendedor = await ObtenerVendedorActual();
                if (vendedor == null)
                {
                    TempData["Error"] = "No tienes un perfil de vendedor";
                    return RedirectToAction("Index", "Home");
                }

                // Validaciones
                if (string.IsNullOrWhiteSpace(aliasCBU) || 
                    string.IsNullOrWhiteSpace(cbu) || 
                    string.IsNullOrWhiteSpace(banco) || 
                    string.IsNullOrWhiteSpace(titularCuenta))
                {
                    TempData["Error"] = "Todos los campos son obligatorios";
                    return RedirectToAction(nameof(Perfil));
                }

                if (cbu.Length != 22)
                {
                    TempData["Error"] = "El CBU debe tener 22 dígitos";
                    return RedirectToAction(nameof(Perfil));
                }

                // Guardar imagen QR si se subió
                string? rutaImagenQR = vendedor.ImagenQR;
                if (imagenQR != null && imagenQR.Length > 0)
                {
                    rutaImagenQR = await _fileService.SaveFileAsync(imagenQR, "qr-vendedores");
                }

                // Actualizar datos
                await _unitOfWork.VendedorRepository.ActualizarDatosPagoAsync(
                    vendedor.Id, aliasCBU, cbu, banco, titularCuenta, cuit, rutaImagenQR);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Datos bancarios guardados correctamente";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar datos bancarios");
                TempData["Error"] = "Error al guardar los datos bancarios";
                return RedirectToAction(nameof(Perfil));
            }
        }

        /// <summary>
        /// Vista de liberaciones del vendedor
        /// </summary>
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> MisLiberaciones(int pagina = 1)
        {
            try
            {
                var vendedor = await ObtenerVendedorActual();
                if (vendedor == null)
                {
                    TempData["Error"] = "No tienes un perfil de vendedor";
                    return RedirectToAction("Index", "Home");
                }

                var liberaciones = await _unitOfWork.Liberaciones.GetByVendedorIdAsync(vendedor.Id);
                var tamanioPagina = 15;
                var totalItems = liberaciones.Count();

                var liberacionesPaginadas = liberaciones
                    .OrderByDescending(l => l.FechaCreacion)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                ViewBag.Paginacion = new Models.ViewModels.PaginacionViewModel(totalItems, pagina, tamanioPagina);
                ViewBag.ResumenFinanciero = await _liberacionService.GetResumenVendedorAsync(vendedor.Id);

                return View(liberacionesPaginadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar liberaciones");
                TempData["Error"] = "Error al cargar las liberaciones";
                return RedirectToAction(nameof(Perfil));
            }
        }

        /// <summary>
        /// Confirmar recepción de fondos (desde vista MVC)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> ConfirmarRecepcion(int liberacionId)
        {
            try
            {
                var vendedor = await ObtenerVendedorActual();
                if (vendedor == null)
                {
                    TempData["Error"] = "No tienes un perfil de vendedor";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _liberacionService.ConfirmarRecepcionVendedorAsync(liberacionId, vendedor.Id);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(MisLiberaciones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar recepción de liberación {Id}", liberacionId);
                TempData["Error"] = "Error al confirmar la recepción";
                return RedirectToAction(nameof(MisLiberaciones));
            }
        }

        /// <summary>
        /// Vista de estadísticas financieras del vendedor
        /// </summary>
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> MisFinanzas()
        {
            try
            {
                var vendedor = await ObtenerVendedorActual();
                if (vendedor == null)
                {
                    TempData["Error"] = "No tienes un perfil de vendedor";
                    return RedirectToAction("Index", "Home");
                }

                var resumen = await _liberacionService.GetResumenVendedorAsync(vendedor.Id);
                var comision = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();

                // Últimas liberaciones
                var ultimasLiberaciones = (await _unitOfWork.Liberaciones.GetByVendedorIdAsync(vendedor.Id))
                    .OrderByDescending(l => l.FechaCreacion)
                    .Take(5)
                    .ToList();

                ViewBag.ResumenFinanciero = resumen;
                ViewBag.ComisionPorcentaje = comision;
                ViewBag.UltimasLiberaciones = ultimasLiberaciones;

                return View(vendedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar finanzas del vendedor");
                TempData["Error"] = "Error al cargar las finanzas";
                return RedirectToAction(nameof(Perfil));
            }
        }

        // ══════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════

        private async Task<Vendedor?> ObtenerVendedorActual()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(email);
            if (persona?.Vendedor == null)
                return null;

            return await _unitOfWork.VendedorRepository.GetByIdWithPersonaAsync(persona.Vendedor.Id);
        }
    }
}