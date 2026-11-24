using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// Solo coordina la petición: la lógica y las consultas están en ClienteService
// Requiere haber iniciado sesión (cualquier rol)
[Authorize]
public class ClientesController(IClienteService clientes) : Controller
{
    public async Task<IActionResult> Index(bool verEliminados = false)
    {
        ViewBag.VerEliminados = verEliminados;
        return View(await clientes.ListarAsync(verEliminados));
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await clientes.ObtenerDetalleAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    public IActionResult Create() => View("Form", new ClienteFormVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormVM vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        var resultado = await clientes.CrearAsync(vm);
        if (!resultado.Exito)
        {
            ModelState.AgregarErrores(resultado);
            return View("Form", vm);
        }

        this.Notificar(resultado);
        return RedirectToAction(nameof(Details), new { id = resultado.Valor });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vm = await clientes.ObtenerFormularioAsync(id);
        return vm is null ? NotFound() : View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteFormVM vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        var resultado = await clientes.ActualizarAsync(id, vm);
        if (resultado.NoEncontrado) return NotFound();
        if (!resultado.Exito)
        {
            ModelState.AgregarErrores(resultado);
            return View("Form", vm);
        }

        this.Notificar(resultado);
        return RedirectToAction(nameof(Details), new { id });
    }

    // Solo el administrador puede eliminar
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await clientes.EliminarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restaurar(int id)
    {
        var resultado = await clientes.RestaurarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index), new { verEliminados = true });
    }
}
