namespace TiendaFluent.Entities;

// Tabla intermedia con datos propios entre Pedido y Producto (N:N con payload)
public class DetallePedido
{
    public int PedidoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }                 // columna calculada en SQL Server
    public Pedido Pedido { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
