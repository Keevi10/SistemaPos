using InventarioAPI.Model;
using InventarioAPI.Repository.Interfaz;
using Microsoft.AspNetCore.Mvc;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly IVentaRepository _ventaRepository;

        public VentasController(IVentaRepository ventaRepository)
        {
            _ventaRepository = ventaRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] VentaRequest request)
        {
            var resultado = await _ventaRepository.RegistrarVentaAsync(request);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var datos = await _ventaRepository.ObtenerDashboardAsync();
            return Ok(datos);
        }

        [HttpGet("por-dia")]
        public async Task<IActionResult> VentasPorDia([FromQuery] int dias = 7)
        {
            var datos = await _ventaRepository.ObtenerVentasPorDiaAsync(dias);
            return Ok(datos);
        }

        [HttpGet("top-productos")]
        public async Task<IActionResult> TopProductos([FromQuery] int limite = 5)
        {
            var datos = await _ventaRepository.ObtenerTopProductosAsync(limite);
            return Ok(datos);
        }

        [HttpGet("top-productos-hoy")]
        public async Task<IActionResult> TopProductosHoy([FromQuery] int limite = 10)
        {
            var datos = await _ventaRepository.ObtenerTopProductosHoyAsync(limite);
            return Ok(datos);
        }
    }
}
