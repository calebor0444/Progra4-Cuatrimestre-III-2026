using TiendaFluent.Entities;

namespace TiendaFluent.Models;

// Lo que recibe la vista Index: los filtros usados, los resultados y las listas para los <select>
public class ProductoListadoVM
{
    public ProductoFiltroVM Filtro { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public List<Etiqueta> Etiquetas { get; set; } = new();
}
