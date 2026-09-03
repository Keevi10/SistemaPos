using Microsoft.AspNetCore.Mvc;

namespace InventarioWeb.Controllers
{
    public class PosController : Controller
    {
        private readonly IConfiguration _configuration;

        public PosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.ApiUrl = _configuration["URLAPI"];
            ViewBag.NombreTienda = _configuration["ConfiguracionTienda:NombreTienda"] ?? "Mi Tienda";
            ViewBag.TextoBienvenida = _configuration["ConfiguracionTienda:TextoBienvenida"] ?? "Bienvenido al sistema POS";
            return View();
        }

        [HttpPost]
        public IActionResult ValidarClaveInventario([FromBody] ClaveRequest request)
        {
            if (request.Clave == "79919302")
                return Ok(new { valido = true });

            return Unauthorized(new { valido = false, mensaje = "Clave incorrecta" });
        }
    }

    public class ClaveRequest
    {
        public string Clave { get; set; } = string.Empty;
    }
}
