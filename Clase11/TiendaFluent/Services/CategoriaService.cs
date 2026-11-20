using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Demuestra: relación autorreferenciada (guía 9.8) y OnDelete Restrict (guía 10)
public class CategoriaService(TiendaContext db) : ICategoriaService
{
    public Task<List<CategoriaListaVM>> ListarAsync() =>
        db.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaListaVM(
                c.Id, c.Nombre,
                c.CategoriaPadre != null ? c.CategoriaPadre.Nombre : null,
                c.Productos.Count, c.Subcategorias.Count))
            .ToListAsync();

    public Task<List<Categoria>> OpcionesPadreAsync(int? excluirId = null) =>
        db.Categorias.Where(c => c.Id != excluirId).OrderBy(c => c.Nombre).ToListAsync();

    public async Task<CategoriaFormVM?> ObtenerFormularioAsync(int id)
    {
        var c = await db.Categorias.FindAsync(id);
        return c is null ? null : new CategoriaFormVM { Id = c.Id, Nombre = c.Nombre, CategoriaPadreId = c.CategoriaPadreId };
    }

    public async Task<Resultado> CrearAsync(CategoriaFormVM vm)
    {
        db.Categorias.Add(new Categoria { Nombre = vm.Nombre.Trim(), CategoriaPadreId = vm.CategoriaPadreId });
        await db.SaveChangesAsync();
        return Resultado.Ok("Categoría creada.");
    }

    public async Task<Resultado> ActualizarAsync(int id, CategoriaFormVM vm)
    {
        var c = await db.Categorias.FindAsync(id);
        if (c is null) return Resultado.NoExiste();

        if (await CreariaCiclo(id, vm.CategoriaPadreId))
            return Resultado.Falla("La categoría padre no puede ser la misma categoría ni una de sus subcategorías.",
                                   nameof(CategoriaFormVM.CategoriaPadreId));

        c.Nombre = vm.Nombre.Trim();
        c.CategoriaPadreId = vm.CategoriaPadreId;
        await db.SaveChangesAsync();
        return Resultado.Ok("Categoría actualizada.");
    }

    public async Task<Resultado> EliminarAsync(int id)
    {
        var c = await db.Categorias.FindAsync(id);
        if (c is null) return Resultado.NoExiste();

        db.Categorias.Remove(c);
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Categoría \"{c.Nombre}\" eliminada.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsLlaveForanea(ex))
        {
            // La FK se creó con ON DELETE NO ACTION (DeleteBehavior.Restrict): SQL Server rechaza el borrado
            return Resultado.Falla($"No se puede eliminar \"{c.Nombre}\": tiene productos o subcategorías (OnDelete Restrict).");
        }
    }

    // En una autorreferencia hay que evitar ciclos (A → B → A): se sube por los padres
    // del padre elegido y, si en algún punto aparece la propia categoría, el cambio no es válido
    private async Task<bool> CreariaCiclo(int id, int? nuevoPadreId)
    {
        if (nuevoPadreId is null) return false;
        var padres = await db.Categorias.ToDictionaryAsync(c => c.Id, c => c.CategoriaPadreId);

        int? actual = nuevoPadreId;
        while (actual is not null)
        {
            if (actual == id) return true;
            actual = padres.GetValueOrDefault(actual.Value);
        }
        return false;
    }
}
