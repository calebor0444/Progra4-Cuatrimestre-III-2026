namespace TiendaFluent.Entities;

// Sin Id: pertenece a Cliente y se guarda en sus mismas columnas
public class Direccion
{
    public string Calle { get; set; } = null!;
    public string Ciudad { get; set; } = null!;
    public string? CodigoPostal { get; set; }
}
