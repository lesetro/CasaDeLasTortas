using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CasaDeLasTortas.Services;
using CasaDeLasTortas.Models.DTOs.Auth;

namespace CasaDeLasTortas.Controllers
{
   
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IJwtService _jwtService;

        public AccountController(ILogger<AccountController> logger, IJwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/Login
        // Muestra la página de login.
        //  User.Identity ahora viene del JwtMiddleware
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            _logger.LogInformation("📍 Accediendo a página de Login");

            //  Si ya está autenticado (JWT válido procesado por JwtMiddleware), redirigir
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                _logger.LogInformation("Usuario ya autenticado con rol: {Role}, redirigiendo", userRole);

                return userRole switch
                {
                    "Vendedor"  => RedirectToAction("DashboardVue", "Vendedor"),
                    "Comprador" => RedirectToAction("DashboardVue", "Comprador"),
                    "Admin"     => RedirectToAction("Index", "Admin"),
                    _           => RedirectToAction("Index", "Home")
                };
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/Register
        // Muestra la página de registro.
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("📍 Accediendo a página de Register");

            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/ForgotPassword
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            _logger.LogInformation("📍 Accediendo a página de ForgotPassword");
            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/Logout
        // Ya no hace SignOutAsync
        // Retorna una vista que limpia localStorage y redirige al login
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult Logout()
        {
            _logger.LogInformation("🚪 Logout solicitado");
            
            // La vista Logout.cshtml se encarga de:
            // 1. Limpiar localStorage (authToken, user)
            // 2. Redirigir a /Account/Login
            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // POST: /Account/Logout
        // Para compatibilidad con formularios que usan POST
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutPost()
        {
            _logger.LogInformation("🚪 LogoutPost llamado");
            return RedirectToAction("Logout");
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/AccessDenied
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("⛔ Acceso denegado");
            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // GET: /Account/Profile
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult Profile()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToAction("Login");

            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // POST: /Account/ValidateToken
        //  Endpoint para verificar si un JWT sigue válido
        // Útil para que el frontend verifique antes de hacer peticiones
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest? request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { valid = false, message = "Token requerido" });
            }

            try
            {
                var principal = _jwtService.ValidateToken(request.Token);
                
                if (principal != null && principal.Identity?.IsAuthenticated == true)
                {
                    var userId   = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                    var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;

                    _logger.LogInformation("✅ Token válido para usuario: {Name}", userName);

                    return Ok(new 
                    { 
                        valid = true,
                        user = new
                        {
                            id     = userId,
                            nombre = userName,
                            rol    = userRole
                        }
                    });
                }

                return Ok(new { valid = false, message = "Token inválido o expirado" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validando token");
                return Ok(new { valid = false, message = "Token inválido" });
            }
        }

       
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Request para validar un token JWT
    /// </summary>
    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    
}