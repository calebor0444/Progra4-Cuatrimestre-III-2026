using Microsoft.EntityFrameworkCore;
using TiendaFluent.Entities;

namespace TiendaFluent.Data;

// Datos de ejemplo que se insertan con UseSeeding / UseAsyncSeeding (guía 13).
// Se ejecutan al aplicar las migraciones (dotnet ef database update o Database.Migrate()).
// Las categorías y etiquetas son catálogos fijos y se siembran con HasData en sus configuraciones.
public static class DatosDemo
{
    public static void Sembrar(TiendaContext db)
    {
        if (db.Productos.Any()) return;
        Agregar(db, db.Etiquetas.ToList());
        db.SaveChanges();
    }

    public static async Task SembrarAsync(TiendaContext db, CancellationToken ct)
    {
        if (await db.Productos.AnyAsync(ct)) return;
        Agregar(db, await db.Etiquetas.ToListAsync(ct));
        await db.SaveChangesAsync(ct);
    }

    private static void Agregar(TiendaContext db, List<Etiqueta> etiquetas)
    {
        Etiqueta E(string nombre) => etiquetas.First(e => e.Nombre == nombre);

        var laptop = new Producto { Sku = "LAP-001", Nombre = "Laptop 14 pulgadas Ryzen 7", Precio = 899.99m, Stock = 15, CategoriaId = 2, Etiquetas = { E("Nuevo"), E("Oferta") } };
        var mouse = new Producto { Sku = "MOU-010", Nombre = "Mouse inalámbrico", Precio = 24.50m, Stock = 120, CategoriaId = 4, Etiquetas = { E("Más vendido") } };
        var teclado = new Producto { Sku = "TEC-020", Nombre = "Teclado mecánico", Precio = 79.90m, Stock = 40, CategoriaId = 4, Etiquetas = { E("Nuevo") } };
        var cafetera = new Producto { Sku = "CAF-100", Nombre = "Cafetera de goteo", Precio = 45.00m, Stock = 25, CategoriaId = 3, Etiquetas = { E("Oferta"), E("Envío gratis") } };

        var ana = new Cliente
        {
            Nombre = "Ana Pérez",
            Email = "ana@ejemplo.com",
            Direccion = new Direccion { Calle = "Av. Central 123", Ciudad = "San José", CodigoPostal = "10101" },
            Perfil = new PerfilCliente { Telefono = "8888-1234", FechaNacimiento = new DateTime(1995, 4, 12) },
        };
        var luis = new Cliente
        {
            Nombre = "Luis Gómez",
            Email = "luis@ejemplo.com",
            Direccion = new Direccion { Calle = "Calle 5, casa 22", Ciudad = "Heredia" },
        };

        var pedido1 = new Pedido
        {
            Cliente = ana,
            Estado = EstadoPedido.Pagado,
            Detalles =
            {
                new DetallePedido { Producto = laptop, Cantidad = 1, PrecioUnitario = laptop.Precio },
                new DetallePedido { Producto = mouse, Cantidad = 2, PrecioUnitario = mouse.Precio },
            },
        };
        pedido1.Total = pedido1.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        var pedido2 = new Pedido
        {
            Cliente = luis,
            Estado = EstadoPedido.Pendiente,
            Detalles = { new DetallePedido { Producto = cafetera, Cantidad = 1, PrecioUnitario = cafetera.Precio } },
        };
        pedido2.Total = pedido2.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        var pago = new PagoTarjeta { Pedido = pedido1, Monto = pedido1.Total, Fecha = DateTime.UtcNow, Marca = "Visa", UltimosDigitos = "4242" };

        laptop.Stock -= 1;
        mouse.Stock -= 2;
        cafetera.Stock -= 1;

        db.AddRange(laptop, mouse, teclado, cafetera, ana, luis, pedido1, pedido2, pago);
    }
}
