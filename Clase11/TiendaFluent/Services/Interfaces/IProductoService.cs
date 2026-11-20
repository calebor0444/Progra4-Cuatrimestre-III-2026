using TiendaFluent.Entities;
using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

// Listas que necesita el formulario de producto
public record CatalogosProducto(List<Categoria> Categorias, List<Etiqueta> Etiquetas);

public interface IProductoService
{
    Task<List<Producto>> ListarAsync();
    Task<CatalogosProducto> CatalogosAsync();
    Task<ProductoFormVM?> ObtenerFormularioAsync(int id);
    Task<Resultado> CrearAsync(ProductoFormVM vm);

    // El SKU del ViewModel se ignora: es llave alterna y no se puede modificar
    Task<Resultado> ActualizarAsync(int id, ProductoFormVM vm);

    Task<Resultado> EliminarAsync(int id);
}
