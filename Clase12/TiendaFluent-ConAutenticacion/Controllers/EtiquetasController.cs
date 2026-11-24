using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaFluent.Entities;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// Solo coordina la petición: la lógica y las consultas están en EtiquetaService
// Requiere haber iniciado sesión (cualquier rol)
[Authorize]
public class EtiquetasController(IEtiquetaService etiquetas) : Controller
{
    public async Task<IActionResult> Index() => View(await etiquetas.ListarAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string nombre)
    {
        this.Notificar(await etiquetas.CrearAsync(nombre));
        return RedirectToAction(nameof(Index));
    }

    // Solo el administrador puede eliminar
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await etiquetas.EliminarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Index));
    }
}
