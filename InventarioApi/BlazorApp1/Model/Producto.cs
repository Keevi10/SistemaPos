namespace InventarioAPI.Model
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Categoria { get; set; } = "General";
        public string? CodigoBarras { get; set; }
        public int Vendidos { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
