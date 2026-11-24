using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Controllers;

// Solo coordina la petición: la lógica y las consultas están en PedidoService
// Requiere haber iniciado sesión (cualquier rol)
[Authorize]
public class PedidosController(IPedidoService pedidos) : Controller
{
    private const int LineasEnFormulario = 4;

    public async Task<IActionResult> Index() => View(await pedidos.ListarAsync());

    public async Task<IActionResult> Details(int id)
    {
        var vm = await pedidos.ObtenerDetalleAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    public Task<IActionResult> Create() =>
        MostrarFormulario(new PedidoCrearVM
        {
            Lineas = Enumerable.Range(0, LineasEnFormulario).Select(_ => new LineaPedidoVM()).ToList(),
        });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoCrearVM vm)
    {
        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        var resultado = await pedidos.CrearAsync(vm);
        if (!resultado.Exito) return await MostrarFormulario(vm, resultado);

        this.Notificar(resultado);
        return RedirectToAction(nameof(Details), new { id = resultado.Valor });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoPedido estado, string rowVersion)
    {
        var resultado = await pedidos.CambiarEstadoAsync(id, estado, rowVersion);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> RegistrarPago(int id)
    {
        var vm = await pedidos.ObtenerFormularioPagoAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPago(PagoFormVM vm)
    {
        if (ModelState.IsValid)
        {
            var resultado = await pedidos.RegistrarPagoAsync(vm);
            if (resultado.NoEncontrado) return NotFound();
            if (resultado.Exito)
            {
                this.Notificar(resultado);
                return RedirectToAction(nameof(Details), new { id = vm.PedidoId });
            }
            ModelState.AgregarErrores(resultado);
        }

        // Volver a mostrar el formulario con el número y el pendiente actuales del pedido
        var datos = await pedidos.ObtenerFormularioPagoAsync(vm.PedidoId);
        if (datos is null) return NotFound();
        vm.NumeroPedido = datos.NumeroPedido;
        vm.Pendiente = datos.Pendiente;
        return View(vm);
    }

    // Solo el administrador puede eliminar
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await pedidos.EliminarAsync(id);
        if (resultado.NoEncontrado) return NotFound();

        this.Notificar(resultado);
        return resultado.Exito
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IActionResult> MostrarFormulario(PedidoCrearVM vm, Resultado? resultado = null)
    {
        if (resultado is not null) ModelState.AgregarErrores(resultado);

        var catalogos = await pedidos.CatalogosAsync();
        ViewBag.Clientes = new SelectList(catalogos.Clientes, nameof(Cliente.Id), nameof(Cliente.Nombre));
        ViewBag.Productos = catalogos.ProductosConStock;
        return View(vm);
    }
}
