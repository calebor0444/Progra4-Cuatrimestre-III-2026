using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

public class PerfilClienteConfiguration : IEntityTypeConfiguration<PerfilCliente>
{
    public void Configure(EntityTypeBuilder<PerfilCliente> builder)
    {
        builder.ToTable("PerfilesCliente");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Telefono).HasMaxLength(20).IsUnicode(false);
        builder.Property(p => p.FechaNacimiento).HasColumnType("date");

        // Mismo filtro que el principal para evitar perfiles "huérfanos" en las consultas (guía 14)
        builder.HasQueryFilter(p => !p.Cliente.Eliminado);
    }
}
