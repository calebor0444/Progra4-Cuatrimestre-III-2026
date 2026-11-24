namespace TiendaFluent.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool Eliminado { get; set; }
    public Direccion Direccion { get; set; } = null!;     // tipo propio (OwnsOne)
    public PerfilCliente? Perfil { get; set; }            // 1:1
    public List<Pedido> Pedidos { get; set; } = new();    // 1:N
}
