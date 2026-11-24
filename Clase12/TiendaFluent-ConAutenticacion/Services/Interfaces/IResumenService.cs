using TiendaFluent.Models;

namespace TiendaFluent.Services.Interfaces;

public interface IResumenService
{
    Task<ResumenVM> ObtenerAsync();
}
