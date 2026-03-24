using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CasaDeLasTortas.Services;

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

        // GET: /Account/Login
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            _logger.LogInformation("📍 Accediendo a página de Login");

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var referer = Request.Headers["Referer"].ToString();
                var estaViniendoDelDashboard =
                    referer.Contains("DashboardVue", StringComparison.OrdinalIgnoreCase) ||
                    referer.Contains("Comprador", StringComparison.OrdinalIgnoreCase) ||
                    referer.Contains("Vendedor", StringComparison.OrdinalIgnoreCase);

                if (!estaViniendoDelDashboard)
                {
                    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                    _logger.LogInformation("Usuario autenticado con rol: {Role}, redirigiendo", userRole);

                    return userRole switch
                    {
                        "Vendedor" => RedirectToAction("DashboardVue", "Vendedor"),
                        "Comprador" => RedirectToAction("DashboardVue", "Comprador"),
                        "Admin" => RedirectToAction("Index", "Admin"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                _logger.LogWarning("Loop detectado, limpiando cookie y redirigiendo limpio");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Redirect("/Account/Login" + (returnUrl != null ? $"?returnUrl={Uri.EscapeDataString(returnUrl)}" : ""));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ═══════════════════════════════════════════════════════════════
        // 🔥 ENDPOINT CRÍTICO: SetCookieSession
        // Establece la cookie de autenticación a partir del token JWT
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> SetCookieSession([FromBody] SetCookieRequest? request)
        {
            try
            {
                _logger.LogInformation("🔐 SetCookieSession llamado");

                if (request == null || string.IsNullOrEmpty(request.Token))
                {
                    _logger.LogWarning("SetCookieSession: Token vacío o nulo");
                    return BadRequest(new { success = false, message = "Token requerido" });
                }

                // Validar el token JWT
                var principal = _jwtService.ValidateToken(request.Token);

                if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("SetCookieSession: Token inválido");
                    return Unauthorized(new { success = false, message = "Token inválido" });
                }

                // Crear claims para la cookie
                var claims = new List<Claim>();
                foreach (var claim in principal.Claims)
                {
                    // Evitar duplicados
                    if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
                    AllowRefresh = true
                };

                // Establecer la cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                var userName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "desconocido";
                var userRole = principal.FindFirst(ClaimTypes.Role)?.Value ?? "sin rol";

                _logger.LogInformation("✅ Cookie de sesión establecida para: {Name} (Rol: {Role})", userName, userRole);

                return Ok(new { success = true, message = "Sesión establecida correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al establecer cookie de sesión");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("📍 Accediendo a página de Register");

            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            _logger.LogInformation("📍 Accediendo a página de ForgotPassword");
            return View();
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("🚪 Cerrando sesión");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // POST: /Account/Logout (para forms con antiforgery)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("⛔ Acceso denegado");
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToAction("Login");

            return View();
        }
    }

    // DTO para el request de SetCookieSession
    public class SetCookieRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}