using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Administración de usuarios con UserManager de Identity (no con TiendaContext directamente):
// UserManager se encarga del hash de la contraseña, la normalización del correo, los roles y el bloqueo.
public class UsuarioService(UserManager<Usuario> usuarios) : IUsuarioService
{
    public static readonly string[] RolesDisponibles = [Roles.Administrador, Roles.Empleado];

    public async Task<List<UsuarioListaVM>> ListarAsync()
    {
        var lista = await usuarios.Users.OrderBy(u => u.NombreCompleto).ToListAsync();
        var administradores = (await usuarios.GetUsersInRoleAsync(Roles.Administrador)).Select(u => u.Id).ToHashSet();

        return lista.Select(u => new UsuarioListaVM(
            u.Id,
            u.NombreCompleto,
            u.Email ?? "",
            administradores.Contains(u.Id) ? Roles.Administrador : Roles.Empleado,
            u.LockoutEnd > DateTimeOffset.UtcNow)).ToList();
    }

    public async Task<UsuarioFormVM?> ObtenerFormularioAsync(string id)
    {
        var u = await usuarios.FindByIdAsync(id);
        if (u is null) return null;
        return new UsuarioFormVM { Id = u.Id, NombreCompleto = u.NombreCompleto, Email = u.Email ?? "", Rol = await RolDe(u) };
    }

    public async Task<Resultado> CrearAsync(UsuarioFormVM vm)
    {
        if (!RolesDisponibles.Contains(vm.Rol)) return Resultado.Falla("Rol no válido.", nameof(vm.Rol));
        if (string.IsNullOrEmpty(vm.Password)) return Resultado.Falla("La contraseña es obligatoria.", nameof(vm.Password));

        var usuario = new Usuario
        {
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true,
            NombreCompleto = vm.NombreCompleto.Trim(),
        };
        var creado = await usuarios.CreateAsync(usuario, vm.Password);
        if (!creado.Succeeded) return Errores(creado);

        await usuarios.AddToRoleAsync(usuario, vm.Rol);
        return Resultado.Ok($"Usuario {usuario.Email} creado como {vm.Rol}.");
    }

    public async Task<Resultado> ActualizarAsync(string id, UsuarioFormVM vm, string idActual)
    {
        if (!RolesDisponibles.Contains(vm.Rol)) return Resultado.Falla("Rol no válido.", nameof(vm.Rol));

        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null) return Resultado.NoExiste();

        var rolActual = await RolDe(usuario);
        if (id == idActual && rolActual == Roles.Administrador && vm.Rol != Roles.Administrador)
            return Resultado.Falla("No puedes quitarte a ti mismo el rol de Administrador.", nameof(vm.Rol));

        // Datos básicos. UserName = Email porque el login es con el correo
        usuario.NombreCompleto = vm.NombreCompleto.Trim();
        usuario.Email = vm.Email;
        usuario.UserName = vm.Email;
        var actualizado = await usuarios.UpdateAsync(usuario);
        if (!actualizado.Succeeded) return Errores(actualizado);

        // Rol: se quita el anterior y se pone el nuevo
        if (rolActual != vm.Rol)
        {
            await usuarios.RemoveFromRolesAsync(usuario, await usuarios.GetRolesAsync(usuario));
            await usuarios.AddToRoleAsync(usuario, vm.Rol);
        }

        // Contraseña nueva (opcional): el administrador la restablece sin conocer la anterior
        if (!string.IsNullOrEmpty(vm.Password))
        {
            var token = await usuarios.GeneratePasswordResetTokenAsync(usuario);
            var cambio = await usuarios.ResetPasswordAsync(usuario, token, vm.Password);
            if (!cambio.Succeeded) return Errores(cambio);
        }

        // Cambiar el sello de seguridad invalida la cookie del usuario editado:
        // en su siguiente validación tendrá que volver a iniciar sesión con los permisos nuevos
        if (rolActual != vm.Rol && id != idActual)
            await usuarios.UpdateSecurityStampAsync(usuario);

        return Resultado.Ok($"Usuario {usuario.Email} actualizado.");
    }

    public async Task<Resultado> CambiarBloqueoAsync(string id, string idActual)
    {
        if (id == idActual) return Resultado.Falla("No puedes bloquear tu propia cuenta.");

        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null) return Resultado.NoExiste();

        var bloqueado = usuario.LockoutEnd > DateTimeOffset.UtcNow;
        // Bloquear = fecha de fin de bloqueo muy lejana; desbloquear = null
        await usuarios.SetLockoutEnabledAsync(usuario, true);
        await usuarios.SetLockoutEndDateAsync(usuario, bloqueado ? null : DateTimeOffset.MaxValue);
        if (bloqueado) await usuarios.ResetAccessFailedCountAsync(usuario);
        else await usuarios.UpdateSecurityStampAsync(usuario);   // cierra la sesión que tenga abierta

        return Resultado.Ok(bloqueado ? $"{usuario.Email} desbloqueado." : $"{usuario.Email} bloqueado: ya no puede iniciar sesión.");
    }

    public async Task<Resultado> EliminarAsync(string id, string idActual)
    {
        if (id == idActual) return Resultado.Falla("No puedes eliminar tu propia cuenta.");

        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null) return Resultado.NoExiste();

        // DeleteAsync también borra sus filas en AspNetUserRoles, AspNetUserClaims, etc. (cascada)
        var eliminado = await usuarios.DeleteAsync(usuario);
        return eliminado.Succeeded ? Resultado.Ok($"Usuario {usuario.Email} eliminado.") : Errores(eliminado);
    }

    private async Task<string> RolDe(Usuario u) =>
        await usuarios.IsInRoleAsync(u, Roles.Administrador) ? Roles.Administrador : Roles.Empleado;

    // Convierte los errores de Identity en errores por campo del formulario
    private static Resultado Errores(IdentityResult resultado) =>
        Resultado.Falla(resultado.Errors.Select(e => new ErrorNegocio(
            e.Code.StartsWith("Password") ? nameof(UsuarioFormVM.Password)
            : e.Code.Contains("Email") || e.Code.Contains("UserName") ? nameof(UsuarioFormVM.Email)
            : "",
            e.Description)).Distinct().ToList());
}
