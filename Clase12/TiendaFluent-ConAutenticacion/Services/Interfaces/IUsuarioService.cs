using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

// idActual = Id del administrador que hace la petición, para no dejarlo quitarse permisos a sí mismo
public interface IUsuarioService
{
    Task<List<UsuarioListaVM>> ListarAsync();
    Task<UsuarioFormVM?> ObtenerFormularioAsync(string id);
    Task<Resultado> CrearAsync(UsuarioFormVM vm);
    Task<Resultado> ActualizarAsync(string id, UsuarioFormVM vm, string idActual);
    Task<Resultado> CambiarBloqueoAsync(string id, string idActual);
    Task<Resultado> EliminarAsync(string id, string idActual);
}
