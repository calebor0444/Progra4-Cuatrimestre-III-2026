using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class DetallePedidoConfiguration : IEntityTypeConfiguration<DetallePedido>
{
    public void Configure(EntityTypeBuilder<DetallePedido> builder)
    {
        builder.ToTable("DetallesPedido", t =>
            t.HasCheckConstraint("CK_DetallesPedido_Cantidad", "[Cantidad] > 0"));

        // Llave compuesta (guía 6.2)
        builder.HasKey(d => new { d.PedidoId, d.ProductoId });

        builder.Property(d => d.PrecioUnitario).HasPrecision(18, 2);

        // Columna calculada y persistida (guía 7.5)
        builder.Property(d => d.Subtotal)
               .HasPrecision(18, 2)
               .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]", stored: true);

        // N:N con datos propios = dos relaciones 1:N (guía 9.5 c)
        builder.HasOne(d => d.Pedido)
               .WithMany(p => p.Detalles)
               .HasForeignKey(d => d.PedidoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
               .WithMany()
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(d => !d.Pedido.Cliente.Eliminado);
    }
}
