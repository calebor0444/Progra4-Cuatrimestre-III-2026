using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

public class PedidoCrearVM
{
    [Required(ErrorMessage = "Selecciona un cliente.")]
    [Display(Name = "Cliente")]
    public int? ClienteId { get; set; }

    public List<LineaPedidoVM> Lineas { get; set; } = new();
}
