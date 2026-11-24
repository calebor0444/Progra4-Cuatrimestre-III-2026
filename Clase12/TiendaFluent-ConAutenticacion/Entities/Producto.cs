namespace TiendaFluent.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;              // llave alterna
    public string Nombre { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public List<Etiqueta> Etiquetas { get; set; } = new();   // N:N
}
