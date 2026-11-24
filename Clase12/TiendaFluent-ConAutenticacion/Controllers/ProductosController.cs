using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// Solo coordina la petición: la lógica y las consultas están en ProductoService
// Requiere haber iniciado sesión (cualquier rol)
[Authorize]
public class ProductosController(IProductoService productos) : Controller
{
    public async Task<IActionResult> Index() => View(await productos.ListarAsync());

    public Task<IActionResult> Create() => MostrarFormulario(new ProductoFormVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoFormVM vm)
    {
        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        var resultado = await productos.CrearAsync(vm);
        if (!resultado.Exito) return await MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vm = await productos.ObtenerFormularioAsync(id);
        return vm is null ? NotFound() : await MostrarFormulario(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoFormVM vm)
    {
        ModelState.Remove(nameof(vm.Sku));   // el SKU no se edita (el servicio lo ignora)
        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        var resultado = await productos.ActualizarAsync(id, vm);
        if (resultado.NoEncontrado) return NotFound();
        if (!resultado.Exito) return await MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    // Solo el administrador puede eliminar
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await productos.EliminarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> MostrarFormulario(ProductoFormVM vm, Resultado? resultado = null)
    {
        if (resultado is not null) ModelState.AgregarErrores(resultado);

        var catalogos = await productos.CatalogosAsync();
        ViewBag.Categorias = new SelectList(catalogos.Categorias, nameof(Categoria.Id), nameof(Categoria.Nombre));
        ViewBag.Etiquetas = catalogos.Etiquetas;
        return View("Form", vm);
    }
}
