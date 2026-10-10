namespace Ejercicio3_OrdenamientoSegunCantidadDeLetras
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var laLista = new List<string>() { "Ana", "Juan", "Pedro", "Alejandro", "Maximiliano" };

            //Ascendente o default
            laLista.Sort();

            foreach (var item in laLista)
            {
                Console.WriteLine(item);
            }


            //Descendente
            laLista.Sort((nombre1, nombre2) => nombre2.CompareTo(nombre1));
         
            foreach (var item in laLista)
            {
                Console.WriteLine(item);
            }


            //Ordenamiento por cantidad de letras (mas customizado)
            //Ejemplo :
            Console.WriteLine();
            laLista.Sort((nombre1, nombre2) =>
            {
                return nombre1.Length.CompareTo(nombre2.Length);
            });

            Console.WriteLine("Mostrando ordenamiento por orden de más a menos");
            foreach (var item in laLista)
            {
                Console.WriteLine(item);
            }

            //Ordenamiento por mas cantidad de una letra en especifico
            Console.WriteLine("Con más cantidad de la letra 'a'");
            laLista.Sort((nombre1, nombre2) =>
            {
                int cantidadDeAEnNombre1 = nombre1.Count(c => c == 'a' || c == 'A');
                int cantidadDeAEnNombre2 = nombre2.Count(c => c == 'a' || c == 'A');
                return cantidadDeAEnNombre2.CompareTo(cantidadDeAEnNombre1);
            });
            foreach (var item in laLista)
            {
                Console.WriteLine(item);
            }
        }
    }
}
