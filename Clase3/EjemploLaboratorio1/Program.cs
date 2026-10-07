using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        const int cantidadEstudiantes = 100_000;

        var random = new Random(23);

        var estudiantes = new List<string>();
        var identificacionesABuscar = new List<string>();

        Console.WriteLine("Generando identificaciones...");

        for (int i = 0; i < cantidadEstudiantes; i++)
        {
            string identificacion = GenerarIdentificacion(random);

            estudiantes.Add(identificacion);

            // Guardamos las últimas 10,000 identificaciones
            if (i >= cantidadEstudiantes - 10_000)
            {
                identificacionesABuscar.Add(identificacion);
            }
        }

        Console.WriteLine("Iniciando búsquedas...");

        var stopwatch = Stopwatch.StartNew();

        int encontrados = 0;

        foreach (var identificacion in identificacionesABuscar)
        {
            if (estudiantes.Contains(identificacion))
            {
                encontrados++;
            }
        }

        stopwatch.Stop();

        Console.WriteLine($"Encontrados: {encontrados}");
        Console.WriteLine($"Tiempo: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
    }

    static string GenerarIdentificacion(Random random)
    {
        return $"{random.Next(1, 10)}-" +
               $"{random.Next(1000, 10000)}-" +
               $"{random.Next(1000, 10000)}";
    }
}