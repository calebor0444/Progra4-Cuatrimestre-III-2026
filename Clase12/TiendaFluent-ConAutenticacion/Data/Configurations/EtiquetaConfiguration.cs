using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class EtiquetaConfiguration : IEntityTypeConfiguration<Etiqueta>
{
    public void Configure(EntityTypeBuilder<Etiqueta> builder)
    {
        builder.ToTable("Etiquetas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Nombre).IsUnique();

        builder.HasData(
            new Etiqueta { Id = 1, Nombre = "Nuevo" },
            new Etiqueta { Id = 2, Nombre = "Oferta" },
            new Etiqueta { Id = 3, Nombre = "Más vendido" },
            new Etiqueta { Id = 4, Nombre = "Envío gratis" });
    }
}
