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
        /// Dashboard Vue.js - La autenticación y navegación se maneja en el cliente
        /// </summary>
        [HttpGet]
        public IActionResult DashboardVue()
        {
            _logger.LogInformation("📍 Sirviendo vista DashboardVue del Comprador");

            // Solo servimos la vista, Vue.js manejará TODO
            return View();
        }

        /// <summary>
        /// Vista principal - alias para DashboardVue
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("📍 Sirviendo vista Index del Comprador");
            return View("DashboardVue");
        }

        [HttpGet]
        [Route("Catalogo")]
        public IActionResult Catalogo()
        {
            return View("DashboardVue");
        }

        [HttpGet]
        [Route("Carrito")]
        public IActionResult Carrito()
        {
            return View("DashboardVue");
        }

        [HttpGet]
        [Route("Historial")]
        public IActionResult Historial()
        {
            return View("DashboardVue");
        }
    }
}