using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// [Authorize] a nivel de clase: TODAS las acciones exigen el rol Administrador.
// Un Empleado que entre a /Usuarios es enviado a /Cuenta/AccesoDenegado.
[Authorize(Roles = Roles.Administrador)]
public class UsuariosController(IUsuarioService servicio, UserManager<Usuario> usuarios) : Controller
{
    // Id del administrador que está usando la pantalla (sale de la cookie)
    private string IdActual => usuarios.GetUserId(User)!;

    public async Task<IActionResult> Index()
    {
        ViewBag.IdActual = IdActual;
        return View(await servicio.ListarAsync());
    }

    public IActionResult Create() => MostrarFormulario(new UsuarioFormVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormVM vm)
    {
        if (!ModelState.IsValid) return MostrarFormulario(vm);

        var resultado = await servicio.CrearAsync(vm);
        if (!resultado.Exito) return MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var vm = await servicio.ObtenerFormularioAsync(id);
        return vm is null ? NotFound() : MostrarFormulario(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UsuarioFormVM vm)
    {
        if (!ModelState.IsValid) return MostrarFormulario(vm);

        var resultado = await servicio.ActualizarAsync(id, vm, IdActual);
        if (resultado.NoEncontrado) return NotFound();
        if (!resultado.Exito) return MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarBloqueo(string id)
    {
        var resultado = await servicio.CambiarBloqueoAsync(id, IdActual);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var resultado = await servicio.EliminarAsync(id, IdActual);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    private ViewResult MostrarFormulario(UsuarioFormVM vm, Resultado? resultado = null)
    {
        if (resultado is not null) ModelState.AgregarErrores(resultado);
        ViewBag.Roles = UsuarioService.RolesDisponibles;
        return View("Form", vm);
    }
}
