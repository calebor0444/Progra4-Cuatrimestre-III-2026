# TiendaFluent

Proyecto ASP.NET Core MVC (.NET 10 + EF Core 10) que implementa el modelo de ejemplo de la guía
**"Entity Framework Core – Fluent API para diseñar bases de datos con Code First"**.

## Requisitos

- SDK de .NET 10 o superior (el proyecto apunta a `net10.0`)
- SQL Server LocalDB (viene con Visual Studio). Para usar SQL Express cambia la cadena en
  `appsettings.json` por `Server=.\\SQLEXPRESS;Database=TiendaFluentDb;Trusted_Connection=True;TrustServerCertificate=True`

## Puesta en marcha

```bash
dotnet tool restore            # instala dotnet-ef 10 (definido en dotnet-tools.json)
dotnet tool run dotnet-ef database update   # crea TiendaFluentAuthDb y carga los datos de ejemplo
dotnet run                     # http://localhost:5177
```

Desde Visual Studio: abre `TiendaFluent.csproj` y, en la Consola del Administrador de Paquetes, ejecuta `Update-Database`.

Para volver a empezar con datos limpios:

```bash
dotnet tool run dotnet-ef database drop --force
dotnet tool run dotnet-ef database update
```

## Autenticación con ASP.NET Core Identity

Esta versión usa su propia base (`TiendaFluentAuthDb`) para no chocar con la de Clase11.

- **Usuario de prueba**: `admin@tienda.com` / `Admin123!` (rol **Administrador**). Se crea solo al
  iniciar la app (`Data/DatosIdentidad.cs`), junto con los roles `Administrador` y `Empleado`.
- **Registro**: quien se registra en *Registrarse* queda con el rol **Empleado**.
- **Permisos**: Inicio es público; Productos, Categorías, Etiquetas, Clientes y Pedidos piden sesión
  (`[Authorize]`); las acciones `Delete` son solo para `[Authorize(Roles = Roles.Administrador)]`.
  Los botones *Eliminar* se ocultan con `User.IsInRole(...)` para los empleados.

| Archivo | Contenido |
|---|---|
| `Entities/Usuario.cs` | `Usuario : IdentityUser` con `NombreCompleto` |
| `Entities/Roles.cs` | Constantes con los nombres de los roles |
| `Data/TiendaContext.cs` | Hereda de `IdentityDbContext<Usuario>` y llama a `base.OnModelCreating` |
| `Data/Configurations/UsuarioConfiguration.cs` | Fluent API de la columna extra `NombreCompleto` |
| `Data/DatosIdentidad.cs` | Siembra roles y administrador con `RoleManager` / `UserManager` |
| `Program.cs` | `AddIdentity`, `ConfigureApplicationCookie`, `UseAuthentication` antes de `UseAuthorization` |
| `Controllers/CuentaController.cs` | Login, Registro, Logout y Acceso denegado (`SignInManager`, `UserManager`) |
| `Views/Cuenta/`, `Views/Shared/_LoginPartial.cshtml` | Formularios y menú de la cuenta |
| `Migrations/…_Identidad.cs` | Migración que crea las tablas `AspNet…` |
| `Controllers/UsuariosController.cs` | Administración de usuarios, solo `[Authorize(Roles = Roles.Administrador)]` |
| `Services/UsuarioService.cs` | Crear, editar (rol y contraseña), bloquear y eliminar usuarios con `UserManager` |
| `Services/ErroresIdentidadEspanol.cs` | Mensajes de error de Identity en español (`AddErrorDescriber`) |
| `Views/Usuarios/` | Lista y formulario de usuarios; el menú *Usuarios* solo lo ve el administrador |

**Administración de usuarios** (`/Usuarios`): el administrador crea usuarios con su rol, cambia nombre,
correo, rol o contraseña, y bloquea, desbloquea o elimina cuentas. No puede quitarse su propio rol,
ni bloquearse o eliminarse a sí mismo. Al cambiar el rol o bloquear una cuenta se actualiza el
*security stamp*, así que la sesión de ese usuario deja de valer en menos de un minuto
(`SecurityStampValidatorOptions.ValidationInterval` en `Program.cs`).

## Estructura

| Carpeta / archivo | Contenido | Sección de la guía |
|---|---|---|
| `Entities/` | Clases POCO sin atributos | 3.1 |
| `Data/TiendaContext.cs` | DbSets, secuencia, `ApplyConfigurationsFromAssembly`, `ConfigureConventions` | 2.2, 2.4, 6.4, 15 |
| `Data/Configurations/` | Una clase `IEntityTypeConfiguration<T>` por entidad (toda la Fluent API) | 5 – 14, 17 |
| `Data/DatosDemo.cs` | Datos de ejemplo con `UseSeeding` / `UseAsyncSeeding` | 13 |
| `Migrations/` | Migración `Inicial` generada con `dotnet ef migrations add Inicial` | 16 |
| `Services/` | Lógica de negocio y consultas a la base (un servicio por módulo) | — |
| `Services/Interfaces/` | Contratos `I…Service` que reciben los controladores por inyección | — |
| `Services/Resultado.cs` | Resultado de cada operación: éxito, errores por campo o registro inexistente | — |
| `Controllers/`, `Views/` | Pantallas MVC para probar cada concepto | — |

## Capas

```
Vista  ⇄  Controlador  →  I…Service  →  …Service  →  TiendaContext (EF Core)  →  SQL Server
```

- **Controladores**: solo reciben la petición, validan las anotaciones del ViewModel (`ModelState`),
  llaman al servicio y deciden qué mostrar (vista, redirección o `NotFound`). No usan `TiendaContext`.
- **Servicios**: hacen las consultas, aplican las reglas de negocio (stock, ciclos de categorías,
  datos por tipo de pago, monto pendiente…) y traducen los errores de SQL Server a mensajes.
  No dependen de MVC: devuelven un `Resultado` en lugar de tocar `ModelState` o `TempData`.
- `Controllers/ResultadoExtensions.cs` pasa ese `Resultado` al `ModelState` (errores por campo)
  o a `TempData` (alertas).
- Los servicios se registran como `Scoped` en `Program.cs`, igual que el `DbContext`.

## Qué probar en la aplicación

| Pantalla | Prueba | Concepto |
|---|---|---|
| Productos | Asignar etiquetas, crear SKU repetido, borrar un producto ya vendido | N:N con `UsingEntity`, llave alterna, CHECK, `Restrict` |
| Categorías | Borrar "Hogar" (tiene productos) | Autorreferencia, `Restrict` |
| Etiquetas | Crear "Oferta" otra vez; borrar una etiqueta en uso | Índice único, cascada en tabla intermedia |
| Clientes | Eliminar y restaurar, ver eliminados, correo repetido | Filtro global, índice único filtrado, tipo propio, 1:1, propiedad sombra |
| Pedidos | Crear pedido, registrar pago, cambiar estado, borrar con y sin pagos | Secuencia, columna calculada, enum como texto, RowVersion, TPH, `Cascade` vs `Restrict` |
