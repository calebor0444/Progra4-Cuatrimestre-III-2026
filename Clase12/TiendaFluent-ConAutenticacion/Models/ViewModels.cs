using System.ComponentModel.DataAnnotations;
using TiendaFluent.Entities;

namespace TiendaFluent.Models;

// Los ViewModels llevan validaciones de la interfaz (Data Annotations).
// La definición de la base de datos vive solo en la Fluent API (Data/Configurations).

public record CategoriaListaVM(int Id, string Nombre, string? Padre, int Productos, int Subcategorias);

public class CategoriaFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = "";

    [Display(Name = "Categoría padre")]
    public int? CategoriaPadreId { get; set; }
}

public class ProductoFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(20, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = "";

    [Range(0, 9999999, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    public List<int> EtiquetasSeleccionadas { get; set; } = new();
}

public class ClienteFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [StringLength(256)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(200)]
    public string Calle { get; set; } = "";

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100)]
    public string Ciudad { get; set; } = "";

    [StringLength(10)]
    [Display(Name = "Código postal")]
    public string? CodigoPostal { get; set; }

    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? FechaNacimiento { get; set; }
}

public class LineaPedidoVM
{
    public int? ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;
}

public class PedidoCrearVM
{
    [Required(ErrorMessage = "Selecciona un cliente.")]
    [Display(Name = "Cliente")]
    public int? ClienteId { get; set; }

    public List<LineaPedidoVM> Lineas { get; set; } = new();
}

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

public class PedidoDetalleVM
{
    public Pedido Pedido { get; set; } = null!;
    public List<Pago> Pagos { get; set; } = new();
    public decimal Pagado => Pagos.Sum(p => p.Monto);
}

public class ClienteDetalleVM
{
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
}

public record ResumenVM(int Categorias, int Productos, int Etiquetas, int Clientes, int Pedidos, decimal Ventas);
