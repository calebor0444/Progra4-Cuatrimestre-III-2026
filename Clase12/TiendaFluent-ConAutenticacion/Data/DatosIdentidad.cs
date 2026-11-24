using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaFluent.Entities;

namespace TiendaFluent.Data;

// Crea los roles y un usuario administrador al iniciar la aplicación.
// Se usa UserManager/RoleManager (y no HasData) porque Identity debe calcular el hash de la contraseña.
public static class DatosIdentidad
{
    // Credenciales de prueba del administrador (solo para el ejemplo de clase)
    public const string AdminEmail = "admin@tienda.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SembrarAsync(IServiceProvider servicios)
    {
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TiendaContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TiendaContext>>();

        // Si la base no existe o faltan migraciones no se siembra (primero: dotnet ef database update)
        if (!await db.Database.CanConnectAsync() || (await db.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogWarning("La base de datos no está al día. Ejecuta 'dotnet tool run dotnet-ef database update'.");
            return;
        }

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { Roles.Administrador, Roles.Empleado })
        {
            if (!await roles.RoleExistsAsync(rol))
                await roles.CreateAsync(new IdentityRole(rol));
        }

        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        if (await usuarios.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new Usuario
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                NombreCompleto = "Administrador de la tienda",
            };
            var resultado = await usuarios.CreateAsync(admin, AdminPassword);
            if (resultado.Succeeded)
                await usuarios.AddToRoleAsync(admin, Roles.Administrador);
            else
                logger.LogError("No se pudo crear el administrador: {Errores}",
                    string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }
    }
}
