using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Demuestra: secuencia para Numero (guía 6.4), valores por defecto (7.4), enum como texto (7.6),
// columna calculada Subtotal (7.5), llave compuesta en DetallesPedido (6.2), concurrencia con
// RowVersion (7.7), herencia TPH en Pagos (12.1) y Cascade vs Restrict al borrar (10)
public class PedidoService(TiendaContext db) : IPedidoService
{
    public Task<List<Pedido>> ListarAsync() =>
        db.Pedidos.Include(p => p.Cliente).OrderByDescending(p => p.Numero).ToListAsync();

    public async Task<PedidoDetalleVM?> ObtenerDetalleAsync(int id)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null) return null;

        // Consulta polimórfica: EF materializa PagoTarjeta o PagoTransferencia según TipoPago
        var pagos = await db.Pagos.Where(p => p.PedidoId == id).OrderBy(p => p.Fecha).ToListAsync();
        return new PedidoDetalleVM { Pedido = pedido, Pagos = pagos };
    }

    public async Task<CatalogosPedido> CatalogosAsync() => new(
        await db.Clientes.OrderBy(c => c.Nombre).ToListAsync(),
        await db.Productos.Where(p => p.Stock > 0).OrderBy(p => p.Nombre).ToListAsync());

    public async Task<Resultado<int>> CrearAsync(PedidoCrearVM vm)
    {
        var errores = new List<ErrorNegocio>();
        if (vm.ClienteId is null)
            errores.Add(new(nameof(PedidoCrearVM.ClienteId), "Selecciona un cliente."));

        // La PK de DetallesPedido es (PedidoId, ProductoId): un producto solo puede ir una vez,
        // así que las líneas repetidas se agrupan sumando cantidades
        var lineas = vm.Lineas
            .Where(l => l.ProductoId is not null && l.Cantidad > 0)
            .GroupBy(l => l.ProductoId!.Value)
            .Select(g => (ProductoId: g.Key, Cantidad: g.Sum(x => x.Cantidad)))
            .ToList();

        if (lineas.Count == 0)
            errores.Add(new("", "Agrega al menos un producto con cantidad mayor que cero."));

        var ids = lineas.Select(l => l.ProductoId).ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        foreach (var l in lineas)
        {
            if (!productos.TryGetValue(l.ProductoId, out var p))
                errores.Add(new("", "Uno de los productos elegidos ya no existe."));
            else if (p.Stock < l.Cantidad)
                errores.Add(new("", $"Stock insuficiente de \"{p.Nombre}\" (disponible: {p.Stock})."));
        }

        if (errores.Count > 0) return Resultado<int>.Falla(errores);

        var pedido = new Pedido
        {
            ClienteId = vm.ClienteId!.Value,
            Estado = EstadoPedido.Pendiente,
            // Numero y Fecha no se asignan: los generan la secuencia y GETUTCDATE() en SQL Server
        };
        foreach (var l in lineas)
        {
            var p = productos[l.ProductoId];
            pedido.Detalles.Add(new DetallePedido { ProductoId = p.Id, Cantidad = l.Cantidad, PrecioUnitario = p.Precio });
            p.Stock -= l.Cantidad;   // se guarda en la misma transacción que el pedido
        }
        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();   // tras guardar, EF ya trae Numero, Fecha, Subtotal y RowVersion

        return Resultado<int>.Ok(pedido.Id, $"Pedido #{pedido.Numero} creado.");
    }

    public async Task<Resultado> CambiarEstadoAsync(int id, EstadoPedido estado, string rowVersion)
    {
        var pedido = await db.Pedidos.FindAsync(id);
        if (pedido is null) return Resultado.NoExiste();

        byte[] versionEnPantalla;
        try { versionEnPantalla = Convert.FromBase64String(rowVersion); }
        catch (FormatException) { return Resultado.Falla("La versión del pedido no es válida; recarga la página."); }

        // Se compara contra la versión que el usuario tenía en pantalla
        db.Entry(pedido).Property(p => p.RowVersion).OriginalValue = versionEnPantalla;
        pedido.Estado = estado;
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Estado cambiado a {estado}.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Resultado.Falla("Otro usuario modificó este pedido mientras lo veías. Se recargaron los datos; vuelve a intentarlo.");
        }
    }

    public async Task<PagoFormVM?> ObtenerFormularioPagoAsync(int pedidoId)
    {
        var pedido = await db.Pedidos.FindAsync(pedidoId);
        if (pedido is null) return null;

        var pendiente = Math.Max(0, pedido.Total - await TotalPagadoAsync(pedidoId));
        return new PagoFormVM { PedidoId = pedido.Id, NumeroPedido = pedido.Numero, Pendiente = pendiente, Monto = pendiente };
    }

    public async Task<Resultado> RegistrarPagoAsync(PagoFormVM vm)
    {
        var pedido = await db.Pedidos.FindAsync(vm.PedidoId);
        if (pedido is null) return Resultado.NoExiste();

        var pagado = await TotalPagadoAsync(vm.PedidoId);
        var errores = ValidarPago(vm, pendiente: pedido.Total - pagado);
        if (errores.Count > 0) return Resultado.Falla(errores);

        // Herencia TPH: se agrega el subtipo y EF escribe el discriminador TipoPago automáticamente
        Pago pago = vm.Tipo == "Tarjeta"
            ? new PagoTarjeta { Marca = vm.Marca!.Trim(), UltimosDigitos = vm.UltimosDigitos! }
            : new PagoTransferencia { Banco = vm.Banco!.Trim(), Referencia = vm.Referencia!.Trim() };
        pago.PedidoId = vm.PedidoId;
        pago.Monto = vm.Monto;
        pago.Fecha = DateTime.UtcNow;
        db.Pagos.Add(pago);

        // Si con este pago se completa el total, el pedido pasa a Pagado
        if (pagado + vm.Monto >= pedido.Total && pedido.Estado == EstadoPedido.Pendiente)
            pedido.Estado = EstadoPedido.Pagado;

        await db.SaveChangesAsync();
        return Resultado.Ok("Pago registrado.");
    }

    public async Task<Resultado> EliminarAsync(int id)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null) return Resultado.NoExiste();

        // Devolver el stock; todo va en la misma transacción que el DELETE
        foreach (var d in pedido.Detalles) d.Producto.Stock += d.Cantidad;

        // DetallesPedido -> Cascade: se borran con el pedido
        // Pagos -> Restrict: si el pedido tiene pagos, SQL Server rechaza el borrado
        db.Pedidos.Remove(pedido);
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Pedido #{pedido.Numero} eliminado junto con sus líneas (Cascade).");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsLlaveForanea(ex))
        {
            return Resultado.Falla($"No se puede eliminar el pedido #{pedido.Numero}: tiene pagos registrados (Pagos usa OnDelete Restrict).");
        }
    }

    private async Task<decimal> TotalPagadoAsync(int pedidoId) =>
        await db.Pagos.Where(p => p.PedidoId == pedidoId).SumAsync(p => (decimal?)p.Monto) ?? 0;

    // Cada subtipo de Pago exige sus propios datos
    private static List<ErrorNegocio> ValidarPago(PagoFormVM vm, decimal pendiente)
    {
        var errores = new List<ErrorNegocio>();

        if (pendiente <= 0)
            errores.Add(new("", "Este pedido ya está pagado por completo."));
        else if (vm.Monto > pendiente)
            errores.Add(new(nameof(PagoFormVM.Monto), $"El monto no puede ser mayor que lo pendiente ({pendiente:C2})."));

        switch (vm.Tipo)
        {
            case "Tarjeta":
                if (string.IsNullOrWhiteSpace(vm.Marca)) errores.Add(new(nameof(PagoFormVM.Marca), "Indica la marca de la tarjeta."));
                if (string.IsNullOrWhiteSpace(vm.UltimosDigitos)) errores.Add(new(nameof(PagoFormVM.UltimosDigitos), "Indica los últimos 4 dígitos."));
                break;
            case "Transferencia":
                if (string.IsNullOrWhiteSpace(vm.Banco)) errores.Add(new(nameof(PagoFormVM.Banco), "Indica el banco."));
                if (string.IsNullOrWhiteSpace(vm.Referencia)) errores.Add(new(nameof(PagoFormVM.Referencia), "Indica la referencia."));
                break;
            default:
                errores.Add(new(nameof(PagoFormVM.Tipo), "Tipo de pago no válido."));
                break;
        }
        return errores;
    }
}
