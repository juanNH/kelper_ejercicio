using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Strategy;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Contratos;

// Estos servicios se conectarán cuando se implementen las siguientes etapas.
public interface IServicioIA
{
    void CambiarEstrategia(Guid enemigoId, IEstrategiaIA estrategia);
    AccionIA DecidirAccion(Guid enemigoId, ContextoCombate contexto);
}

public interface IServicioInventario
{
    ResultadoAccion Analizar(Guid unidadId, IVisitor visitante);
}

public sealed record ProgresoJugador(int Experiencia, IReadOnlyList<string> Logros);

public interface IRepositorioProgreso
{
    Task<ProgresoJugador> CargarAsync(CancellationToken cancellationToken = default);
    Task GuardarAsync(ProgresoJugador progreso, CancellationToken cancellationToken = default);
}
