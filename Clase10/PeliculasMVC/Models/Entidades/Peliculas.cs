using System.ComponentModel.DataAnnotations;

namespace PeliculasMVC.Models.Entidades
{
    public class Peliculas
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Director { get; set; } = string.Empty;
        public string? Sinopsis { get; set; } = string.Empty;
        [Required]
        public DateTime FechaDeEstreno { get; set; }
    }
}
