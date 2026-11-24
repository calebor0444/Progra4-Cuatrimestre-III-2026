using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaFluent.Entities;
using TiendaFluent.Models;

namespace TiendaFluent.Controllers;

// Inicio de sesión, registro y cierre de sesión con ASP.NET Core Identity.
// UserManager administra los usuarios (crear, roles, contraseñas) y SignInManager la cookie de sesión.
public class CuentaController(UserManager<Usuario> usuarios, SignInManager<Usuario> sesion) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // lockoutOnFailure: true -> tras varios intentos fallidos la cuenta se bloquea unos minutos
        var resultado = await sesion.PasswordSignInAsync(vm.Email, vm.Password, vm.Recordarme, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            // Url.IsLocalUrl evita redirecciones abiertas hacia sitios externos
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return LocalRedirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, resultado.IsLockedOut
            ? "La cuenta está bloqueada (por intentos fallidos o por el administrador)."
            : "Correo o contraseña incorrectos.");
        return View(vm);
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var usuario = new Usuario { UserName = vm.Email, Email = vm.Email, NombreCompleto = vm.NombreCompleto };
        var resultado = await usuarios.CreateAsync(usuario, vm.Password);

        if (!resultado.Succeeded)
        {
            // Errores de Identity: correo repetido, contraseña débil, etc.
            foreach (var error in resultado.Errors.DistinctBy(e => e.Description))
                ModelState.AddModelError(string.Empty, error.Description);
            return View(vm);
        }

        // Quien se registra solo queda como Empleado; el Administrador se crea en DatosIdentidad
        await usuarios.AddToRoleAsync(usuario, Roles.Empleado);
        await sesion.SignInAsync(usuario, isPersistent: false);

        TempData["Ok"] = $"Bienvenido, {usuario.NombreCompleto}.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await sesion.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();
}
