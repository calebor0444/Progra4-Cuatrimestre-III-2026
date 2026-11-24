using TiendaFluent.Entities;
using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

public interface IClienteService
{
    Task<List<Cliente>> ListarAsync(bool incluirEliminados);
    Task<ClienteDetalleVM?> ObtenerDetalleAsync(int id);
    Task<ClienteFormVM?> ObtenerFormularioAsync(int id);

    // Devuelve el Id del cliente creado
    Task<Resultado<int>> CrearAsync(ClienteFormVM vm);

    Task<Resultado> ActualizarAsync(int id, ClienteFormVM vm);
    Task<Resultado> EliminarAsync(int id);
    Task<Resultado> RestaurarAsync(int id);
}
