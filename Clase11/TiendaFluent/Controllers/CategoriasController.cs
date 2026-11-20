using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// Solo coordina la petición: la lógica y las consultas están en CategoriaService
public class CategoriasController(ICategoriaService categorias) : Controller
{
    public async Task<IActionResult> Index() => View(await categorias.ListarAsync());

    public async Task<IActionResult> Create()
    {
        await CargarPadres();
        return View("Form", new CategoriaFormVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaFormVM vm)
    {
        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        var resultado = await categorias.CrearAsync(vm);
        if (!resultado.Exito) return await MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vm = await categorias.ObtenerFormularioAsync(id);
        if (vm is null) return NotFound();
        await CargarPadres(id);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaFormVM vm)
    {
        if (!ModelState.IsValid) return await MostrarFormulario(vm, excluirId: id);

        var resultado = await categorias.ActualizarAsync(id, vm);
        if (resultado.NoEncontrado) return NotFound();
        if (!resultado.Exito) return await MostrarFormulario(vm, resultado, id);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await categorias.EliminarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> MostrarFormulario(CategoriaFormVM vm, Resultado? resultado = null, int? excluirId = null)
    {
        if (resultado is not null) ModelState.AgregarErrores(resultado);
        await CargarPadres(excluirId);
        return View("Form", vm);
    }

    private async Task CargarPadres(int? excluirId = null) =>
        ViewBag.Padres = new SelectList(await categorias.OpcionesPadreAsync(excluirId), nameof(Categoria.Id), nameof(Categoria.Nombre));
}
