using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Interfaces;
using System.Security.Claims;
using CasaDeLasTortas.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: Home/Index - Página principal con catálogo público de tortas
        public async Task<IActionResult> Index(int pagina = 1, int tamanioPagina = 12, string busqueda = "", string categoria = "", string ordenarPor = "Populares")
        {
            try
            {
                // Si el usuario está autenticado, redirigir según su rol
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                    // Si es vendedor, mostrar su dashboard
                    if (userRole == "Vendedor")
                    {
                        return await DashboardVendedor();
                    }
                    // Si es comprador, mostrar catálogo personalizado
                    else if (userRole == "Comprador")
                    {
                        return await CatalogoComprador(pagina, tamanioPagina, busqueda, categoria, ordenarPor);
                    }
                    // Si es admin, mostrar estadísticas generales
                    else if (userRole == "Admin")
                    {
                        return await DashboardAdmin();
                    }
                }

                // Para usuarios no autenticados, mostrar catálogo público
                return await CatalogoPublico(pagina, tamanioPagina, busqueda, categoria, ordenarPor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la página principal");
                TempData["Error"] = "Error al cargar el contenido. Por favor, intente nuevamente.";
                return View("Error");
            }
        }

        // Vista de catálogo público para usuarios no autenticados
        private async Task<IActionResult> CatalogoPublico(int pagina, int tamanioPagina, string busqueda, string categoria, string ordenarPor)
        {
            var tortas = await _unitOfWork.TortaRepository.GetTortasDisponiblesConDetallesAsync();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(busqueda))
            {
                tortas = tortas.Where(t =>
                    t.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    t.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    (t.Categoria != null && t.Categoria.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    t.Vendedor.NombreComercial.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                tortas = tortas.Where(t => t.Categoria == categoria);
            }

            // Aplicar ordenamiento
            tortas = ordenarPor switch
            {
                "Nuevas" => tortas.OrderByDescending(t => t.FechaCreacion),
                "PrecioMenor" => tortas.OrderBy(t => t.Precio),
                "PrecioMayor" => tortas.OrderByDescending(t => t.Precio),
                "Calificacion" => tortas.OrderByDescending(t => t.Calificacion),
                "Nombre" => tortas.OrderBy(t => t.Nombre),
                _ => tortas.OrderByDescending(t => t.VecesVendida) // Populares por defecto
            };

            // Paginación
            var totalItems = tortas.Count();
            var tortasPaginadas = tortas
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToList();

            // Mapear a ViewModels
            var tortasViewModel = tortasPaginadas.Select(t => new CasaDeLasTortas.Models.ViewModels.TortaCatalogoDTO
            {
                Id = t.Id,
                Nombre = t.Nombre,
                DescripcionCorta = t.Descripcion.Length > 100 ? t.Descripcion.Substring(0, 100) + "..." : t.Descripcion,
                Precio = t.Precio,
                Categoria = t.Categoria,
                Calificacion = t.Calificacion,
                NombreVendedor = t.Vendedor.NombreComercial,
                ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                Disponible = t.Disponible,
                EsNuevo = t.FechaCreacion >= DateTime.Now.AddDays(-7),
                EsPopular = t.VecesVendida >= 5,
                
            }).ToList();

            // Obtener categorías para filtros
            var categorias = await _unitOfWork.TortaRepository.GetCategoriasAsync();

            // Tortas destacadas (las más vendidas)
            var tortasDestacadasEntities = await _unitOfWork.TortaRepository.GetTortasDestacadasAsync(6);
            var tortasDestacadas = tortasDestacadasEntities.Select(t => new TortaDestacadaDTO
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Precio = t.Precio,
                Calificacion = t.Calificacion,
                NombreVendedor = t.Vendedor.NombreComercial,
                ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
            }).ToList();

            // Estadísticas para mostrar
            var estadisticas = new EstadisticasHomeViewModel
            {
                TotalTortas = totalItems,
                TotalVendedores = await _unitOfWork.VendedorRepository.GetCountAsync(),
                VentasHoy = await _unitOfWork.PagoRepository.GetVentasDelDiaAsync(),
                TortasNuevas = tortasPaginadas.Count(t => t.FechaCreacion >= DateTime.Now.AddDays(-7))
            };

            var viewModel = new HomeViewModel
            {
                Tortas = tortasViewModel,
                TortasDestacadas = tortasDestacadas,
                Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                Filtros = new FiltrosHomeViewModel
                {
                    Busqueda = busqueda,
                    Categoria = categoria,
                    OrdenarPor = ordenarPor,
                    CategoriasDisponibles = categorias
                },
                Estadisticas = estadisticas
            };

            ViewBag.Title = "Casa de las Tortas - Descubre las mejores tortas artesanales";
            return View("Index", viewModel);
        }

        // Vista personalizada para compradores autenticados
        private async Task<IActionResult> CatalogoComprador(int pagina, int tamanioPagina, string busqueda, string categoria, string ordenarPor)
        {
            var compradorId = await ObtenerCompradorIdActual();
            if (compradorId == null)
            {
                return await CatalogoPublico(pagina, tamanioPagina, busqueda, categoria, ordenarPor);
            }

            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(compradorId.Value);

            // Obtener tortas disponibles
            var tortas = await _unitOfWork.TortaRepository.GetTortasDisponiblesConDetallesAsync();

            // Aplicar filtros de preferencias del comprador si las tiene
            if (!string.IsNullOrEmpty(comprador?.Preferencias))
            {
                var preferencias = comprador.Preferencias.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (!string.IsNullOrEmpty(categoria))
                {
                    tortas = tortas.Where(t => t.Categoria == categoria);
                }
                else
                {
                    // Priorizar tortas de categorías preferidas
                    var tortasPreferidas = tortas.Where(t => preferencias.Any(p =>
                        t.Categoria?.Contains(p.Trim(), StringComparison.OrdinalIgnoreCase) == true));
                    var tortasOtras = tortas.Where(t => !preferencias.Any(p =>
                        t.Categoria?.Contains(p.Trim(), StringComparison.OrdinalIgnoreCase) == true));
                    tortas = tortasPreferidas.Concat(tortasOtras);
                }
            }

            // Aplicar otros filtros y ordenamiento igual que en catálogo público
            if (!string.IsNullOrEmpty(busqueda))
            {
                tortas = tortas.Where(t =>
                    t.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    t.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    (t.Categoria != null && t.Categoria.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    t.Vendedor.NombreComercial.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                );
            }

            tortas = ordenarPor switch
            {
                "Nuevas" => tortas.OrderByDescending(t => t.FechaCreacion),
                "PrecioMenor" => tortas.OrderBy(t => t.Precio),
                "PrecioMayor" => tortas.OrderByDescending(t => t.Precio),
                "Calificacion" => tortas.OrderByDescending(t => t.Calificacion),
                "Nombre" => tortas.OrderBy(t => t.Nombre),
                _ => tortas.OrderByDescending(t => t.VecesVendida)
            };

            // Paginación
            var totalItems = tortas.Count();
            var tortasPaginadas = tortas.Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina).ToList();

            // Mapear tortas
            var tortasViewModel = tortasPaginadas.Select(t => new CasaDeLasTortas.Models.ViewModels.TortaCatalogoDTO
            {
                Id = t.Id,
                Nombre = t.Nombre,
                DescripcionCorta = t.Descripcion.Length > 100 ? t.Descripcion.Substring(0, 100) + "..." : t.Descripcion,
                Precio = t.Precio,
                Categoria = t.Categoria,
                Calificacion = t.Calificacion,
                NombreVendedor = t.Vendedor.NombreComercial,
                ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                Disponible = t.Disponible,
                EsNuevo = t.FechaCreacion >= DateTime.Now.AddDays(-7),
                EsPopular = t.VecesVendida >= 5,
                
            }).ToList();

            // Obtener historial de compras del comprador
            var historialCompras = await _unitOfWork.PagoRepository.GetByCompradorIdAsync(compradorId.Value);
            var ventasMes = historialCompras.Where(p => p.FechaPago >= DateTime.Now.AddDays(-30) && p.Estado == EstadoPago.Completado);

            // Recomendaciones basadas en compras anteriores
            var categoriasCompradas = historialCompras
                .Where(p => p.Estado == EstadoPago.Completado)
                .Select(p => p.Torta.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToList();

            var tortasRecomendadasEntities = await _unitOfWork.TortaRepository.GetTortasPorCategoriasAsync(categoriasCompradas, 6);
            var tortasRecomendadas = tortasRecomendadasEntities.Select(t => new CasaDeLasTortas.Models.ViewModels.TortaCatalogoDTO
            {
                Id = t.Id,
                Nombre = t.Nombre,
                DescripcionCorta = t.Descripcion.Length > 100 ? t.Descripcion.Substring(0, 100) + "..." : t.Descripcion,
                Precio = t.Precio,
                Categoria = t.Categoria,
                Calificacion = t.Calificacion,
                NombreVendedor = t.Vendedor.NombreComercial,
                ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                Disponible = t.Disponible,
                EsNuevo = t.FechaCreacion >= DateTime.Now.AddDays(-7),
                EsPopular = t.VecesVendida >= 5,
               
            }).ToList();

            var viewModel = new CompradorDashboardViewModel
            {
                Tortas = tortasViewModel,
                TortasRecomendadas = tortasRecomendadas,
                Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                Comprador = new CompradorInfoViewModel
                {
                    Nombre = comprador?.Persona.Nombre,
                    TotalCompras = comprador?.TotalCompras ?? 0,
                    GastoMes = ventasMes.Sum(p => p.Monto),
                    PedidosPendientes = historialCompras.Count(p => p.Estado == EstadoPago.Pendiente)
                },
                Filtros = new FiltrosHomeViewModel
                {
                    Busqueda = busqueda,
                    Categoria = categoria,
                    OrdenarPor = ordenarPor,
                    CategoriasDisponibles = await _unitOfWork.TortaRepository.GetCategoriasAsync()
                },
                Estadisticas = new CompradorStatsViewModel
                {
                    TotalCompras = comprador?.TotalCompras ?? 0,
                    TotalGastado = historialCompras.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto),
                    PedidosActivos = historialCompras.Count(p => p.Estado == EstadoPago.Pendiente),
                    ComprasMes = ventasMes.Count()
                }
            };

            ViewBag.Title = $"Bienvenido {comprador?.Persona.Nombre} - Casa de las Tortas";
            return View("IndexComprador", viewModel);
        }

        // Dashboard para vendedores
        private async Task<IActionResult> DashboardVendedor()
        {
            var vendedorId = await ObtenerVendedorIdActual();
            if (vendedorId == null)
            {
                return RedirectToAction("Create", "Vendedor");
            }

            return RedirectToAction("Dashboard", "Vendedor", new { id = vendedorId });
        }

        // Dashboard para administradores
        private async Task<IActionResult> DashboardAdmin()
        {
            var estadisticas = new
            {
                TotalTortas = await _unitOfWork.TortaRepository.CountAsync(),
                TotalVendedores = await _unitOfWork.VendedorRepository.GetCountAsync(),
                TotalCompradores = await _unitOfWork.CompradorRepository.CountAsync(),
                VentasHoy = await _unitOfWork.PagoRepository.GetVentasDelDiaAsync(),
                VentasMes = await _unitOfWork.PagoRepository.GetVentasDelMesAsync(),
                PagosPendientes = await _unitOfWork.PagoRepository.CountByEstadoAsync(EstadoPago.Pendiente),
                TortasNuevasHoy = await _unitOfWork.TortaRepository.GetTortasNuevasHoyAsync()
            };

            ViewBag.Title = "Panel de Administración - Casa de las Tortas";
            return View("IndexAdmin", estadisticas);
        }

        // Búsqueda AJAX para autocompletado
        [HttpGet]
        public async Task<IActionResult> BusquedaRapida(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 2)
                {
                    return Json(new List<object>());
                }

                var tortas = await _unitOfWork.TortaRepository.GetTortasDisponiblesAsync();
                var resultados = tortas
                    .Where(t =>
                        t.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                        (t.Categoria != null && t.Categoria.Contains(termino, StringComparison.OrdinalIgnoreCase)) ||
                        t.Vendedor.NombreComercial.Contains(termino, StringComparison.OrdinalIgnoreCase)
                    )
                    .Take(8)
                    .Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre,
                        precio = t.Precio,
                        categoria = t.Categoria,
                        vendedor = t.Vendedor.NombreComercial,
                        imagen = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                        url = Url.Action("Details", "Torta", new { id = t.Id })
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // Obtener tortas por categoría (AJAX)
        [HttpGet]
        public async Task<IActionResult> TortasPorCategoria(string categoria, int cantidad = 6)
        {
            try
            {
                var tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(categoria);
                var resultado = tortas
                    .Where(t => t.Disponible && t.Stock > 0)
                    .Take(cantidad)
                    .Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre,
                        precio = t.Precio,
                        calificacion = t.Calificacion,
                        imagen = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                        vendedor = t.Vendedor.NombreComercial,
                        url = Url.Action("Details", "Torta", new { id = t.Id })
                    })
                    .ToList();

                return Json(resultado);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // Página de política de privacidad
        public IActionResult Privacy()
        {
            return View();
        }

        // Página de términos y condiciones
        public IActionResult Terms()
        {
            return View();
        }

        // Página de contacto
        public IActionResult Contact()
        {
            return View();
        }

        // Página de información sobre la empresa
        public IActionResult About()
        {
            return View();
        }

        // Manejo de errores
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Métodos helper privados
        private async Task<int?> ObtenerCompradorIdActual()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Comprador?.Id;
        }

        private async Task<int?> ObtenerVendedorIdActual()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            var userEmail = User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var persona = await _unitOfWork.PersonaRepository.GetByEmailAsync(userEmail);
            return persona?.Vendedor?.Id;
        }
    }
}