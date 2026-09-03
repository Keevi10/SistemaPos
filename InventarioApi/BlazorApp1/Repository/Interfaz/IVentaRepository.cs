using InventarioAPI.Model;

namespace InventarioAPI.Repository.Interfaz
{
    public interface IVentaRepository
    {
        Task<ResultadoOperacion> RegistrarVentaAsync(VentaRequest request);
        Task<DashboardDto> ObtenerDashboardAsync();
        Task<IEnumerable<VentaPorDiaDto>> ObtenerVentasPorDiaAsync(int dias);
        Task<IEnumerable<TopProductoDto>> ObtenerTopProductosAsync(int limite);
        Task<IEnumerable<TopProductoDto>> ObtenerTopProductosHoyAsync(int limite);
    }
}
