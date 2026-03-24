using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Models.ViewModels;
using CasaDeLasTortas.Services;
using System.Security.Claims;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            IAuthService authService, 
            IJwtService jwtService,
            ILogger<AuthApiController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de prueba para verificar que el controlador funciona
        /// </summary>
        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "AuthApiController está funcionando correctamente",
                timestamp = DateTime.UtcNow,
                endpoints = new[] { "login", "register", "me", "refresh", "reset-password" }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  NUEVO: POST api/AuthApi/reset-password
        // ══════════════════════════════════════════════════════════
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { success = false, message = "Email y nueva contraseña son requeridos" });

            if (model.NewPassword.Length < 6)
                return BadRequest(new { success = false, message = "La contraseña debe tener al menos 6 caracteres" });

            try
            {
                var result = await _authService.ResetPasswordDirectAsync(model.Email, model.NewPassword);

                if (!result)
                    return BadRequest(new { success = false, message = "No se encontró una cuenta activa con ese email" });

                _logger.LogInformation("Contraseña reseteada para: {Email}", model.Email);
                return Ok(new { success = true, message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reset-password para: {Email}", model.Email);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Login y generación de token JWT
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            _logger.LogInformation("Intento de login para email: {Email}", model?.Email);

            if (model == null)
            {
                _logger.LogWarning("Login request con modelo nulo");
                return BadRequest(new { message = "Datos de login requeridos" });
            }

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "Email es requerido" });

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Contraseña es requerida" });

            try
            {
                var persona = await _authService.ValidateCredentialsAsync(model.Email, model.Password);
                
                if (persona == null)
                {
                    _logger.LogWarning("Credenciales inválidas para: {Email}", model.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                await _authService.UpdateLastAccessAsync(persona.Id);

                var token = _jwtService.GenerateToken(persona);
                _logger.LogInformation("Login exitoso para: {Email}", model.Email);

                return Ok(new
                {
                    success = true,
                    token,
                    user = new
                    {
                        id = persona.Id,
                        nombre = persona.Nombre,
                        email = persona.Email,
                        rol = persona.Rol,
                        telefono = persona.Telefono,
                        avatar = persona.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para: {Email}", model.Email);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor durante el login" 
                });
            }
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            _logger.LogInformation("Intento de registro para email: {Email}", model?.Email);

            if (model == null)
                return BadRequest(new { message = "Datos de registro requeridos" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                
                _logger.LogWarning("ModelState inválido para registro: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Datos inválidos", errors });
            }

            try
            {
                var emailExists = await _authService.IsEmailAvailableAsync(model.Email);
                if (!emailExists)
                    return BadRequest(new { message = "El email ya está registrado" });

                // Generar avatar con iniciales usando ui-avatars.com
                var avatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(model.Nombre)}&background=random&color=fff&size=150&bold=true&format=svg";

                var persona = new Persona
                {
                    Nombre = model.Nombre,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Rol = model.Rol ?? "Comprador",
                    Avatar = avatarUrl,
                    FechaRegistro = DateTime.Now,
                    UltimoAcceso = DateTime.Now,
                    Activo = true
                };

                var result = await _authService.RegisterAsync(persona, model.Password);
                var token = _jwtService.GenerateToken(result);

                _logger.LogInformation("Registro exitoso para: {Email}", model.Email);

                return Ok(new
                {
                    success = true,
                    token,
                    user = new
                    {
                        id = result.Id,
                        nombre = result.Nombre,
                        email = result.Email,
                        rol = result.Rol,
                        telefono = result.Telefono,
                        avatar = result.Avatar
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de operación en registro: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario: {Message}", ex.Message);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor durante el registro" 
                });
            }
        }

        /// <summary>
        /// Obtener información del usuario autenticado
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("PersonaId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Token inválido - ID de usuario no encontrado" });

            try
            {
                var persona = await _authService.GetUserByIdAsync(userId);

                if (persona == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                object? rolData = null;
                if (persona.Rol == "Vendedor" && persona.Vendedor != null)
                {
                    rolData = new 
                    { 
                        tipo = "Vendedor",
                        vendedorId = persona.Vendedor.Id,
                        nombreComercial = persona.Vendedor.NombreComercial,
                        especialidad = persona.Vendedor.Especialidad,
                        verificado = persona.Vendedor.Verificado
                    };
                }
                else if (persona.Rol == "Comprador" && persona.Comprador != null)
                {
                    rolData = new 
                    { 
                        tipo = "Comprador",
                        compradorId = persona.Comprador.Id,
                        direccion = persona.Comprador.Direccion,
                        ciudad = persona.Comprador.Ciudad,
                        totalCompras = persona.Comprador.TotalCompras
                    };
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = persona.Id,
                        nombre = persona.Nombre,
                        email = persona.Email,
                        rol = persona.Rol,
                        telefono = persona.Telefono,
                        avatar = persona.Avatar,
                        fechaRegistro = persona.FechaRegistro,
                        ultimoAcceso = persona.UltimoAcceso,
                        activo = persona.Activo,
                        rolData = rolData
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario: {Message}", ex.Message);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor" 
                });
            }
        }

        /// <summary>
        /// Refrescar token JWT
        /// </summary>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("PersonaId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Token inválido" });

            try
            {
                var persona = await _authService.GetUserByIdAsync(userId);

                if (persona == null)
                    return Unauthorized(new { message = "Usuario no encontrado" });

                var newToken = _jwtService.GenerateToken(persona);

                return Ok(new 
                { 
                    success = true,
                    token = newToken,
                    expiresIn = 86400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar token: {Message}", ex.Message);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor" 
                });
            }
        }

        /// <summary>
        /// Verificar si un email está disponible
        /// </summary>
        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email requerido" });

            try
            {
                var isAvailable = await _authService.IsEmailAvailableAsync(email);

                return Ok(new 
                { 
                    success = true,
                    available = isAvailable,
                    message = isAvailable ? "Email disponible" : "Email ya registrado"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar email: {Message}", ex.Message);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor" 
                });
            }
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("PersonaId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Token inválido" });

            try
            {
                await _authService.UpdateLastAccessAsync(userId);

                return Ok(new { 
                    success = true,
                    message = "Sesión cerrada exitosamente" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante logout: {Message}", ex.Message);
                return StatusCode(500, new { 
                    success = false,
                    message = "Error interno del servidor" 
                });
            }
        }
    }

    // ── Modelo para el request ──
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}