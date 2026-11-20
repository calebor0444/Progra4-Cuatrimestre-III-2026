using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

public class ResumenService(TiendaContext db) : IResumenService
{
    // Los conteos ya respetan los filtros globales: no cuentan clientes eliminados ni sus pedidos
    public async Task<ResumenVM> ObtenerAsync() => new(
        await db.Categorias.CountAsync(),
        await db.Productos.CountAsync(),
        await db.Etiquetas.CountAsync(),
        await db.Clientes.CountAsync(),
        await db.Pedidos.CountAsync(),
        await db.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0);
}
