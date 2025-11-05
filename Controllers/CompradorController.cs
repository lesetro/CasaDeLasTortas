using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CasaDeLasTortas.Controllers
{
    [Authorize]
    public class CompradorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompradorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Comprador
        public async Task<IActionResult> Index(int pagina = 1, int tamanioPagina = 10, string busqueda = "", string filtroCiudad = "", string ordenarPor = "NombrePersona")
        {
            try
            {
                var compradores = await _unitOfWork.CompradorRepository.GetAllWithPersonaAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(busqueda))
                {
                    compradores = compradores.Where(c => 
                        c.Persona.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        c.Persona.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        c.Direccion.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        (c.Ciudad != null && c.Ciudad.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                    );
                }

                if (!string.IsNullOrEmpty(filtroCiudad))
                {
                    compradores = compradores.Where(c => c.Ciudad != null && c.Ciudad.Contains(filtroCiudad, StringComparison.OrdinalIgnoreCase));
                }

                // Aplicar ordenamiento
                compradores = ordenarPor switch
                {
                    "Email" => compradores.OrderBy(c => c.Persona.Email),
                    "Ciudad" => compradores.OrderBy(c => c.Ciudad),
                    "TotalCompras" => compradores.OrderByDescending(c => c.TotalCompras),
                    "FechaCreacion" => compradores.OrderBy(c => c.FechaCreacion),
                    "FechaCreacion_desc" => compradores.OrderByDescending(c => c.FechaCreacion),
                    "NombrePersona_desc" => compradores.OrderByDescending(c => c.Persona.Nombre),
                    _ => compradores.OrderBy(c => c.Persona.Nombre)
                };

                // Calcular paginación
                var totalItems = compradores.Count();
                var compradoresPaginados = compradores
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Mapear a ViewModels
                var compradoresViewModel = compradoresPaginados.Select(c => new CompradorViewModel
                {
                    Id = c.Id,
                    PersonaId = c.PersonaId,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Ciudad = c.Ciudad,
                    Provincia = c.Provincia,
                    CodigoPostal = c.CodigoPostal,
                    TotalCompras = c.TotalCompras,
                    FechaNacimiento = c.FechaNacimiento,
                    Preferencias = c.Preferencias,
                    FechaCreacion = c.FechaCreacion,
                    Activo = c.Activo,
                    NombrePersona = c.Persona.Nombre,
                    Email = c.Persona.Email,
                    Avatar = c.Persona.Avatar
                }).ToList();

                // Calcular estadísticas adicionales para cada comprador
                foreach (var comprador in compradoresViewModel)
                {
                    var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdAsync(comprador.Id);
                    comprador.TotalGastado = pagos.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto);
                    comprador.PedidosActivos = pagos.Count(p => p.Estado == EstadoPago.Pendiente);
                    comprador.UltimaCompra = pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault()?.FechaPago;
                }

                var viewModel = new CompradorListViewModel
                {
                    Compradores = compradoresViewModel,
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                    FiltroCiudad = filtroCiudad,
                    Busqueda = busqueda,
                    OrdenarPor = ordenarPor
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los compradores: {ex.Message}";
                return View(new CompradorListViewModel { Compradores = new List<CompradorViewModel>() });
            }
        }

        // GET: Comprador/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CompradorViewModel
                {
                    Id = comprador.Id,
                    PersonaId = comprador.PersonaId,
                    Direccion = comprador.Direccion,
                    Telefono = comprador.Telefono,
                    Ciudad = comprador.Ciudad,
                    Provincia = comprador.Provincia,
                    CodigoPostal = comprador.CodigoPostal,
                    TotalCompras = comprador.TotalCompras,
                    FechaNacimiento = comprador.FechaNacimiento,
                    Preferencias = comprador.Preferencias,
                    FechaCreacion = comprador.FechaCreacion,
                    Activo = comprador.Activo,
                    NombrePersona = comprador.Persona.Nombre,
                    Email = comprador.Persona.Email,
                    Avatar = comprador.Persona.Avatar
                };

                // Calcular estadísticas
                var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdAsync(id);
                viewModel.TotalGastado = pagos.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto);
                viewModel.PedidosActivos = pagos.Count(p => p.Estado == EstadoPago.Pendiente);
                viewModel.UltimaCompra = pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault()?.FechaPago;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Comprador/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Obtener personas que no tienen perfil de comprador
                var personasSinComprador = await _unitOfWork.PersonaRepository.GetPersonasSinCompradorAsync();
                
                ViewBag.PersonaId = new SelectList(
                    personasSinComprador.Select(p => new { 
                        Id = p.Id, 
                        Texto = $"{p.Nombre} - {p.Email}" 
                    }), 
                    "Id", 
                    "Texto"
                );

                return View(new CompradorCreateViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Comprador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompradorCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await CargarPersonasParaCrear();
                return View(viewModel);
            }

            try
            {
                // Verificar que la persona existe y no tiene comprador
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(viewModel.PersonaId);
                if (persona == null)
                {
                    ModelState.AddModelError("PersonaId", "Persona no encontrada.");
                    await CargarPersonasParaCrear();
                    return View(viewModel);
                }

                if (persona.Comprador != null)
                {
                    ModelState.AddModelError("PersonaId", "Esta persona ya tiene un perfil de comprador.");
                    await CargarPersonasParaCrear();
                    return View(viewModel);
                }

                var comprador = new Comprador
                {
                    PersonaId = viewModel.PersonaId,
                    Direccion = viewModel.Direccion,
                    Telefono = viewModel.Telefono,
                    Ciudad = viewModel.Ciudad,
                    Provincia = viewModel.Provincia,
                    CodigoPostal = viewModel.CodigoPostal,
                    FechaNacimiento = viewModel.FechaNacimiento,
                    Preferencias = viewModel.Preferencias,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                await _unitOfWork.CompradorRepository.AddAsync(comprador);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Perfil de comprador creado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = comprador.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el comprador: {ex.Message}";
                await CargarPersonasParaCrear();
                return View(viewModel);
            }
        }

        // GET: Comprador/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CompradorEditViewModel
                {
                    Id = comprador.Id,
                    Direccion = comprador.Direccion,
                    Telefono = comprador.Telefono,
                    Ciudad = comprador.Ciudad,
                    Provincia = comprador.Provincia,
                    CodigoPostal = comprador.CodigoPostal,
                    FechaNacimiento = comprador.FechaNacimiento,
                    Preferencias = comprador.Preferencias,
                    Activo = comprador.Activo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Comprador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompradorEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de comprador no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                comprador.Direccion = viewModel.Direccion;
                comprador.Telefono = viewModel.Telefono;
                comprador.Ciudad = viewModel.Ciudad;
                comprador.Provincia = viewModel.Provincia;
                comprador.CodigoPostal = viewModel.CodigoPostal;
                comprador.FechaNacimiento = viewModel.FechaNacimiento;
                comprador.Preferencias = viewModel.Preferencias;
                comprador.Activo = viewModel.Activo;

                await _unitOfWork.CompradorRepository.UpdateAsync(comprador);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Comprador actualizado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = comprador.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Comprador/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CompradorViewModel
                {
                    Id = comprador.Id,
                    Direccion = comprador.Direccion,
                    Telefono = comprador.Telefono,
                    Ciudad = comprador.Ciudad,
                    TotalCompras = comprador.TotalCompras,
                    NombrePersona = comprador.Persona.Nombre,
                    Email = comprador.Persona.Email
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Comprador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si tiene pagos asociados
                var tienePagos = await _unitOfWork.PagoRepository.ExistsAsync(p => p.CompradorId == id);
                if (tienePagos)
                {
                    TempData["Error"] = "No se puede eliminar el comprador porque tiene pagos asociados.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                await _unitOfWork.CompradorRepository.DeleteAsync(comprador);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Comprador eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Comprador/MisCompras/5
        public async Task<IActionResult> MisCompras(int id, int pagina = 1, int tamanioPagina = 10, string filtroEstado = "")
        {
            try
            {
                var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdWithDetailsAsync(id);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(filtroEstado) && Enum.TryParse<EstadoPago>(filtroEstado, out var estado))
                {
                    pagos = pagos.Where(p => p.Estado == estado);
                }

                // Ordenar por fecha más reciente
                pagos = pagos.OrderByDescending(p => p.FechaPago);

                // Calcular paginación
                var totalItems = pagos.Count();
                var pagosPaginados = pagos
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Calcular estadísticas del mes actual
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var pagosMes = pagos.Where(p => p.FechaPago >= inicioMes);

                var viewModel = new CompradorPerfilViewModel
                {
                    Comprador = new CompradorViewModel
                    {
                        Id = comprador.Id,
                        NombrePersona = comprador.Persona.Nombre,
                        Email = comprador.Persona.Email,
                        Avatar = comprador.Persona.Avatar,
                        Direccion = comprador.Direccion,
                        Ciudad = comprador.Ciudad,
                        TotalCompras = comprador.TotalCompras
                    },
                    HistorialCompras = pagosPaginados.Select(p => new PagoViewModel
                    {
                        Id = p.Id,
                        Monto = p.Monto,
                        Cantidad = p.Cantidad,
                        FechaPago = p.FechaPago,
                        Estado = p.Estado,
                        FechaEntrega = p.FechaEntrega,
                        NombreTorta = p.Torta?.Nombre ?? "N/A",
                        NombreVendedor = p.Vendedor?.NombreComercial ?? "N/A",
                        ImagenTorta = p.Torta?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    }).ToList(),
                    PedidosActivos = pagos
                        .Where(p => p.Estado == EstadoPago.Pendiente)
                        .Select(p => new PagoViewModel
                        {
                            Id = p.Id,
                            Monto = p.Monto,
                            Cantidad = p.Cantidad,
                            FechaPago = p.FechaPago,
                            Estado = p.Estado,
                            FechaEntrega = p.FechaEntrega,
                            NombreTorta = p.Torta?.Nombre ?? "N/A",
                            NombreVendedor = p.Vendedor?.NombreComercial ?? "N/A"
                        }).ToList(),
                    TotalGastadoMes = pagosMes.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto),
                    ComprasMes = pagosMes.Count(p => p.Estado == EstadoPago.Completado)
                };

                // Compras por categoría
                var tortasCompradas = pagos.Where(p => p.Estado == EstadoPago.Completado && p.Torta != null);
                viewModel.ComprasPorCategoria = tortasCompradas
                    .GroupBy(p => p.Torta.Categoria ?? "Sin categoría")
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Cantidad));

                // Vendedores favoritos (con más compras)
                var vendedoresFavoritos = tortasCompradas
                    .GroupBy(p => p.Vendedor)
                    .OrderByDescending(g => g.Sum(p => p.Cantidad))
                    .Take(5)
                    .Select(g => new VendedorViewModel
                    {
                        Id = g.Key.Id,
                        NombreComercial = g.Key.NombreComercial,
                        Especialidad = g.Key.Especialidad,
                        Calificacion = g.Key.Calificacion,
                        Avatar = g.Key.Persona?.Avatar
                    }).ToList();

                viewModel.VendedoresFavoritos = vendedoresFavoritos;

                ViewBag.Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina);
                ViewBag.FiltroEstado = filtroEstado;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las compras: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Búsqueda de compradores
        [HttpGet]
        public async Task<IActionResult> BuscarCompradores(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 2)
                {
                    return Json(new List<object>());
                }

                var compradores = await _unitOfWork.CompradorRepository.GetAllWithPersonaAsync();
                var resultados = compradores
                    .Where(c => 
                        c.Persona.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                        c.Persona.Email.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                        (c.Ciudad != null && c.Ciudad.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    )
                    .Take(10)
                    .Select(c => new
                    {
                        id = c.Id,
                        nombre = c.Persona.Nombre,
                        email = c.Persona.Email,
                        ciudad = c.Ciudad,
                        totalCompras = c.TotalCompras,
                        avatar = c.Persona.Avatar
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // AJAX: Obtener estadísticas del comprador
        [HttpGet]
        public async Task<IActionResult> EstadisticasComprador(int id)
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetByCompradorIdAsync(id);
                var totalGastado = pagos.Where(p => p.Estado == EstadoPago.Completado).Sum(p => p.Monto);
                var pedidosActivos = pagos.Count(p => p.Estado == EstadoPago.Pendiente);
                var ultimaCompra = pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault()?.FechaPago;

                return Json(new
                {
                    totalGastado,
                    pedidosActivos,
                    ultimaCompra,
                    totalPedidos = pagos.Count()
                });
            }
            catch (Exception)
            {
                return Json(new { error = "Error al cargar estadísticas" });
            }
        }

        // Método helper para cargar personas sin comprador
        private async Task CargarPersonasParaCrear()
        {
            var personasSinComprador = await _unitOfWork.PersonaRepository.GetPersonasSinCompradorAsync();
            ViewBag.PersonaId = new SelectList(
                personasSinComprador.Select(p => new { 
                    Id = p.Id, 
                    Texto = $"{p.Nombre} - {p.Email}" 
                }), 
                "Id", 
                "Texto"
            );
        }
    }
}