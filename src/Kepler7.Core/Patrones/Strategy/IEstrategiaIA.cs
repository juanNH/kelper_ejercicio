using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Strategy;

public interface IEstrategiaIA
{
    string Nombre { get; }
    AccionIA DecidirAccion(Enemigo enemigo, ContextoCombate contexto);
}
