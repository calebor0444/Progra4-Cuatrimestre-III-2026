using TiendaFluent.Entities;

namespace TiendaFluent.Models;

public class ClienteDetalleVM
{
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
}
