using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

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
