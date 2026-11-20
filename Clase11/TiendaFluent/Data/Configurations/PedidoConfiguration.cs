using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos", t =>
            t.HasCheckConstraint("CK_Pedidos_Total", "[Total] >= 0"));
        builder.HasKey(p => p.Id);

        // Número tomado de la secuencia definida en TiendaContext (guía 6.4)
        builder.Property(p => p.Numero)
               .HasDefaultValueSql("NEXT VALUE FOR NumerosPedido");
        builder.HasIndex(p => p.Numero).IsUnique();

        builder.Property(p => p.Fecha)
               .HasColumnType("datetime2(0)")
               .HasDefaultValueSql("GETUTCDATE()");

        // Enum guardado como texto (guía 7.6)
        builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Total).HasPrecision(18, 2);

        // Concurrencia optimista (guía 7.7)
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasIndex(p => new { p.ClienteId, p.Fecha });

        // 1:N con Cliente; no se puede borrar un cliente con pedidos (guía 9.3 y 10)
        builder.HasOne(p => p.Cliente)
               .WithMany(c => c.Pedidos)
               .HasForeignKey(p => p.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.Cliente.Eliminado);
    }
}
