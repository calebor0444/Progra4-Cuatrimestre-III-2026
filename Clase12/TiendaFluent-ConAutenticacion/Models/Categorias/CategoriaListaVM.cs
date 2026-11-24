namespace TiendaFluent.Models;

public record CategoriaListaVM(int Id, string Nombre, string? Padre, int Productos, int Subcategorias);
