using TiendaFluent.Entities;
using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

// Listas que necesita el formulario de nuevo pedido
public record CatalogosPedido(List<Cliente> Clientes, List<Producto> ProductosConStock);

public interface IPedidoService
{
    Task<List<Pedido>> ListarAsync();
    Task<PedidoDetalleVM?> ObtenerDetalleAsync(int id);
    Task<CatalogosPedido> CatalogosAsync();

    // Devuelve el Id del pedido creado
    Task<Resultado<int>> CrearAsync(PedidoCrearVM vm);

    // rowVersion es la versión (en base64) que el usuario tenía en pantalla
    Task<Resultado> CambiarEstadoAsync(int id, EstadoPedido estado, string rowVersion);

    Task<PagoFormVM?> ObtenerFormularioPagoAsync(int pedidoId);
    Task<Resultado> RegistrarPagoAsync(PagoFormVM vm);
    Task<Resultado> EliminarAsync(int id);
}
