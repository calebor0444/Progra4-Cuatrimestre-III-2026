namespace TiendaFluent.Entities;

// Nombres de los roles en un solo lugar para no repetir cadenas en [Authorize] ni en las vistas
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";
}
