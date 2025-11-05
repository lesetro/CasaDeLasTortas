using Microsoft.AspNetCore.Mvc;

namespace CasaDeLasTortas.Controllers
{
    public class VendedorController : Controller
    {
        private readonly ILogger<VendedorController> _logger;

        public VendedorController(ILogger<VendedorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Dashboard Vue.js - La autenticación se maneja en el cliente
        /// </summary>
        [HttpGet]
        public IActionResult DashboardVue()
        {
            _logger.LogInformation("📍 Sirviendo vista DashboardVue (autenticación en cliente)");
            
            // Solo servimos la vista, Vue.js manejará la autenticación
            return View();
        }

        /// <summary>
        /// Dashboard tradicional (si lo necesitas)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("📍 Sirviendo vista Index");
            return View();
        }
    }
}