using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

// Criterios de búsqueda de la pantalla de productos.
// Todos son opcionales (nullable): si vienen vacíos, ese filtro no se aplica.
// Llegan por la URL (?Texto=cafe&PrecioMax=10) porque el formulario usa method="get".
public class ProductoFiltroVM : IValidatableObject
{
    [Display(Name = "Nombre o SKU")]
    public string? Texto { get; set; }

    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [Display(Name = "Etiqueta")]
    public int? EtiquetaId { get; set; }

    [Range(0, 9999999, ErrorMessage = "El precio mínimo no puede ser negativo.")]
    [Display(Name = "Precio mínimo")]
    public decimal? PrecioMin { get; set; }

    [Range(0, 9999999, ErrorMessage = "El precio máximo no puede ser negativo.")]
    [Display(Name = "Precio máximo")]
    public decimal? PrecioMax { get; set; }

    [Display(Name = "Solo con stock")]
    public bool SoloConStock { get; set; }

    // "nombre" (por defecto), "precio_asc" o "precio_desc"
    public string? Orden { get; set; }

    // Validación que involucra dos campos a la vez
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (PrecioMin is not null && PrecioMax is not null && PrecioMin > PrecioMax)
            yield return new ValidationResult("El precio mínimo no puede ser mayor que el máximo.", [nameof(PrecioMin)]);
    }
}
