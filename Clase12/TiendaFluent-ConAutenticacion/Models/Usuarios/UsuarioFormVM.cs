using System.ComponentModel.DataAnnotations;
using TiendaFluent.Entities;

namespace TiendaFluent.Models;

// Mismo formulario para crear y editar. En la edición la contraseña es opcional:
// si se deja vacía, se conserva la actual.
public class UsuarioFormVM
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [StringLength(256)]
    [Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Selecciona un rol.")]
    public string Rol { get; set; } = Roles.Empleado;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarPassword { get; set; }
}
