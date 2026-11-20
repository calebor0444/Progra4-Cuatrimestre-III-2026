using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TiendaFluent.Services;

namespace TiendaFluent.Controllers;

// Traduce un Resultado de los servicios a lo que entiende MVC
public static class ResultadoExtensions
{
    // Los errores van al ModelState para que la vista los muestre junto a cada campo
    public static void AgregarErrores(this ModelStateDictionary modelState, Resultado resultado)
    {
        foreach (var error in resultado.Errores)
            modelState.AddModelError(error.Campo, error.Mensaje);
    }

    // El mensaje va a TempData: el layout lo muestra como alerta verde (Ok) o roja (Error)
    public static void Notificar(this Controller controller, Resultado resultado) =>
        controller.TempData[resultado.Exito ? "Ok" : "Error"] = resultado.Mensaje;
}
