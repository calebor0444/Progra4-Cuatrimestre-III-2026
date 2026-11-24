namespace TiendaFluent.Entities;

public class Etiqueta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public List<Producto> Productos { get; set; } = new();
}
