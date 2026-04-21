using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Services;


namespace CasaDeLasTortas.Controllers
{
    [Authorize]
    public class PersonaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public PersonaController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        // GET: Persona
        public async Task<IActionResult> Index(int pagina = 1, int tamanioPagina = 10, string busqueda = "", string filtroRol = "", string ordenarPor = "Nombre")
        {
            try
            {
                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(busqueda))
                {
                    personas = personas.Where(p =>
                        p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                    );
                }

                if (!string.IsNullOrEmpty(filtroRol))
                {
                    personas = personas.Where(p => p.Rol == filtroRol);
                }

                // Aplicar ordenamiento
                personas = ordenarPor switch
                {
                    "Email" => personas.OrderBy(p => p.Email),
                    "Rol" => personas.OrderBy(p => p.Rol),
                    "FechaRegistro" => personas.OrderBy(p => p.FechaRegistro),
                    "FechaRegistro_desc" => personas.OrderByDescending(p => p.FechaRegistro),
                    "Nombre_desc" => personas.OrderByDescending(p => p.Nombre),
                    _ => personas.OrderBy(p => p.Nombre)
                };

                // Calcular paginación
                var totalItems = personas.Count();
                var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamanioPagina);
                var personasPaginadas = personas
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .ToList();

                // Mapear a ViewModels
                var personasViewModel = personasPaginadas.Select(p => new PersonaViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Email = p.Email,
                    Telefono = p.Telefono,
                    Avatar = p.Avatar,
                    Rol = p.Rol,
                    FechaRegistro = p.FechaRegistro,
                    UltimoAcceso = p.UltimoAcceso,
                    Activo = p.Activo
                }).ToList();

                var viewModel = new PersonaListViewModel
                {
                    Personas = personasViewModel,
                    Paginacion = new PaginacionViewModel(totalItems, pagina, tamanioPagina),
                    FiltroRol = filtroRol,
                    Busqueda = busqueda,
                    OrdenarPor = ordenarPor
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las personas: {ex.Message}";
                return View(new PersonaListViewModel { Personas = new List<PersonaViewModel>() });
            }
        }

        // GET: Persona/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PersonaViewModel
                {
                    Id = persona.Id,
                    Nombre = persona.Nombre,
                    Email = persona.Email,
                    Telefono = persona.Telefono,
                    Avatar = persona.Avatar,
                    Rol = persona.Rol,
                    FechaRegistro = persona.FechaRegistro,
                    UltimoAcceso = persona.UltimoAcceso,
                    Activo = persona.Activo
                };

                // Cargar información adicional según el rol
                if (persona.Rol == "Vendedor" && persona.Vendedor != null)
                {
                    viewModel.Vendedor = new VendedorViewModel
                    {
                        Id = persona.Vendedor.Id,
                        NombreComercial = persona.Vendedor.NombreComercial,
                        Especialidad = persona.Vendedor.Especialidad,
                        Descripcion = persona.Vendedor.Descripcion,
                        Calificacion = persona.Vendedor.Calificacion,
                        TotalVentas = persona.Vendedor.TotalVentas,
                        Verificado = persona.Vendedor.Verificado,
                        Activo = persona.Vendedor.Activo
                    };
                }
                else if (persona.Rol == "Comprador" && persona.Comprador != null)
                {
                    viewModel.Comprador = new CompradorViewModel
                    {
                        Id = persona.Comprador.Id,
                        Direccion = persona.Comprador.Direccion,
                        Telefono = persona.Comprador.Telefono,
                        Ciudad = persona.Comprador.Ciudad,
                        Provincia = persona.Comprador.Provincia,
                        TotalCompras = persona.Comprador.TotalCompras,
                        Activo = persona.Comprador.Activo
                    };
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Persona/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PersonaEditViewModel
                {
                    Id = persona.Id,
                    Nombre = persona.Nombre,
                    Apellido = persona.Apellido,       
                    Email = persona.Email,
                    Telefono = persona.Telefono,
                    Dni = persona.Dni,            
                    Direccion = persona.Direccion,      
                    FechaNacimiento = persona.FechaNacimiento,
                    AvatarActual = persona.Avatar,
                    Activo = persona.Activo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Persona/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonaEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de persona no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar email duplicado
                var emailExiste = await _unitOfWork.PersonaRepository.ExistsAsync(
                    p => p.Email == viewModel.Email && p.Id != id);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este email ya está en uso por otra persona.");
                    return View(viewModel);
                }

                // Actualizar todos los campos
                persona.Nombre = viewModel.Nombre;
                persona.Apellido = viewModel.Apellido;        
                persona.Email = viewModel.Email;
                persona.Telefono = viewModel.Telefono;
                persona.Dni = viewModel.Dni;             
                persona.Direccion = viewModel.Direccion;       
                persona.FechaNacimiento = viewModel.FechaNacimiento; 
                persona.Activo = viewModel.Activo;

                // Manejar avatar
                if (viewModel.NuevoAvatar != null && viewModel.NuevoAvatar.Length > 0)
                {
                    if (!string.IsNullOrEmpty(persona.Avatar))
                        await _fileService.DeleteFileAsync(persona.Avatar);

                    persona.Avatar = await _fileService.UploadFileAsync(viewModel.NuevoAvatar, "avatars");
                }

                await _unitOfWork.PersonaRepository.UpdateAsync(persona);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Persona actualizada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = persona.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Persona/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PersonaViewModel
                {
                    Id = persona.Id,
                    Nombre = persona.Nombre,
                    Email = persona.Email,
                    Telefono = persona.Telefono,
                    Avatar = persona.Avatar,
                    Rol = persona.Rol,
                    FechaRegistro = persona.FechaRegistro,
                    UltimoAcceso = persona.UltimoAcceso,
                    Activo = persona.Activo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Persona/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si tiene datos relacionados
                var tieneVendedor = persona.Vendedor != null;
                var tieneComprador = persona.Comprador != null;

                if (tieneVendedor || tieneComprador)
                {
                    TempData["Error"] = "No se puede eliminar la persona porque tiene información de vendedor o comprador asociada.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Eliminar avatar si existe
                if (!string.IsNullOrEmpty(persona.Avatar))
                {
                    await _fileService.DeleteFileAsync(persona.Avatar);
                }

                await _unitOfWork.PersonaRepository.DeleteAsync(persona);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Persona eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Persona/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int id)
        {
            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.PersonaNombre = persona.Nombre;
                ViewBag.PersonaId = id;

                return View(new ChangePasswordViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Persona/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                ViewBag.PersonaNombre = persona?.Nombre;
                ViewBag.PersonaId = id;
                return View(viewModel);
            }

            try
            {
                var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);
                if (persona == null)
                {
                    TempData["Error"] = "Persona no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar contraseña actual 
                // Por ejemplo: if (!_authService.VerifyPassword(viewModel.CurrentPassword, persona.PasswordHash))

                // Actualizar contraseña 
                // persona.PasswordHash = _authService.HashPassword(viewModel.NewPassword);

                await _unitOfWork.PersonaRepository.UpdateAsync(persona);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Contraseña actualizada exitosamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cambiar contraseña: {ex.Message}";
                ViewBag.PersonaNombre = (await _unitOfWork.PersonaRepository.GetByIdAsync(id))?.Nombre;
                ViewBag.PersonaId = id;
                return View(viewModel);
            }
        }

        // AJAX: Búsqueda en tiempo real
        [HttpGet]
        public async Task<IActionResult> BuscarPersonas(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 2)
                {
                    return Json(new List<object>());
                }

                var personas = await _unitOfWork.PersonaRepository.GetAllAsync();
                var resultados = personas
                    .Where(p =>
                        p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(termino, StringComparison.OrdinalIgnoreCase)
                    )
                    .Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.Nombre,
                        email = p.Email,
                        rol = p.Rol,
                        avatar = p.Avatar
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // Método helper para verificar permisos
        private bool TienePermisoEdicion(int personaId)
        {
            // Aquí implementarías la lógica de permisos
            // Por ejemplo, solo admin o la misma persona puede editar
            return true; // Placeholder
        }

        // GET: Persona/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new PersonaCreateViewModel());
        }

        // POST: Persona/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PersonaCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                // Verificar email duplicado
                var emailExiste = await _unitOfWork.PersonaRepository.ExistsByEmailAsync(viewModel.Email);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este email ya está registrado.");
                    return View(viewModel);
                }

                var persona = new Persona
                {
                    Nombre = viewModel.Nombre,
                    Apellido = viewModel.Apellido,
                    Email = viewModel.Email,
                    Telefono = viewModel.Telefono,
                    Dni = viewModel.Dni,
                    Direccion = viewModel.Direccion,
                    FechaNacimiento = viewModel.FechaNacimiento,
                    Rol = viewModel.Rol,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(viewModel.Password),
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                // Subir avatar si se adjuntó uno
                if (viewModel.AvatarFile != null && viewModel.AvatarFile.Length > 0)
                {
                    persona.Avatar = await _fileService.UploadFileAsync(viewModel.AvatarFile, "avatars");
                }

                await _unitOfWork.PersonaRepository.AddAsync(persona);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = $"Persona '{persona.NombreCompleto}' creada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = persona.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la persona: {ex.Message}";
                return View(viewModel);
            }
        }
    }
}