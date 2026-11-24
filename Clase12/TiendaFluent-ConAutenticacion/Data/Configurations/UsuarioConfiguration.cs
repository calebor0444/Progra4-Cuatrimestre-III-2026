using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TiendaFluent.Entities;

namespace TiendaFluent.Data.Configurations;

// Las tablas de Identity (AspNetUsers, AspNetRoles, ...) las configura IdentityDbContext.
// Aquí solo se configura la columna extra que agrega Usuario.
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
    }
}
