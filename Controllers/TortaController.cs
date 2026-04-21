using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CasaDeLasTortas.Controllers
{
    [Authorize]
    public class TortaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<TortaController> _logger; 

        public TortaController(IUnitOfWork unitOfWork, IFileService  fileService, ILogger<TortaController> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: Torta
        public async Task<IActionResult> Index(int pagina = 1, int tamanioPagina = 12, string busqueda = "", string filtroCategoria = "", int? filtroVendedorId = null, decimal? precioMinimo = null, decimal? precioMaximo = null, bool? soloDisponibles = true, string ordenarPor = "Nombre")
        {
            try
            {
                IEnumerable<Torta> tortas;

                // Obtener tortas según filtros
                if (!string.IsNullOrEmpty(busqueda))
                {
                    tortas = await _unitOfWork.TortaRepository.SearchAsync(busqueda);
                }
                else if (!string.IsNullOrEmpty(filtroCategoria))
                {
                    tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(filtroCategoria);
                }
                else if (filtroVendedorId.HasValue)
                {
                    tortas = await _unitOfWork.TortaRepository.GetByVendedorIdAsync(filtroVendedorId.Value);
                }
                else
                {
                    tortas = await _unitOfWork.TortaRepository.GetAllWithVendedorAsync();
                }

                // Aplicar filtros adicionales en memoria
                if (precioMinimo.HasValue)
                {
                    tortas = tortas.Where(t => t.Precio >= precioMinimo.Value);
                }

                if (precioMaximo.HasValue)
                {
                    tortas = tortas.Where(t => t.Precio <= precioMaximo.Value);
                }

                if (soloDisponibles == true)
                {
                    tortas = tortas.Where(t => t.Disponible && t.Stock > 0);
                }

                // Aplicar ordenamiento
                tortas = ordenarPor switch
                {
                    "Precio" => tortas.OrderBy(t => t.Precio),
                    "Precio_desc" => tortas.OrderByDescending(t => t.Precio),
                    "Calificacion" => tortas.OrderByDescending(t => t.Calificacion),
                    "VecesVendida" => tortas.OrderByDescending(t => t.VecesVendida),
                    "FechaCreacion" => tortas.OrderBy(t => t.FechaCreacion),
                    "FechaCreacion_desc" => tortas.OrderByDescending(t => t.FechaCreacion),
                    "Vendedor" => tortas.OrderBy(t => t.Vendedor.NombreComercial),
                    "Nombre_desc" => tortas.OrderByDescending(t => t.Nombre),
                    _ => tortas.OrderBy(t => t.Nombre)
                };

                // Calcular paginación
                var totalItems = tortas.Count();
                var tortasPaginadas = tortas
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Mapear a ViewModels
                var tortasViewModel = tortasPaginadas.Select(t => new TortaViewModel
                {
                    Id = t.Id,
                    VendedorId = t.VendedorId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Precio = t.Precio,
                    Stock = t.Stock,
                    Categoria = t.Categoria,
                    Tamanio = t.Tamanio,
                    TiempoPreparacion = t.TiempoPreparacion,
                    Ingredientes = t.Ingredientes,
                    Personalizable = t.Personalizable,
                    VecesVendida = t.VecesVendida,
                    Calificacion = t.Calificacion,
                    Disponible = t.Disponible,
                    FechaCreacion = t.FechaCreacion,
                    FechaActualizacion = t.FechaActualizacion,
                    NombreVendedor = t.Vendedor?.NombreComercial,
                    EspecialidadVendedor = t.Vendedor?.Especialidad,
                    ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    Imagenes = t.Imagenes?.Select(i => new ImagenTortaViewModel
                    {
                        Id = i.Id,
                        UrlImagen = i.UrlImagen,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList() ?? new List<ImagenTortaViewModel>()
                }).ToList();

                // Datos para filtros
                var vendedores = await _unitOfWork.VendedorRepository.GetAllWithPersonaAsync();
                var categoriasDisponibles = await ObtenerCategorias();

                var viewModel = new TortaListViewModel
                {
                    Tortas = tortasViewModel,
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                    FiltroCategoria = filtroCategoria,
                    FiltroVendedor = filtroVendedorId?.ToString(),
                    PrecioMinimo = precioMinimo,
                    PrecioMaximo = precioMaximo,
                    SoloDisponibles = soloDisponibles,
                    Busqueda = busqueda,
                    OrdenarPor = ordenarPor,
                    CategoriasDisponibles = categoriasDisponibles,
                    VendedoresDisponibles = vendedores.Select(v => new VendedorViewModel
                    {
                        Id = v.Id,
                        NombreComercial = v.NombreComercial,
                        Nombre = v.Persona?.Nombre
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las tortas: {ex.Message}";
                return View(new TortaListViewModel { Tortas = new List<TortaViewModel>() });
            }
        }

        // GET: Torta/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new TortaDetalleViewModel
                {
                    Torta = new TortaViewModel
                    {
                        Id = torta.Id,
                        VendedorId = torta.VendedorId,
                        Nombre = torta.Nombre,
                        Descripcion = torta.Descripcion,
                        Precio = torta.Precio,
                        Stock = torta.Stock,
                        Categoria = torta.Categoria,
                        Tamanio = torta.Tamanio,
                        TiempoPreparacion = torta.TiempoPreparacion,
                        Ingredientes = torta.Ingredientes,
                        Personalizable = torta.Personalizable,
                        VecesVendida = torta.VecesVendida,
                        Calificacion = torta.Calificacion,
                        Disponible = torta.Disponible,
                        FechaCreacion = torta.FechaCreacion,
                        FechaActualizacion = torta.FechaActualizacion,
                        Imagenes = torta.Imagenes?.OrderBy(i => i.Orden).Select(i => new ImagenTortaViewModel
                        {
                            Id = i.Id,
                            UrlImagen = i.UrlImagen,
                            NombreArchivo = i.NombreArchivo,
                            EsPrincipal = i.EsPrincipal,
                            Orden = i.Orden,
                            FechaSubida = i.FechaSubida
                        }).ToList() ?? new List<ImagenTortaViewModel>()
                    },
                    Vendedor = new VendedorViewModel
                    {
                        Id = torta.Vendedor.Id,
                        NombreComercial = torta.Vendedor.NombreComercial,
                        Especialidad = torta.Vendedor.Especialidad,
                        Descripcion = torta.Vendedor.Descripcion,
                        Calificacion = torta.Vendedor.Calificacion,
                        TotalVentas = torta.Vendedor.TotalVentas,
                        Verificado = torta.Vendedor.Verificado,
                        Nombre = torta.Vendedor.Persona?.Nombre,
                        Avatar = torta.Vendedor.Persona?.Avatar,
                        Telefono = torta.Vendedor.Persona?.Telefono
                    }
                };

                // Determinar permisos del usuario actual
                viewModel.PuedeEditar = await PuedeEditarTorta(torta.VendedorId);
                viewModel.PuedeComprar = torta.Disponible && torta.Stock > 0;

                // Obtener tortas relacionadas (misma categoría o mismo vendedor)
                var tortasRelacionadas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(torta.Categoria ?? "");
                viewModel.TortasRelacionadas = tortasRelacionadas
                    .Where(t => t.Id != id && t.Disponible)
                    .Take(4)
                    .Select(t => new TortaViewModel
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Precio = t.Precio,
                        Calificacion = t.Calificacion,
                        NombreVendedor = t.Vendedor?.NombreComercial,
                        ImagenPrincipal = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Torta/Create
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var vendedores = await _unitOfWork.VendedorRepository.GetAllWithPersonaAsync();

                ViewBag.VendedorId = new SelectList(
                    vendedores.Select(v => new
                    {
                        Id = v.Id,
                        Texto = $"{v.NombreComercial} - {v.Persona?.Nombre}"
                    }),
                    "Id",
                    "Texto"
                );

                ViewBag.Categorias = await ObtenerCategorias();
                ViewBag.Tamanios = ObtenerTamanios();

                return View(new TortaCreateViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Torta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Create(TortaCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosParaFormulario();
                return View(viewModel);
            }

            try
            {
                // Verificar que el vendedor existe
                var vendedor = await _unitOfWork.VendedorRepository.GetByIdAsync(viewModel.VendedorId);
                if (vendedor == null)
                {
                    ModelState.AddModelError("VendedorId", "Vendedor no encontrado.");
                    await CargarDatosParaFormulario();
                    return View(viewModel);
                }

                var torta = new Torta
                {
                    VendedorId = viewModel.VendedorId,
                    Nombre = viewModel.Nombre,
                    Descripcion = viewModel.Descripcion,
                    Precio = viewModel.Precio,
                    Stock = viewModel.Stock,
                    Categoria = viewModel.Categoria,
                    Tamanio = viewModel.Tamanio,
                    TiempoPreparacion = viewModel.TiempoPreparacion,
                    Ingredientes = viewModel.Ingredientes,
                    Personalizable = viewModel.Personalizable,
                    Disponible = true,
                    FechaCreacion = DateTime.Now
                };

                await _unitOfWork.TortaRepository.AddAsync(torta);
                await _unitOfWork.SaveAsync();

                // Manejar imágenes si se subieron
                if (viewModel.Imagenes != null && viewModel.Imagenes.Any())
                {
                    await ProcesarImagenesTorta(torta.Id, viewModel.Imagenes, viewModel.ImagenPrincipalIndex);
                }

                TempData["Success"] = "Torta creada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = torta.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la torta: {ex.Message}";
                await CargarDatosParaFormulario();
                return View(viewModel);
            }
        }

        // GET: Torta/Edit/5
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (!await PuedeEditarTorta(torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para editar esta torta.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = new TortaEditViewModel
                {
                    Id = torta.Id,
                    VendedorId = torta.VendedorId,
                    Nombre = torta.Nombre,
                    Descripcion = torta.Descripcion,
                    Precio = torta.Precio,
                    Stock = torta.Stock,
                    Categoria = torta.Categoria,
                    Tamanio = torta.Tamanio,
                    TiempoPreparacion = torta.TiempoPreparacion,
                    Ingredientes = torta.Ingredientes,
                    Personalizable = torta.Personalizable,
                    Disponible = torta.Disponible,
                    ImagenesActuales = torta.Imagenes?.OrderBy(i => i.Orden).Select(i => new ImagenTortaViewModel
                    {
                        Id = i.Id,
                        UrlImagen = i.UrlImagen,
                        NombreArchivo = i.NombreArchivo,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList() ?? new List<ImagenTortaViewModel>()
                };

                ViewBag.Categorias = await ObtenerCategorias();
                ViewBag.Tamanios = ObtenerTamanios();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Torta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Edit(int id, TortaEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de torta no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategorias();
                ViewBag.Tamanios = ObtenerTamanios();
                return View(viewModel);
            }

            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (!await PuedeEditarTorta(torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para editar esta torta.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Actualizar datos
                torta.Nombre = viewModel.Nombre;
                torta.Descripcion = viewModel.Descripcion;
                torta.Precio = viewModel.Precio;
                torta.Stock = viewModel.Stock;
                torta.Categoria = viewModel.Categoria;
                torta.Tamanio = viewModel.Tamanio;
                torta.TiempoPreparacion = viewModel.TiempoPreparacion;
                torta.Ingredientes = viewModel.Ingredientes;
                torta.Personalizable = viewModel.Personalizable;
                torta.Disponible = viewModel.Disponible;
                torta.FechaActualizacion = DateTime.Now;

                _unitOfWork.TortaRepository.Update(torta);

                // Manejar eliminación de imágenes
                if (viewModel.ImagenesAEliminar != null && viewModel.ImagenesAEliminar.Any())
                {
                    await EliminarImagenesTorta(viewModel.ImagenesAEliminar);
                }

                // Manejar nuevas imágenes
                if (viewModel.NuevasImagenes != null && viewModel.NuevasImagenes.Any())
                {
                    await ProcesarImagenesTorta(id, viewModel.NuevasImagenes, null);
                }

                // Cambiar imagen principal si se especificó
                if (viewModel.NuevaImagenPrincipalId.HasValue)
                {
                    await CambiarImagenPrincipal(id, viewModel.NuevaImagenPrincipalId.Value);
                }

                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Torta actualizada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = torta.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                ViewBag.Categorias = await ObtenerCategorias();
                ViewBag.Tamanios = ObtenerTamanios();
                return View(viewModel);
            }
        }

        // GET: Torta/Delete/5
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (!await PuedeEditarTorta(torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para eliminar esta torta.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = new TortaViewModel
                {
                    Id = torta.Id,
                    Nombre = torta.Nombre,
                    Descripcion = torta.Descripcion,
                    Precio = torta.Precio,
                    Stock = torta.Stock,
                    VecesVendida = torta.VecesVendida,
                    NombreVendedor = torta.Vendedor?.NombreComercial,
                    ImagenPrincipal = torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Torta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(id);
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (!await PuedeEditarTorta(torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para eliminar esta torta.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                //  Verificar si tiene detalles de venta asociados (en lugar de pagos directos)
                var tieneDetallesVenta = torta.DetallesVenta != null && torta.DetallesVenta.Any();
                if (tieneDetallesVenta)
                {
                    TempData["Error"] = "No se puede eliminar la torta porque tiene ventas asociadas.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Eliminar imágenes asociadas
                if (torta.Imagenes != null && torta.Imagenes.Any())
                {
                    foreach (var imagen in torta.Imagenes)
                    {
                        await _fileService.DeleteFileAsync(imagen.UrlImagen);
                        _unitOfWork.ImagenesTorta.Delete(imagen);
                    }
                }

                _unitOfWork.TortaRepository.Delete(torta);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Torta eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar torta {TortaId}", id);
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // AJAX: Búsqueda de tortas
        [HttpGet]
        public async Task<IActionResult> BuscarTortas(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 2)
                {
                    return Json(new List<object>());
                }

                var tortas = await _unitOfWork.TortaRepository.SearchAsync(termino);
                var resultados = tortas
                    .Where(t => t.Disponible && t.Stock > 0)
                    .Take(10)
                    .Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre,
                        precio = t.Precio,
                        categoria = t.Categoria,
                        vendedor = t.Vendedor?.NombreComercial,
                        imagen = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                        stock = t.Stock
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // AJAX: Obtener tortas por categoría
        [HttpGet]
        public async Task<IActionResult> TortasPorCategoria(string categoria)
        {
            try
            {
                var tortas = await _unitOfWork.TortaRepository.GetByCategoriaAsync(categoria);
                var resultados = tortas
                    .Where(t => t.Disponible && t.Stock > 0)
                    .Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre,
                        precio = t.Precio,
                        imagen = t.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                        vendedor = t.Vendedor?.NombreComercial
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // Métodos helper privados
        private async Task<bool> PuedeEditarTorta(int vendedorId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int personaId))
                return false;

            // Si es Admin, puede editar cualquier torta
            if (User.IsInRole("Admin"))
                return true;

            // Si es Vendedor, solo puede editar sus propias tortas
            if (User.IsInRole("Vendedor"))
            {
                var vendedor = await _unitOfWork.VendedorRepository.GetByPersonaIdAsync(personaId);
                return vendedor != null && vendedor.Id == vendedorId;
            }

            return false;
        }

        private async Task CargarDatosParaFormulario()
        {
            var vendedores = await _unitOfWork.VendedorRepository.GetAllWithPersonaAsync();
            ViewBag.VendedorId = new SelectList(
                vendedores.Select(v => new
                {
                    Id = v.Id,
                    Texto = $"{v.NombreComercial} - {v.Persona?.Nombre}"
                }),
                "Id",
                "Texto"
            );

            ViewBag.Categorias = await ObtenerCategorias();
            ViewBag.Tamanios = ObtenerTamanios();
        }

        private async Task<List<string>> ObtenerCategorias()
        {
            var tortas = await _unitOfWork.TortaRepository.GetAllAsync();
            return tortas
                .Where(t => !string.IsNullOrEmpty(t.Categoria))
                .Select(t => t.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        private List<string> ObtenerTamanios()
        {
            return new List<string> { "Pequeña", "Mediana", "Grande", "Extra Grande" };
        }

        private async Task ProcesarImagenesTorta(int tortaId, List<IFormFile> imagenes, int? imagenPrincipalIndex)
        {
            for (int i = 0; i < imagenes.Count; i++)
            {
                var archivo = imagenes[i];
                if (archivo.Length > 0)
                {
                    var urlImagen = await _fileService.SaveFileAsync(archivo, "tortas");

                    var imagenTorta = new ImagenTorta
                    {
                        TortaId = tortaId,
                        UrlImagen = urlImagen,
                        NombreArchivo = archivo.FileName,
                        TamanioArchivo = archivo.Length,
                        TipoArchivo = archivo.ContentType,
                        EsPrincipal = imagenPrincipalIndex == i,
                        Orden = i,
                        FechaSubida = DateTime.Now
                    };

                    await _unitOfWork.ImagenesTorta.AddAsync(imagenTorta);
                }
            }
        }

        private async Task EliminarImagenesTorta(List<int> imagenesIds)
        {
            foreach (var imagenId in imagenesIds)
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(imagenId);
                if (imagen != null)
                {
                    await _fileService.DeleteFileAsync(imagen.UrlImagen);
                    _unitOfWork.ImagenesTorta.Delete(imagen);
                }
            }
        }

        private async Task CambiarImagenPrincipal(int tortaId, int nuevaImagenPrincipalId)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId);

            foreach (var imagen in imagenes)
            {
                imagen.EsPrincipal = imagen.Id == nuevaImagenPrincipalId;
                _unitOfWork.ImagenesTorta.Update(imagen);
            }
        }
    }
}