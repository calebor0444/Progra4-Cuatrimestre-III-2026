using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PeliculasMVC.Models.ViewModels
{
    public class Peliculas
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 10 caracteres.")]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Director { get; set; } = string.Empty;
        public string? Sinopsis { get; set; } = string.Empty;
        [Required]
        [DisplayName("Fecha de estreno")]
        public DateTime FechaDeEstreno { get; set; }

        [NotMapped]
        public string FechaDeEstrenoFormateada
        {
            get
            {
                return FechaDeEstreno.ToString("dd/MM/yyyy");
            }
        }
    }
}
