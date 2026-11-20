namespace TiendaFluent.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int? CategoriaPadreId { get; set; }            // anulable: hay categorías raíz
    public Categoria? CategoriaPadre { get; set; }
    public List<Categoria> Subcategorias { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
}
