using Microsoft.EntityFrameworkCore;
using TiendaFluent.Entities;

namespace TiendaFluent.Data;

public class TiendaContext : DbContext
{
    public TiendaContext(DbContextOptions<TiendaContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PerfilCliente> PerfilesCliente => Set<PerfilCliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Etiqueta> Etiquetas => Set<Etiqueta>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Secuencia para el número de pedido visible al cliente (guía 6.4)
        modelBuilder.HasSequence<int>("NumerosPedido").StartsAt(1000).IncrementsBy(1);

        // Aplica todas las clases IEntityTypeConfiguration<T> de Data/Configurations (guía 2.4)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TiendaContext).Assembly);
    }

    // Convenciones globales por tipo (guía 15)
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(200);
    }
}
