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
    public Task<List<Producto>> ListarAsync() =>
        db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Etiquetas)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

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
