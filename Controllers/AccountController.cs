using Microsoft.AspNetCore.Mvc;

namespace CasaDeLasTortas.Controllers
{
    public class AccountController : Controller  
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            _logger.LogInformation("📍 Accediendo a página de Login");
            
            // Si ya está autenticado, redirigir según su rol
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                _logger.LogInformation("Usuario ya autenticado con rol: {Role}", userRole);
                
                return userRole switch
                {
                    "Vendedor" => RedirectToAction("DashboardVue", "Vendedor"),
                    "Comprador" => RedirectToAction("Index", "Comprador"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("📍 Accediendo a página de Register");
            
            // Si ya está autenticado, redirigir
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            _logger.LogInformation("🚪 Cerrando sesión (logout del lado del servidor)");
            
            // En JWT puro, el logout real se hace en el cliente (borrar token)
            // Este método solo muestra una página de confirmación o redirige
            
            return View(); // O redirigir directamente: return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("⛔ Acceso denegado");
            return View();
        }
    }
}