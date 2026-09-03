using InventarioAPI.Model;
using InventarioAPI.Repository.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Repository.Implementacion
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AppDbContext _context;

        public VentaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoOperacion> RegistrarVentaAsync(VentaRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return new ResultadoOperacion { Exito = false, Mensaje = "El carrito está vacío" };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = new Venta { FechaVenta = DateTime.Now };
                decimal total = 0;

                foreach (var item in request.Items)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null)
                        return new ResultadoOperacion { Exito = false, Mensaje = $"Producto {item.ProductoId} no encontrado" };

                    if (producto.Cantidad < item.Cantidad)
                        return new ResultadoOperacion { Exito = false, Mensaje = $"Stock insuficiente para {producto.Nombre}" };

                    var subtotal = producto.Precio * item.Cantidad;
                    total += subtotal;

                    producto.Cantidad -= item.Cantidad;
                    producto.Vendidos += item.Cantidad;

                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotal
                    });
                }

                venta.Total = total;
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultadoOperacion
                {
                    Exito = true,
                    Mensaje = "Venta registrada correctamente",
                    Datos = new { ventaId = venta.Id, total = venta.Total }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultadoOperacion { Exito = false, Mensaje = $"Error al procesar venta: {ex.Message}" };
            }
        }

        public async Task<DashboardDto> ObtenerDashboardAsync()
        {
            var totalProductos = await _context.Productos.CountAsync();
            var ventasTotales = await _context.Ventas.SumAsync(v => v.Total);
            var itemsVendidos = await _context.DetalleVentas.SumAsync(d => (int?)d.Cantidad) ?? 0;

            return new DashboardDto
            {
                TotalProductos = totalProductos,
                VentasTotales = ventasTotales,
                ItemsVendidos = itemsVendidos
            };
        }

        public async Task<IEnumerable<VentaPorDiaDto>> ObtenerVentasPorDiaAsync(int dias)
        {
            var hoy = DateTime.Today;
            var inicio = hoy.AddDays(-(dias - 1));

            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.FechaVenta.Date >= inicio && v.FechaVenta.Date <= hoy)
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new { Fecha = g.Key, Monto = g.Sum(v => v.Total) })
                .ToListAsync();

            var resultado = new List<VentaPorDiaDto>();
            for (int i = dias - 1; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);
                var ventaDia = ventas.FirstOrDefault(v => v.Fecha == fecha);
                resultado.Add(new VentaPorDiaDto
                {
                    Fecha = fecha.ToString("yyyy-MM-dd"),
                    Monto = ventaDia?.Monto ?? 0,
                    EtiquetaDia = fecha.ToString("ddd d/M", new System.Globalization.CultureInfo("es-MX"))
                });
            }

            return resultado;
        }

        public async Task<IEnumerable<TopProductoDto>> ObtenerTopProductosAsync(int limite)
        {
            return await _context.DetalleVentas
                .AsNoTracking()
                .Include(d => d.Producto)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre, d.Producto.Categoria })
                .Select(g => new TopProductoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(t => t.CantidadVendida)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopProductoDto>> ObtenerTopProductosHoyAsync(int limite)
        {
            var hoy = DateTime.Today;

            return await _context.DetalleVentas
                .AsNoTracking()
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                .Where(d => d.Venta!.FechaVenta.Date == hoy)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre, d.Producto.Categoria })
                .Select(g => new TopProductoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(t => t.CantidadVendida)
                .Take(limite)
                .ToListAsync();
        }
    }
}
