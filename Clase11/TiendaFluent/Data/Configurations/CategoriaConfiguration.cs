using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);

        // Autorreferencia (guía 9.8)
        builder.HasOne(c => c.CategoriaPadre)
               .WithMany(c => c.Subcategorias)
               .HasForeignKey(c => c.CategoriaPadreId)
               .OnDelete(DeleteBehavior.Restrict);

        // Datos semilla (guía 13)
        builder.HasData(
            new Categoria { Id = 1, Nombre = "Electrónica" },
            new Categoria { Id = 2, Nombre = "Computadoras", CategoriaPadreId = 1 },
            new Categoria { Id = 3, Nombre = "Hogar" },
            new Categoria { Id = 4, Nombre = "Accesorios", CategoriaPadreId = 2 });
    }
}
