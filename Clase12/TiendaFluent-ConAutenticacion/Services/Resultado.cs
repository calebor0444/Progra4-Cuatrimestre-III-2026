namespace TiendaFluent.Services;

// Error de una regla de negocio. Campo es el nombre de la propiedad del ViewModel
// (vacío si el error es general) para que el controlador lo pueda poner en ModelState.
public record ErrorNegocio(string Campo, string Mensaje);

// Resultado de una operación de un servicio. El servicio no conoce MVC (ni ModelState,
// ni TempData, ni NotFound): solo dice qué pasó y el controlador decide cómo mostrarlo.
public class Resultado
{
    public bool NoEncontrado { get; protected init; }
    public IReadOnlyList<ErrorNegocio> Errores { get; protected init; } = [];

    // Mensaje para el usuario: el de éxito o, si falló, los errores juntos
    public string? Mensaje { get; protected init; }

    public bool Exito => !NoEncontrado && Errores.Count == 0;

    public static Resultado Ok(string? mensaje = null) => new() { Mensaje = mensaje };

    public static Resultado Falla(string mensaje, string campo = "") =>
        new() { Errores = [new(campo, mensaje)], Mensaje = mensaje };

    public static Resultado Falla(IReadOnlyList<ErrorNegocio> errores) =>
        new() { Errores = errores, Mensaje = string.Join(" ", errores.Select(e => e.Mensaje)) };

    public static Resultado NoExiste() => new() { NoEncontrado = true, Mensaje = "El registro no existe." };
}

// Igual que Resultado pero devuelve un valor cuando sale bien (por ejemplo, el Id creado)
public class Resultado<T> : Resultado
{
    public T? Valor { get; private init; }

    public static Resultado<T> Ok(T valor, string? mensaje = null) => new() { Valor = valor, Mensaje = mensaje };

    public static new Resultado<T> Falla(string mensaje, string campo = "") =>
        new() { Errores = [new(campo, mensaje)], Mensaje = mensaje };

    public static new Resultado<T> Falla(IReadOnlyList<ErrorNegocio> errores) =>
        new() { Errores = errores, Mensaje = string.Join(" ", errores.Select(e => e.Mensaje)) };

    public static new Resultado<T> NoExiste() => new() { NoEncontrado = true, Mensaje = "El registro no existe." };
}
