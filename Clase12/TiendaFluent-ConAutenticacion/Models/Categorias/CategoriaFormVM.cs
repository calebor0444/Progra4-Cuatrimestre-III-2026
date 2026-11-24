using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

public class CategoriaFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = "";

    [Display(Name = "Categoría padre")]
    public int? CategoriaPadreId { get; set; }
}
