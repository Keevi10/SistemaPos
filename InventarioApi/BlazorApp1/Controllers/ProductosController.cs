using InventarioAPI.Model;
using InventarioAPI.Repository.Interfaz;
using Microsoft.AspNetCore.Mvc;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;

        public ProductosController(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var productos = await _productoRepository.ObtenerTodosAsync();
            return Ok(productos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(producto);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Ok(await _productoRepository.ObtenerTodosAsync());

            var productos = await _productoRepository.BuscarAsync(termino);
            return Ok(productos);
        }

        [HttpGet("codigo/{codigoBarras}")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigoBarras)
        {
            var producto = await _productoRepository.ObtenerPorCodigoBarrasAsync(codigoBarras);
            if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProductoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                return BadRequest(new { mensaje = "El nombre es requerido" });

            if (request.Precio <= 0 || request.Cantidad < 0)
                return BadRequest(new { mensaje = "Precio y cantidad deben ser válidos" });

            var total = await _productoRepository.ContarAsync();
            if (total >= 999)
                return BadRequest(new { mensaje = "Límite de 999 productos alcanzado" });

            if (!string.IsNullOrWhiteSpace(request.CodigoBarras))
            {
                var existente = await _productoRepository.ObtenerPorCodigoBarrasAsync(request.CodigoBarras.Trim());
                if (existente != null)
                    return BadRequest(new { mensaje = "Este código de barras ya existe" });
            }

            var producto = new Producto
            {
                Nombre = request.Nombre.Trim(),
                Precio = request.Precio,
                Cantidad = request.Cantidad,
                Categoria = string.IsNullOrWhiteSpace(request.Categoria) ? "General" : request.Categoria,
                CodigoBarras = string.IsNullOrWhiteSpace(request.CodigoBarras) ? null : request.CodigoBarras.Trim(),
                Vendidos = 0,
                FechaCreacion = DateTime.Now
            };

            var creado = await _productoRepository.CrearAsync(producto);
            return Ok(creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProductoUpdateRequest request)
        {
            var producto = await _productoRepository.ActualizarAsync(id, request);
            if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(producto);
        }

        [HttpPut("codigo/{codigoBarras}/stock")]
        public async Task<IActionResult> AgregarStock(string codigoBarras, [FromBody] AddStockRequest request)
        {
            if (request.Cantidad <= 0)
                return BadRequest(new { mensaje = "La cantidad debe ser mayor a 0" });

            var producto = await _productoRepository.AgregarStockPorCodigoBarrasAsync(codigoBarras, request.Cantidad);
            if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(producto);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _productoRepository.EliminarAsync(id);
            if (!eliminado) return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { mensaje = "Producto eliminado" });
        }
    }
}
