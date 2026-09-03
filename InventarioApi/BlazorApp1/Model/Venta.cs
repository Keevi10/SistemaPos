namespace InventarioAPI.Model
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
