using TiendaFluent.Entities;

namespace TiendaFluent.Models;

public class PedidoDetalleVM
{
    public Pedido Pedido { get; set; } = null!;
    public List<Pago> Pagos { get; set; } = new();
    public decimal Pagado => Pagos.Sum(p => p.Monto);
}
