using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Demuestra: N:N con UsingEntity (guía 9.5), llave alterna Sku (guía 6.5),
// restricciones CHECK (guía 8.2) y OnDelete Restrict desde DetallesPedido (guía 10)
public class ProductoService(TiendaContext db) : IProductoService
{
    public async Task<List<Producto>> BuscarAsync(ProductoFiltroVM filtro)
    {
        // 1. Partimos de una consulta "abierta" (IQueryable): todavía NO va a la base de datos
        IQueryable<Producto> consulta = db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Etiquetas);

        // 2. Agregamos un Where por cada criterio que el usuario llenó.
        //    Varios Where seguidos equivalen a unirlos con AND.
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(texto) || p.Sku.Contains(texto));
        }

        if (filtro.CategoriaId is not null)
            consulta = consulta.Where(p => p.CategoriaId == filtro.CategoriaId);

        // Filtro sobre la relación N:N: productos que tengan AL MENOS una etiqueta con ese Id
        if (filtro.EtiquetaId is not null)
            consulta = consulta.Where(p => p.Etiquetas.Any(e => e.Id == filtro.EtiquetaId));

        if (filtro.PrecioMin is not null)
            consulta = consulta.Where(p => p.Precio >= filtro.PrecioMin);

        if (filtro.PrecioMax is not null)
            consulta = consulta.Where(p => p.Precio <= filtro.PrecioMax);

        if (filtro.SoloConStock)
            consulta = consulta.Where(p => p.Stock > 0);

        // 3. Ordenamiento elegido por el usuario
        consulta = filtro.Orden switch
        {
            "precio_asc" => consulta.OrderBy(p => p.Precio),
            "precio_desc" => consulta.OrderByDescending(p => p.Precio),
            _ => consulta.OrderBy(p => p.Nombre),
        };

        // 4. Recién aquí EF traduce todo a un solo SELECT ... WHERE ... ORDER BY y lo ejecuta
        return await consulta.ToListAsync();
    }

    public async Task<CatalogosProducto> CatalogosAsync() => new(
        await db.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
        await db.Etiquetas.OrderBy(e => e.Nombre).ToListAsync());

    public async Task<ProductoFormVM?> ObtenerFormularioAsync(int id)
    {
        var p = await db.Productos.Include(x => x.Etiquetas).FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : new ProductoFormVM
        {
            Id = p.Id, Sku = p.Sku, Nombre = p.Nombre, Precio = p.Precio, Stock = p.Stock,
            CategoriaId = p.CategoriaId, EtiquetasSeleccionadas = p.Etiquetas.Select(e => e.Id).ToList(),
        };
    }

    public async Task<Resultado> CrearAsync(ProductoFormVM vm)
    {
        var producto = new Producto
        {
            Sku = vm.Sku.Trim().ToUpperInvariant(),
            Nombre = vm.Nombre.Trim(),
            Precio = vm.Precio,
            Stock = vm.Stock,
            CategoriaId = vm.CategoriaId!.Value,
            // Basta con llenar la colección: EF inserta las filas en ProductoEtiquetas
            Etiquetas = await db.Etiquetas.Where(e => vm.EtiquetasSeleccionadas.Contains(e.Id)).ToListAsync(),
        };
        db.Productos.Add(producto);
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok("Producto creado.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsDuplicado(ex))
        {
            return Resultado.Falla("Ya existe un producto con ese SKU (llave alterna AK_Productos_Sku).", nameof(ProductoFormVM.Sku));
        }
    }

    public async Task<Resultado> ActualizarAsync(int id, ProductoFormVM vm)
    {
        var p = await db.Productos.Include(x => x.Etiquetas).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return Resultado.NoExiste();

        p.Nombre = vm.Nombre.Trim();
        p.Precio = vm.Precio;
        p.Stock = vm.Stock;
        p.CategoriaId = vm.CategoriaId!.Value;

        // Sincronizar la relación N:N: EF calcula qué filas insertar o borrar en ProductoEtiquetas
        p.Etiquetas.RemoveAll(e => !vm.EtiquetasSeleccionadas.Contains(e.Id));
        var actuales = p.Etiquetas.Select(e => e.Id).ToList();
        var nuevas = await db.Etiquetas
            .Where(e => vm.EtiquetasSeleccionadas.Contains(e.Id) && !actuales.Contains(e.Id))
            .ToListAsync();
        p.Etiquetas.AddRange(nuevas);

        await db.SaveChangesAsync();
        return Resultado.Ok("Producto actualizado.");
    }

    public async Task<Resultado> EliminarAsync(int id)
    {
        var p = await db.Productos.FindAsync(id);
        if (p is null) return Resultado.NoExiste();

        db.Productos.Remove(p);
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Producto \"{p.Nombre}\" eliminado.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsLlaveForanea(ex))
        {
            return Resultado.Falla($"No se puede eliminar \"{p.Nombre}\": ya aparece en pedidos (DetallesPedido usa OnDelete Restrict).");
        }
    }
}
