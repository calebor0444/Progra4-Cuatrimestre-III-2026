using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        // Restricciones CHECK (guía 8.2)
        builder.ToTable("Productos", t =>
        {
            t.HasCheckConstraint("CK_Productos_Precio", "[Precio] >= 0");
            t.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
        });
        builder.HasKey(p => p.Id);

        // Llave alterna (guía 6.5)
        builder.Property(p => p.Sku).HasMaxLength(20).IsUnicode(false);
        builder.HasAlternateKey(p => p.Sku);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Precio).HasPrecision(18, 2);

        builder.HasOne(p => p.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(p => p.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);

        // N:N con tabla intermedia personalizada (guía 9.5 b)
        builder.HasMany(p => p.Etiquetas)
               .WithMany(e => e.Productos)
               .UsingEntity(
                   "ProductoEtiquetas",
                   r => r.HasOne(typeof(Etiqueta)).WithMany().HasForeignKey("EtiquetaId"),
                   l => l.HasOne(typeof(Producto)).WithMany().HasForeignKey("ProductoId"),
                   j => j.HasKey("ProductoId", "EtiquetaId"));
    }
}
