namespace TiendaFluent.Entities;

public class PerfilCliente
{
    public int Id { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public int ClienteId { get; set; }                    // FK
    public Cliente Cliente { get; set; } = null!;
}
