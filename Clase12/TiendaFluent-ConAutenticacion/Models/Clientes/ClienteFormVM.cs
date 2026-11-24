using System.ComponentModel.DataAnnotations;

namespace TiendaFluent.Models;

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
