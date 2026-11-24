namespace TiendaFluent.Entities;

// Jerarquía mapeada con TPH: una sola tabla Pagos con columna discriminadora TipoPago
public abstract class Pago
{
    public int Id { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
}

public class PagoTarjeta : Pago
{
    public string UltimosDigitos { get; set; } = null!;
    public string Marca { get; set; } = null!;
}

public class PagoTransferencia : Pago
{
    public string Banco { get; set; } = null!;
    public string Referencia { get; set; } = null!;
}
