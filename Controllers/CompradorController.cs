using Microsoft.AspNetCore.Mvc;

namespace CasaDeLasTortas.Controllers
{
    public class CompradorController : Controller
    {
        private readonly ILogger<CompradorController> _logger;

        public CompradorController(ILogger<CompradorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Dashboard Vue.js - La autenticación se maneja en el cliente
        /// </summary>
        [HttpGet]
        public IActionResult DashboardVue()
        {
            _logger.LogInformation(" Sirviendo vista DashboardVue del Comprador (autenticación en cliente)");
            
            // Solo servimos la vista, Vue.js manejará la autenticación
            return View();
        }

        /// <summary>
        /// Vista principal del comprador (alias para DashboardVue)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation(" Sirviendo vista Index del Comprador");
            return View("DashboardVue");
        }

        /// <summary>
        /// Perfil del comprador (también manejado por Vue.js)
        /// </summary>
        [HttpGet]
        public IActionResult Perfil()
        {
            _logger.LogInformation(" Sirviendo vista Perfil del Comprador (manejado por Vue.js)");
            return View("DashboardVue");
        }

        /// <summary>
        /// Historial de compras (manejado por Vue.js)
        /// </summary>
        [HttpGet]
        public IActionResult Historial()
        {
            _logger.LogInformation(" Sirviendo vista Historial del Comprador (manejado por Vue.js)");
            return View("DashboardVue");
        }

        /// <summary>
        /// Catálogo de tortas (manejado por Vue.js)
        /// </summary>
        [HttpGet]
        public IActionResult Catalogo()
        {
            _logger.LogInformation(" Sirviendo vista Catalogo del Comprador (manejado por Vue.js)");
            return View("DashboardVue");
        }

        /// <summary>
        /// Carrito de compras (manejado por Vue.js)
        /// </summary>
        [HttpGet]
        public IActionResult Carrito()
        {
            _logger.LogInformation(" Sirviendo vista Carrito del Comprador (manejado por Vue.js)");
            return View("DashboardVue");
        }
    }
}