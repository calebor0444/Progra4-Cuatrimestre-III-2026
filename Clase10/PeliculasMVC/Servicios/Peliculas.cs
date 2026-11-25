
namespace PeliculasMVC.Servicios
{
    public class Peliculas
    {
        public static List<Models.Entidades.Peliculas> peliculas = null;

        public static List<Models.Entidades.Peliculas> ObtenerPeliculas()
        {
            if (peliculas == null)
            {
                peliculas = new List<Models.Entidades.Peliculas>
                {
                    new Models.Entidades.Peliculas { Id = 1, Titulo = "Pelicula 1", Director = "Director 1",
                        Sinopsis = "Sinopsis 1", FechaDeEstreno = new DateTime(2023, 1, 1) },
                    new Models.Entidades.Peliculas { Id = 2, Titulo = "Pelicula 2", Director = "Director 2",
                        Sinopsis = "Sinopsis 2", FechaDeEstreno = new DateTime(2023, 2, 1) },
                    new Models.Entidades.Peliculas { Id = 3, Titulo = "Pelicula 3", Director = "Director 3",
                        Sinopsis = "Sinopsis 3", FechaDeEstreno = new DateTime(2023, 3, 1) }
                };
            }
            return peliculas;
        }

        internal static void EliminarPelicula(int id)
        {
            var pelicula = peliculas.FirstOrDefault(p => p.Id == id);
            if (pelicula != null)
            {
                peliculas.Remove(pelicula);
            }
        }

        internal static void GuardarPelicula(Models.ViewModels.Peliculas pelicula)
        {
            bool existe = peliculas.Any(p => p.Titulo.Equals(pelicula.Titulo, StringComparison.OrdinalIgnoreCase));
            if (existe)
            {
                throw new InvalidOperationException("Ya existe una película con el mismo título.");
            }

            var nuevaPelicula = new Models.Entidades.Peliculas
            {
                Id = peliculas.Count + 1,
                Titulo = pelicula.Titulo,
                Director = pelicula.Director,
                Sinopsis = pelicula.Sinopsis,
                FechaDeEstreno = pelicula.FechaDeEstreno
            };

            peliculas.Add(nuevaPelicula);
        }

        internal static Models.ViewModels.Peliculas ObtenerPelicula(int id)
        {
            var laPelicula = peliculas.FirstOrDefault(p => p.Id == id);
            var laPeliculaMapeada = new Models.ViewModels.Peliculas
            {
                Id = laPelicula.Id,
                Titulo = laPelicula.Titulo,
                Director = laPelicula.Director,
                Sinopsis = laPelicula.Sinopsis,
                FechaDeEstreno = laPelicula.FechaDeEstreno
            };
           return laPeliculaMapeada;
        }
    }
}
