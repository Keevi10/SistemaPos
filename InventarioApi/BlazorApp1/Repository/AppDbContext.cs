using InventarioAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("tbl_productos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Categoria).HasMaxLength(50);
                entity.Property(e => e.CodigoBarras).HasMaxLength(50);
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("tbl_ventas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("tbl_detalle_venta");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(e => e.VentaId);
                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.ProductoId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
