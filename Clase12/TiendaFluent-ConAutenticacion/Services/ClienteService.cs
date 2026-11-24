using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Models;
using TiendaFluent.Services.Interfaces;

namespace TiendaFluent.Services;

// Demuestra: tipo propio Direccion (guía 11), relación 1:1 con PerfilCliente (guía 9.4),
// propiedad sombra FechaCreacion (guía 7.8), filtro global de borrado lógico (guía 14)
// e índice único filtrado sobre Email (guía 8.1)
public class ClienteService(TiendaContext db) : IClienteService
{
    private const string ErrorCorreoDuplicado = "Ya existe un cliente activo con ese correo (índice único IX_Clientes_Email).";

    public Task<List<Cliente>> ListarAsync(bool incluirEliminados)
    {
        var query = db.Clientes.Include(c => c.Perfil).AsQueryable();
        if (incluirEliminados) query = query.IgnoreQueryFilters();   // salta HasQueryFilter(c => !c.Eliminado)
        return query.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<ClienteDetalleVM?> ObtenerDetalleAsync(int id)
    {
        var cliente = await db.Clientes
            .Include(c => c.Perfil)
            .Include(c => c.Pedidos.OrderByDescending(p => p.Fecha))
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return null;

        // La propiedad sombra no existe en la clase: se lee desde el Entry
        var fecha = db.Entry(cliente).Property<DateTime>("FechaCreacion").CurrentValue;
        return new ClienteDetalleVM { Cliente = cliente, FechaCreacion = fecha };
    }

    public async Task<ClienteFormVM?> ObtenerFormularioAsync(int id)
    {
        var c = await db.Clientes.Include(x => x.Perfil).FirstOrDefaultAsync(x => x.Id == id);
        return c is null ? null : new ClienteFormVM
        {
            Id = c.Id, Nombre = c.Nombre, Email = c.Email,
            Calle = c.Direccion.Calle, Ciudad = c.Direccion.Ciudad, CodigoPostal = c.Direccion.CodigoPostal,
            Telefono = c.Perfil?.Telefono, FechaNacimiento = c.Perfil?.FechaNacimiento,
        };
    }

    public async Task<Resultado<int>> CrearAsync(ClienteFormVM vm)
    {
        var cliente = new Cliente
        {
            Nombre = vm.Nombre.Trim(),
            Email = NormalizarCorreo(vm.Email),
            Direccion = new Direccion { Calle = vm.Calle, Ciudad = vm.Ciudad, CodigoPostal = vm.CodigoPostal },
            Perfil = TienePerfil(vm) ? new PerfilCliente { Telefono = vm.Telefono, FechaNacimiento = vm.FechaNacimiento } : null,
        };
        db.Clientes.Add(cliente);
        try
        {
            await db.SaveChangesAsync();
            return Resultado<int>.Ok(cliente.Id, "Cliente creado.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsDuplicado(ex))
        {
            return Resultado<int>.Falla(ErrorCorreoDuplicado, nameof(ClienteFormVM.Email));
        }
    }

    public async Task<Resultado> ActualizarAsync(int id, ClienteFormVM vm)
    {
        var c = await db.Clientes.Include(x => x.Perfil).FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return Resultado.NoExiste();

        c.Nombre = vm.Nombre.Trim();
        c.Email = NormalizarCorreo(vm.Email);

        // Tipo propio: se modifican sus propiedades como si fueran columnas del cliente
        c.Direccion.Calle = vm.Calle;
        c.Direccion.Ciudad = vm.Ciudad;
        c.Direccion.CodigoPostal = vm.CodigoPostal;

        // 1:1 opcional: crear, actualizar o quitar el perfil
        if (TienePerfil(vm))
        {
            c.Perfil ??= new PerfilCliente();
            c.Perfil.Telefono = vm.Telefono;
            c.Perfil.FechaNacimiento = vm.FechaNacimiento;
        }
        else if (c.Perfil is not null)
        {
            db.PerfilesCliente.Remove(c.Perfil);
        }

        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok("Cliente actualizado.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsDuplicado(ex))
        {
            return Resultado.Falla(ErrorCorreoDuplicado, nameof(ClienteFormVM.Email));
        }
    }

    // Borrado lógico: no se elimina la fila, se marca y el filtro global la oculta
    public async Task<Resultado> EliminarAsync(int id)
    {
        var c = await db.Clientes.FindAsync(id);
        if (c is null) return Resultado.NoExiste();

        c.Eliminado = true;
        await db.SaveChangesAsync();
        return Resultado.Ok($"Cliente \"{c.Nombre}\" eliminado (borrado lógico). Sus pedidos también dejan de aparecer.");
    }

    public async Task<Resultado> RestaurarAsync(int id)
    {
        var c = await db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return Resultado.NoExiste();

        c.Eliminado = false;
        try
        {
            await db.SaveChangesAsync();
            return Resultado.Ok($"Cliente \"{c.Nombre}\" restaurado.");
        }
        catch (DbUpdateException ex) when (ErroresBd.EsDuplicado(ex))
        {
            return Resultado.Falla($"No se puede restaurar: otro cliente activo ya usa {c.Email} (índice único filtrado).");
        }
    }

    private static string NormalizarCorreo(string email) => email.Trim().ToLowerInvariant();

    private static bool TienePerfil(ClienteFormVM vm) =>
        !string.IsNullOrWhiteSpace(vm.Telefono) || vm.FechaNacimiento is not null;
}
