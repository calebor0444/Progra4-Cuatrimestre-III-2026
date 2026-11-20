using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TiendaFluent.Data;
using TiendaFluent.Services;
using TiendaFluent.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registro del DbContext (guía 2.3) + datos de ejemplo con UseSeeding (guía 13)
builder.Services.AddDbContext<TiendaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Tienda"))
           .UseSeeding((context, _) => DatosDemo.Sembrar((TiendaContext)context))
           .UseAsyncSeeding((context, _, ct) => DatosDemo.SembrarAsync((TiendaContext)context, ct)));

// Servicios de negocio. Scoped = una instancia por petición, igual que el DbContext que reciben
builder.Services.AddScoped<IResumenService, ResumenService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IEtiquetaService, EtiquetaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
