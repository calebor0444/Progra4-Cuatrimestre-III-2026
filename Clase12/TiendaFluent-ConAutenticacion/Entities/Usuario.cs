using Microsoft.AspNetCore.Identity;

namespace TiendaFluent.Entities;

// Usuario de la aplicación. Hereda de IdentityUser (Id, UserName, Email, PasswordHash, ...)
// y agrega los datos propios que necesita la tienda.
public class Usuario : IdentityUser
{
    public string NombreCompleto { get; set; } = "";
}
