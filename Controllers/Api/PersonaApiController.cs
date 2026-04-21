using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class PersonaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersonaApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtener todas las personas con paginación
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var personas = await _unitOfWork.PersonaRepository.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.PersonaRepository.CountAsync();

            return Ok(new
            {
                data = personas,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener persona por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);

            if (persona == null)
                return NotFound(new { message = "Persona no encontrada" });

            var personaDTO = new PersonaDTO
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Email = persona.Email,
                Telefono = persona.Telefono,
                Direccion = persona.Direccion,
                FechaNacimiento = persona.FechaNacimiento,
                Dni = persona.Dni,
                Avatar = persona.Avatar,
                Rol = persona.Rol,
                FechaRegistro = persona.FechaRegistro,
                UltimoAcceso = persona.UltimoAcceso,
                Activo = persona.Activo
            };

            return Ok(personaDTO);
        }

        /// <summary>
        /// Crear nueva persona
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // Permitir registro sin autenticación
        public async Task<IActionResult> Create([FromBody] PersonaCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el email ya existe
            if (await _unitOfWork.PersonaRepository.ExistsByEmailAsync(dto.Email))
            {
                return BadRequest(new { message = "El email ya está registrado" });
            }

            var persona = new Persona
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                FechaNacimiento = dto.FechaNacimiento,
                Dni = dto.Dni,
                Rol = dto.Rol,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            await _unitOfWork.PersonaRepository.AddAsync(persona);
            await _unitOfWork.SaveChangesAsync();

            var personaResponse = new PersonaResponseDTO
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Email = persona.Email,
                Rol = persona.Rol,
                Telefono = persona.Telefono,
                Direccion = persona.Direccion,
                FechaRegistro = persona.FechaRegistro
            };

            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, personaResponse);
        }

        /// <summary>
        /// Actualizar persona existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PersonaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);

            if (persona == null)
                return NotFound(new { message = "Persona no encontrada" });

            // Verificar permisos (solo admin o el propio usuario)
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var esAdmin = User.IsInRole("Admin");
            var esPropioUsuario = int.TryParse(idClaim, out var cId) && persona.Id == cId;

            if (!esAdmin && !esPropioUsuario)
            {
                return Forbid();
            }

            persona.Nombre = dto.Nombre;
            persona.Apellido = dto.Apellido;
            persona.Email = dto.Email;
            persona.Telefono = dto.Telefono;
            persona.Direccion = dto.Direccion;
            persona.FechaNacimiento = dto.FechaNacimiento;
            persona.Dni = dto.Dni;
            
            if (esAdmin)
            {
                persona.Activo = dto.Activo;
            }

            _unitOfWork.PersonaRepository.Update(persona);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Persona actualizada correctamente" });
        }

        /// <summary>
        /// Eliminar persona
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);

            if (persona == null)
                return NotFound(new { message = "Persona no encontrada" });

            _unitOfWork.PersonaRepository.Delete(persona);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Cambiar contraseña
        /// </summary>
        [HttpPut("{id}/cambiar-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(id);

            if (persona == null)
                return NotFound(new { message = "Persona no encontrada" });

            // Verificar permisos (solo admin o el propio usuario)
            var idClaim2 = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var esAdmin = User.IsInRole("Admin");
            var esPropioUsuario = int.TryParse(idClaim2, out var cId2) && persona.Id == cId2;

            if (!esAdmin && !esPropioUsuario)
            {
                return Forbid();
            }

            // Verificar contraseña actual (solo para el propio usuario)
            if (!esAdmin && !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, persona.PasswordHash))
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }

            persona.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _unitOfWork.PersonaRepository.Update(persona);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada correctamente" });
        }

        /// <summary>
        /// Buscar personas por término
        /// </summary>
        [HttpGet("search")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> Search([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return BadRequest(new { message = "Término de búsqueda requerido" });

            var personas = await _unitOfWork.PersonaRepository.SearchAsync(termino);
            return Ok(personas);
        }

        /// <summary>
        /// Obtener perfil del usuario actual
        /// </summary>
        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("PersonaId")?.Value;
            if (!int.TryParse(claim, out var personaId))
                return Unauthorized();

            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(personaId);
            if (persona == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var perfil = new PersonaPerfilDTO
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Email = persona.Email,
                Telefono = persona.Telefono,
                Direccion = persona.Direccion,
                FechaNacimiento = persona.FechaNacimiento,
                Dni = persona.Dni,
                Avatar = persona.Avatar,
                Rol = persona.Rol,
                FechaRegistro = persona.FechaRegistro,
                UltimoAcceso = persona.UltimoAcceso
            };

            return Ok(perfil);
        }
    }
}