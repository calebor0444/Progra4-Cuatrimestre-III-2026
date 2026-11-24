using TiendaFluent.Entities;
using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

public interface ICategoriaService
{
    Task<List<CategoriaListaVM>> ListarAsync();

    // Categorías que se pueden elegir como padre (sin la que se está editando)
    Task<List<Categoria>> OpcionesPadreAsync(int? excluirId = null);

    Task<CategoriaFormVM?> ObtenerFormularioAsync(int id);
    Task<Resultado> CrearAsync(CategoriaFormVM vm);
    Task<Resultado> ActualizarAsync(int id, CategoriaFormVM vm);
    Task<Resultado> EliminarAsync(int id);
}
