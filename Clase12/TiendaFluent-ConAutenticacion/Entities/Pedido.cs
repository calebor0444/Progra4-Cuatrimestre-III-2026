namespace TiendaFluent.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int Numero { get; set; }                       // lo asigna la secuencia NumerosPedido
    public DateTime Fecha { get; set; }                   // valor por defecto GETUTCDATE()
    public EstadoPedido Estado { get; set; }              // se guarda como texto
    public decimal Total { get; set; }
    public byte[] RowVersion { get; set; } = null!;       // concurrencia optimista
    public int ClienteId { get; set; }                    // FK
    public Cliente Cliente { get; set; } = null!;
    public List<DetallePedido> Detalles { get; set; } = new();
}
