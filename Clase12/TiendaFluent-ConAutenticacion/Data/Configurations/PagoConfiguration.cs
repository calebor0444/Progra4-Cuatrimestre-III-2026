using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Monto).HasPrecision(18, 2);

        // Herencia TPH con columna discriminadora (guía 12.1)
        builder.HasDiscriminator<string>("TipoPago")
               .HasValue<PagoTarjeta>("Tarjeta")
               .HasValue<PagoTransferencia>("Transferencia");
        builder.Property("TipoPago").HasMaxLength(20);

        // Relación unidireccional: Pedido no tiene colección de pagos (guía 9.7)
        builder.HasOne(p => p.Pedido)
               .WithMany()
               .HasForeignKey(p => p.PedidoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.Pedido.Cliente.Eliminado);
    }
}
