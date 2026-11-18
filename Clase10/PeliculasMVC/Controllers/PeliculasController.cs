using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PeliculasMVC.Controllers
{
    public class PeliculasController : Controller
    {

        // GET: PeliculasController
        public ActionResult Index()
        {
            var lasPeliculas = Servicios.Peliculas.ObtenerPeliculas();
            var peliculasViewModel = lasPeliculas.Select(p => new Models.ViewModels.Peliculas
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Director = p.Director,
                Sinopsis = p.Sinopsis,
                FechaDeEstreno = p.FechaDeEstreno
            }).ToList();

            return View(peliculasViewModel);
        }

        // GET: PeliculasController/Details/5
        public ActionResult Details(int id)
        {
            var laPelicula = Servicios.Peliculas.ObtenerPelicula(id);

            return View(laPelicula);
        }

        // GET: PeliculasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PeliculasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Models.ViewModels.Peliculas pelicula)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(pelicula);
                }

                Servicios.Peliculas.GuardarPelicula(pelicula);

                return RedirectToAction(nameof(Index));
            }
            catch(InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Create", pelicula);
            }
        }

        // GET: PeliculasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PeliculasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PeliculasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PeliculasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
