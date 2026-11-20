using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Demuestra: índice único (guía 8.1) y borrado en cascada de la tabla intermedia N:N (guía 9.5)
public class EtiquetaService(TiendaContext db) : IEtiquetaService
{
    public Task<List<Etiqueta>> ListarAsync() =>
        db.Etiquetas.Include(e => e.Productos).OrderBy(e => e.Nombre).ToListAsync();

    public async Task<Resultado> CrearAsync(string? nombre)
    {
        nombre = (nombre ?? "").Trim();
        if (nombre.Length is 0 or > 50)
            return Resultado.Falla("El nombre es obligatorio y debe tener máximo 50 caracteres.");

        db.Etiquetas.Add(new Etiqueta { Nombre = nombre });
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Etiqueta \"{nombre}\" creada.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsDuplicado(ex))
        {
            // HasIndex(e => e.Nombre).IsUnique() impide nombres repetidos
            return Resultado.Falla($"Ya existe una etiqueta llamada \"{nombre}\" (índice único).");
        }
    }

    public async Task<Resultado> EliminarAsync(int id)
    {
        var e = await db.Etiquetas.FindAsync(id);
        if (e is null) return Resultado.NoExiste();

        db.Etiquetas.Remove(e);   // ProductoEtiquetas tiene ON DELETE CASCADE: se quitan sus asociaciones
        await db.SaveChangesAsync();
        return Resultado.Ok($"Etiqueta \"{e.Nombre}\" eliminada; se quitó de los productos que la tenían.");
    }
}
