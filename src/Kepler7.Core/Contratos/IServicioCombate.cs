using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Contratos;

public interface IServicioCombate
{
    Mision Mision { get; }
    ResultadoAccion IncorporarUnidad(Unidad unidad);
    ResultadoAccion Mover(Guid unidadId, Posicion destino);
    ResultadoAccion Atacar(Guid atacanteId, Guid objetivoId, ConfiguracionDefensa defensa);
    ResultadoAccion CambiarEstado(Guid unidadId, TipoEstado estado);
}
