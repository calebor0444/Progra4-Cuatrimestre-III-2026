using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);

        // Único solo entre los clientes no eliminados: índice filtrado (guía 8.1)
        builder.HasIndex(c => c.Email).IsUnique().HasFilter("[Eliminado] = 0");

        // Auditoría sin ensuciar la clase: propiedad sombra (guía 7.8)
        builder.Property<DateTime>("FechaCreacion").HasDefaultValueSql("GETUTCDATE()");

        // Borrado lógico: filtro global (guía 14)
        builder.HasQueryFilter(c => !c.Eliminado);

        // Dirección en columnas de la misma tabla: tipo propio (guía 11.1)
        builder.OwnsOne(c => c.Direccion, d =>
        {
            d.Property(x => x.Calle).HasColumnName("Calle").HasMaxLength(200);
            d.Property(x => x.Ciudad).HasColumnName("Ciudad").HasMaxLength(100);
            d.Property(x => x.CodigoPostal).HasColumnName("CodigoPostal").HasMaxLength(10);
        });
        builder.Navigation(c => c.Direccion).IsRequired();

        // 1:1 con el perfil (guía 9.4)
        builder.HasOne(c => c.Perfil)
               .WithOne(p => p.Cliente)
               .HasForeignKey<PerfilCliente>(p => p.ClienteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
