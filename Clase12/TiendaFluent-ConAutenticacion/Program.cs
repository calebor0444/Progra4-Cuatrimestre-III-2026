using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Entities;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registro del DbContext (guía 2.3) + datos de ejemplo con UseSeeding (guía 13)
builder.Services.AddDbContext<TiendaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Tienda"))
           .UseSeeding((context, _) => DatosDemo.Sembrar((TiendaContext)context))
           .UseAsyncSeeding((context, _, ct) => DatosDemo.SembrarAsync((TiendaContext)context, ct)));

// ASP.NET Core Identity: usuarios y roles guardados con EF Core en TiendaContext
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<TiendaContext>()
    .AddErrorDescriber<ErroresIdentidadEspanol>()   // mensajes de Identity en español
    .AddDefaultTokenProviders();

// Cookie de autenticación: a dónde mandar al usuario si no ha iniciado sesión o no tiene permiso
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.LogoutPath = "/Cuenta/Logout";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Cada minuto se compara el sello de seguridad de la cookie con el de la base: si un administrador
// cambia el rol, bloquea o elimina a un usuario, su sesión deja de valer (por defecto son 30 min)
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(1));

// Servicios de negocio. Scoped = una instancia por petición, igual que el DbContext que reciben
builder.Services.AddScoped<IResumenService, ResumenService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IEtiquetaService, EtiquetaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// Roles (Administrador, Empleado) y usuario administrador inicial
await DatosIdentidad.SembrarAsync(app.Services);

// Cultura con punto decimal para que los precios se lean bien desde los formularios
var cultura = new CultureInfo("es-CR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura],
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// El orden importa: primero se identifica al usuario (cookie) y luego se revisan sus permisos
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
