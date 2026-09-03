namespace InventarioAPI.Model
{
    public class ProductoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Categoria { get; set; } = "General";
        public string? CodigoBarras { get; set; }
    }

    public class ProductoUpdateRequest
    {
        public string? Nombre { get; set; }
        public decimal? Precio { get; set; }
        public int? Cantidad { get; set; }
        public string? Categoria { get; set; }
        public string? CodigoBarras { get; set; }
    }

    public class VentaItemRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaRequest
    {
        public List<VentaItemRequest> Items { get; set; } = new();
    }

    public class DashboardDto
    {
        public int TotalProductos { get; set; }
        public decimal VentasTotales { get; set; }
        public int ItemsVendidos { get; set; }
    }

    public class VentaPorDiaDto
    {
        public string Fecha { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string EtiquetaDia { get; set; } = string.Empty;
    }

    public class TopProductoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class AddStockRequest
    {
        public int Cantidad { get; set; }
    }

    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public object? Datos { get; set; }
    }
}
