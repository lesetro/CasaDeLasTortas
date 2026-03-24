using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CasaDeLasTortas.Controllers
{
    [Authorize]
    public class ImagenTortaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public ImagenTortaController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        // GET: ImagenTorta
        public async Task<IActionResult> Index(int? tortaId = null, int pagina = 1, int tamanioPagina = 20)
        {
            try
            {
                IEnumerable<ImagenTorta> imagenes;
                string nombreTorta = "";

                if (tortaId.HasValue)
                {
                    // Mostrar imágenes de una torta específica
                    imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId.Value); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(tortaId.Value); // CORREGIDO: TortaRepository → Tortas
                    nombreTorta = torta?.Nombre ?? "Torta no encontrada";
                }
                else
                {
                    // Mostrar todas las imágenes
                    imagenes = await _unitOfWork.ImagenesTorta.GetAllWithTortaAsync(); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                }

                // Ordenar por torta y luego por orden
                imagenes = imagenes.OrderBy(i => i.TortaId).ThenBy(i => i.Orden).ThenByDescending(i => i.EsPrincipal);

                // Calcular paginación
                var totalItems = imagenes.Count();
                var imagenesPaginadas = imagenes
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Mapear a ViewModels
                var imagenesViewModel = imagenesPaginadas.Select(i => new ImagenTortaViewModel
                {
                    Id = i.Id,
                    TortaId = i.TortaId,
                    UrlImagen = i.UrlImagen,
                    NombreArchivo = i.NombreArchivo,
                    TamanioArchivo = i.TamanioArchivo,
                    TipoArchivo = i.TipoArchivo,
                    EsPrincipal = i.EsPrincipal,
                    Orden = i.Orden,
                    FechaSubida = i.FechaSubida,
                    NombreTorta = i.Torta?.Nombre
                }).ToList();

                var viewModel = new ImagenTortaListViewModel
                {
                    TortaId = tortaId ?? 0,
                    NombreTorta = nombreTorta,
                    Imagenes = imagenesViewModel,
                    PuedeEditar = await PuedeEditarImagenes(tortaId)
                };

                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina);
                ViewBag.TortaId = tortaId;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las imágenes: {ex.Message}";
                return View(new ImagenTortaListViewModel { Imagenes = new List<ImagenTortaViewModel>() });
            }
        }

        // GET: ImagenTorta/Create
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Create(int? tortaId = null)
        {
            try
            {
                var viewModel = new ImagenTortaCreateViewModel();
                
                if (tortaId.HasValue)
                {
                    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(tortaId.Value); // CORREGIDO: TortaRepository → Tortas
                    if (torta != null && await PuedeEditarTorta(torta.VendedorId))
                    {
                        viewModel.TortaId = torta.Id;
                    }
                    else
                    {
                        TempData["Error"] = "No tiene permisos para agregar imágenes a esta torta.";
                        return RedirectToAction("Index", "Torta");
                    }
                }

                await CargarTortasParaFormulario();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction("Index", "Torta");
            }
        }

        // POST: ImagenTorta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Create(ImagenTortaCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await CargarTortasParaFormulario();
                return View(viewModel);
            }

            try
            {
                // Verificar que la torta existe y el usuario tiene permisos
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(viewModel.TortaId); // CORREGIDO: TortaRepository → Tortas
                if (torta == null)
                {
                    ModelState.AddModelError("TortaId", "Torta no encontrada.");
                    await CargarTortasParaFormulario();
                    return View(viewModel);
                }

                if (!await PuedeEditarTorta(torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para agregar imágenes a esta torta.";
                    return RedirectToAction("Details", "Torta", new { id = torta.Id });
                }

                if (viewModel.Imagenes == null || !viewModel.Imagenes.Any())
                {
                    ModelState.AddModelError("Imagenes", "Debe seleccionar al menos una imagen.");
                    await CargarTortasParaFormulario();
                    return View(viewModel);
                }

                // Obtener el orden inicial (último orden + 1)
                var imagenesExistentes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(viewModel.TortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                var siguienteOrden = imagenesExistentes.Any() ? imagenesExistentes.Max(i => i.Orden) + 1 : 0;

                var imagenesCreadas = new List<ImagenTorta>();

                // Procesar cada imagen
                for (int i = 0; i < viewModel.Imagenes.Count; i++)
                {
                    var archivo = viewModel.Imagenes[i];
                    
                    if (archivo.Length > 0)
                    {
                        // Validar tipo de archivo
                        if (!EsTipoImagenValido(archivo.ContentType))
                        {
                            ModelState.AddModelError("Imagenes", $"El archivo {archivo.FileName} no es un tipo de imagen válido.");
                            continue;
                        }

                        // Validar tamaño
                        if (archivo.Length > 5 * 1024 * 1024) // 5MB
                        {
                            ModelState.AddModelError("Imagenes", $"El archivo {archivo.FileName} excede el tamaño máximo de 5MB.");
                            continue;
                        }

                        // Subir archivo - CORREGIDO: UploadFileAsync → SaveFileAsync
                        var urlImagen = await _fileService.SaveFileAsync(archivo, "tortas");
                        
                        var imagenTorta = new ImagenTorta
                        {
                            TortaId = viewModel.TortaId,
                            UrlImagen = urlImagen,
                            NombreArchivo = archivo.FileName,
                            TamanioArchivo = archivo.Length,
                            TipoArchivo = archivo.ContentType,
                            EsPrincipal = viewModel.ImagenPrincipalIndex == i,
                            Orden = siguienteOrden + i,
                            FechaSubida = DateTime.Now
                        };

                        await _unitOfWork.ImagenesTorta.AddAsync(imagenTorta); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                        imagenesCreadas.Add(imagenTorta);
                    }
                }

                // Si no hay imagen principal y se crearon imágenes, hacer la primera como principal
                if (imagenesCreadas.Any() && !imagenesCreadas.Any(i => i.EsPrincipal))
                {
                    var primerImagen = imagenesCreadas.First();
                    primerImagen.EsPrincipal = await EsPrimeraImagenDeLaTorta(viewModel.TortaId);
                    _unitOfWork.ImagenesTorta.Update(primerImagen); // CORREGIDO: UpdateAsync → Update
                }

                await _unitOfWork.SaveAsync();

                TempData["Success"] = $"{imagenesCreadas.Count} imagen(es) agregada(s) exitosamente.";
                return RedirectToAction(nameof(Index), new { tortaId = viewModel.TortaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear las imágenes: {ex.Message}";
                await CargarTortasParaFormulario();
                return View(viewModel);
            }
        }

        // GET: ImagenTorta/Edit/5
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagen == null)
                {
                    TempData["Error"] = "Imagen no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarTorta(imagen.Torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para editar esta imagen.";
                    return RedirectToAction(nameof(Index), new { tortaId = imagen.TortaId });
                }

                var viewModel = new ImagenTortaEditViewModel
                {
                    Id = imagen.Id,
                    EsPrincipal = imagen.EsPrincipal,
                    Orden = imagen.Orden
                };

                ViewBag.NombreTorta = imagen.Torta.Nombre;
                ViewBag.UrlImagen = imagen.UrlImagen;
                ViewBag.TortaId = imagen.TortaId;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ImagenTorta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Edit(int id, ImagenTortaEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de imagen no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var imagenTemp = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagenTemp != null)
                {
                    ViewBag.NombreTorta = imagenTemp.Torta.Nombre;
                    ViewBag.UrlImagen = imagenTemp.UrlImagen;
                    ViewBag.TortaId = imagenTemp.TortaId;
                }
                return View(viewModel);
            }

            try
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagen == null)
                {
                    TempData["Error"] = "Imagen no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarTorta(imagen.Torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para editar esta imagen.";
                    return RedirectToAction(nameof(Index), new { tortaId = imagen.TortaId });
                }

                // Si se marca como principal, desmarcar otras imágenes principales de la misma torta
                if (viewModel.EsPrincipal && !imagen.EsPrincipal)
                {
                    await DesmarcarImagenesPrincipales(imagen.TortaId);
                }

                imagen.EsPrincipal = viewModel.EsPrincipal;
                imagen.Orden = viewModel.Orden;

                _unitOfWork.ImagenesTorta.Update(imagen); // CORREGIDO: UpdateAsync → Update
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Imagen actualizada exitosamente.";
                return RedirectToAction(nameof(Index), new { tortaId = imagen.TortaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                var imagenTemp = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagenTemp != null)
                {
                    ViewBag.NombreTorta = imagenTemp.Torta.Nombre;
                    ViewBag.UrlImagen = imagenTemp.UrlImagen;
                    ViewBag.TortaId = imagenTemp.TortaId;
                }
                return View(viewModel);
            }
        }

        // GET: ImagenTorta/Delete/5
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagen == null)
                {
                    TempData["Error"] = "Imagen no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarTorta(imagen.Torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para eliminar esta imagen.";
                    return RedirectToAction(nameof(Index), new { tortaId = imagen.TortaId });
                }

                var viewModel = new ImagenTortaViewModel
                {
                    Id = imagen.Id,
                    TortaId = imagen.TortaId,
                    UrlImagen = imagen.UrlImagen,
                    NombreArchivo = imagen.NombreArchivo,
                    TamanioArchivo = imagen.TamanioArchivo,
                    EsPrincipal = imagen.EsPrincipal,
                    Orden = imagen.Orden,
                    FechaSubida = imagen.FechaSubida,
                    NombreTorta = imagen.Torta.Nombre
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ImagenTorta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagen == null)
                {
                    TempData["Error"] = "Imagen no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await PuedeEditarTorta(imagen.Torta.VendedorId))
                {
                    TempData["Error"] = "No tiene permisos para eliminar esta imagen.";
                    return RedirectToAction(nameof(Index), new { tortaId = imagen.TortaId });
                }

                var tortaId = imagen.TortaId;
                var eraPrincipal = imagen.EsPrincipal;

                // Eliminar archivo físico
                await _fileService.DeleteFileAsync(imagen.UrlImagen);

                // Eliminar registro de la base de datos
                _unitOfWork.ImagenesTorta.Delete(imagen); // CORREGIDO: DeleteAsync → Delete

                // Si era la imagen principal, asignar otra como principal
                if (eraPrincipal)
                {
                    await AsignarNuevaImagenPrincipal(tortaId);
                }

                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Imagen eliminada exitosamente.";
                return RedirectToAction(nameof(Index), new { tortaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // POST: ImagenTorta/SetPrincipal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> SetPrincipal(int id)
        {
            try
            {
                var imagen = await _unitOfWork.ImagenesTorta.GetByIdWithTortaAsync(id); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                if (imagen == null)
                {
                    return Json(new { success = false, message = "Imagen no encontrada." });
                }

                if (!await PuedeEditarTorta(imagen.Torta.VendedorId))
                {
                    return Json(new { success = false, message = "No tiene permisos para editar esta imagen." });
                }

                // Desmarcar otras imágenes principales
                await DesmarcarImagenesPrincipales(imagen.TortaId);

                // Marcar esta como principal
                imagen.EsPrincipal = true;
                _unitOfWork.ImagenesTorta.Update(imagen); // CORREGIDO: UpdateAsync → Update
                await _unitOfWork.SaveAsync();

                return Json(new { success = true, message = "Imagen principal actualizada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: ImagenTorta/UpdateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> imagenesIds)
        {
            try
            {
                if (imagenesIds == null || !imagenesIds.Any())
                {
                    return Json(new { success = false, message = "No se proporcionaron IDs de imágenes." });
                }

                for (int i = 0; i < imagenesIds.Count; i++)
                {
                    var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(imagenesIds[i]); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                    if (imagen != null)
                    {
                        // Verificar permisos
                        var torta = await _unitOfWork.TortaRepository.GetByIdAsync(imagen.TortaId); // CORREGIDO: TortaRepository → Tortas
                        if (torta != null && await PuedeEditarTorta(torta.VendedorId))
                        {
                            imagen.Orden = i;
                            _unitOfWork.ImagenesTorta.Update(imagen); // CORREGIDO: UpdateAsync → Update
                        }
                    }
                }

                await _unitOfWork.SaveAsync();
                return Json(new { success = true, message = "Orden actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: ImagenTorta/Gallery/5
        public async Task<IActionResult> Gallery(int tortaId)
        {
            try
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithDetallesCompletosAsync(tortaId); // CORREGIDO: GetByIdWithDetailsAsync → GetByIdWithDetallesCompletosAsync
                if (torta == null)
                {
                    TempData["Error"] = "Torta no encontrada.";
                    return RedirectToAction("Index", "Torta");
                }

                var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                var imagenesOrdenadas = imagenes.OrderBy(i => i.Orden).ToList();

                var viewModel = new ImagenTortaListViewModel
                {
                    TortaId = tortaId,
                    NombreTorta = torta.Nombre,
                    Imagenes = imagenesOrdenadas.Select(i => new ImagenTortaViewModel
                    {
                        Id = i.Id,
                        UrlImagen = i.UrlImagen,
                        NombreArchivo = i.NombreArchivo,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList(),
                    PuedeEditar = await PuedeEditarTorta(torta.VendedorId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la galería: {ex.Message}";
                return RedirectToAction("Index", "Torta");
            }
        }

        // AJAX: Obtener imágenes de una torta
        [HttpGet]
        public async Task<IActionResult> GetImagenesByTorta(int tortaId)
        {
            try
            {
                var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
                var resultado = imagenes.OrderBy(i => i.Orden).Select(i => new
                {
                    id = i.Id,
                    url = i.UrlImagen,
                    esPrincipal = i.EsPrincipal,
                    orden = i.Orden,
                    nombreArchivo = i.NombreArchivo
                }).ToList();

                return Json(resultado);
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

        private async Task<bool> PuedeEditarImagenes(int? tortaId)
        {
            if (!tortaId.HasValue) return false;

            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(tortaId.Value); 
            return torta != null && await PuedeEditarTorta(torta.VendedorId);
        }

        private async Task CargarTortasParaFormulario()
        {
            var tortas = await _unitOfWork.TortaRepository.GetAllWithVendedorAsync(); 
            
            // Filtrar solo las tortas que el usuario puede editar
            var tortasPermitidas = new List<Torta>();
            foreach (var torta in tortas)
            {
                if (await PuedeEditarTorta(torta.VendedorId))
                {
                    tortasPermitidas.Add(torta);
                }
            }

            ViewBag.TortaId = new SelectList(
                tortasPermitidas.Select(t => new { 
                    Id = t.Id, 
                    Texto = $"{t.Nombre} - {t.Vendedor?.NombreComercial ?? "Sin vendedor"}" 
                }), 
                "Id", 
                "Texto"
            );
        }

        private bool EsTipoImagenValido(string contentType)
        {
            var tiposPermitidos = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            return tiposPermitidos.Contains(contentType.ToLower());
        }

        private async Task<bool> EsPrimeraImagenDeLaTorta(int tortaId)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
            return !imagenes.Any();
        }

        private async Task DesmarcarImagenesPrincipales(int tortaId)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
            foreach (var imagen in imagenes.Where(i => i.EsPrincipal))
            {
                imagen.EsPrincipal = false;
                _unitOfWork.ImagenesTorta.Update(imagen); // CORREGIDO: UpdateAsync → Update
            }
        }

        private async Task AsignarNuevaImagenPrincipal(int tortaId)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaIdAsync(tortaId); // CORREGIDO: ImagenTortaRepository → ImagenesTorta
            var primeraImagen = imagenes.OrderBy(i => i.Orden).FirstOrDefault();
            
            if (primeraImagen != null)
            {
                primeraImagen.EsPrincipal = true;
                _unitOfWork.ImagenesTorta.Update(primeraImagen); // CORREGIDO: UpdateAsync → Update
            }
        }
    }
}