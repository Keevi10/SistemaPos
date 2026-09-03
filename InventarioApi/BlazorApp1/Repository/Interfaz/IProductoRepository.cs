using InventarioAPI.Model;

namespace InventarioAPI.Repository.Interfaz
{
    public interface IProductoRepository
    {
        Task<IEnumerable<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras);
        Task<IEnumerable<Producto>> BuscarAsync(string termino);
        Task<Producto> CrearAsync(Producto producto);
        Task<Producto?> ActualizarAsync(int id, ProductoUpdateRequest request);
        Task<bool> EliminarAsync(int id);
        Task<int> ContarAsync();
        Task<Producto?> AgregarStockPorCodigoBarrasAsync(string codigoBarras, int cantidad);
    }
}
