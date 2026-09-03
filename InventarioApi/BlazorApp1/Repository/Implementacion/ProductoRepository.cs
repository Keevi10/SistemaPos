using InventarioAPI.Model;
using InventarioAPI.Repository.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Repository.Implementacion
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras)
        {
            return await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
        }

        public async Task<IEnumerable<Producto>> BuscarAsync(string termino)
        {
            var busqueda = termino.Trim().ToLower();
            return await _context.Productos
                .AsNoTracking()
                .Where(p => p.Cantidad > 0 &&
                    (p.Nombre.ToLower().Contains(busqueda) ||
                     (p.CodigoBarras != null && p.CodigoBarras.Contains(termino))))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto> CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task<Producto?> ActualizarAsync(int id, ProductoUpdateRequest request)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Nombre))
                producto.Nombre = request.Nombre.Trim();

            if (request.Precio.HasValue)
                producto.Precio = request.Precio.Value;

            if (request.Cantidad.HasValue)
                producto.Cantidad = request.Cantidad.Value;

            if (!string.IsNullOrWhiteSpace(request.Categoria))
                producto.Categoria = request.Categoria;

            if (request.CodigoBarras != null)
                producto.CodigoBarras = string.IsNullOrWhiteSpace(request.CodigoBarras) ? null : request.CodigoBarras.Trim();

            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task<Producto?> AgregarStockPorCodigoBarrasAsync(string codigoBarras, int cantidad)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
            if (producto == null) return null;

            producto.Cantidad += cantidad;
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return false;

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarAsync()
        {
            return await _context.Productos.CountAsync();
        }
    }
}
