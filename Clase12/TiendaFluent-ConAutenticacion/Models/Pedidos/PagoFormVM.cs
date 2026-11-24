using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

public class PagoFormVM
{
    public int PedidoId { get; set; }
    public int NumeroPedido { get; set; }
    public decimal Pendiente { get; set; }

    [Display(Name = "Tipo de pago")]
    public string Tipo { get; set; } = "Tarjeta";

    [Range(0.01, 9999999, ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    public string? Marca { get; set; }

    [Display(Name = "Últimos 4 dígitos")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Escribe exactamente 4 dígitos.")]
    public string? UltimosDigitos { get; set; }

    public string? Banco { get; set; }

    [Display(Name = "Referencia")]
    public string? Referencia { get; set; }
}
