using Microsoft.AspNetCore.Identity;

namespace TiendaFluent.Services;

// Identity trae sus mensajes en inglés. Esta clase los traduce; se registra en Program.cs
// con .AddErrorDescriber<ErroresIdentidadEspanol>(). Como el UserName es el correo, los
// mensajes de usuario y correo duplicado dicen lo mismo (y se muestran una sola vez).
public class ErroresIdentidadEspanol : IdentityErrorDescriber
{
    private static IdentityError E(string code, string mensaje) => new() { Code = code, Description = mensaje };

    public override IdentityError DuplicateUserName(string userName) => E(nameof(DuplicateUserName), $"El correo '{userName}' ya está registrado.");
    public override IdentityError DuplicateEmail(string email) => E(nameof(DuplicateEmail), $"El correo '{email}' ya está registrado.");
    public override IdentityError InvalidUserName(string? userName) => E(nameof(InvalidUserName), $"El usuario '{userName}' no es válido.");
    public override IdentityError InvalidEmail(string? email) => E(nameof(InvalidEmail), $"El correo '{email}' no es válido.");
    public override IdentityError DuplicateRoleName(string role) => E(nameof(DuplicateRoleName), $"El rol '{role}' ya existe.");
    public override IdentityError InvalidRoleName(string? role) => E(nameof(InvalidRoleName), $"El rol '{role}' no es válido.");
    public override IdentityError UserAlreadyInRole(string role) => E(nameof(UserAlreadyInRole), $"El usuario ya tiene el rol '{role}'.");
    public override IdentityError UserNotInRole(string role) => E(nameof(UserNotInRole), $"El usuario no tiene el rol '{role}'.");
    public override IdentityError PasswordMismatch() => E(nameof(PasswordMismatch), "La contraseña es incorrecta.");
    public override IdentityError InvalidToken() => E(nameof(InvalidToken), "El token no es válido o ya expiró.");
    public override IdentityError ConcurrencyFailure() => E(nameof(ConcurrencyFailure), "Otro usuario modificó este registro. Vuelve a intentarlo.");
    public override IdentityError DefaultError() => E(nameof(DefaultError), "Ocurrió un error desconocido.");
    public override IdentityError PasswordTooShort(int length) => E(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => E(nameof(PasswordRequiresUniqueChars), $"La contraseña debe tener al menos {uniqueChars} caracteres distintos.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => E(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe tener al menos un símbolo (por ejemplo ! o #).");
    public override IdentityError PasswordRequiresDigit() => E(nameof(PasswordRequiresDigit), "La contraseña debe tener al menos un número.");
    public override IdentityError PasswordRequiresLower() => E(nameof(PasswordRequiresLower), "La contraseña debe tener al menos una minúscula.");
    public override IdentityError PasswordRequiresUpper() => E(nameof(PasswordRequiresUpper), "La contraseña debe tener al menos una mayúscula.");
}
