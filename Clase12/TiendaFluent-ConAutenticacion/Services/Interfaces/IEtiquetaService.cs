using TiendaFluent.Entities;

namespace TiendaFluent.Services.Interfaces;

public interface IEtiquetaService
{
    Task<List<Etiqueta>> ListarAsync();
    Task<Resultado> CrearAsync(string? nombre);
    Task<Resultado> EliminarAsync(int id);
}
