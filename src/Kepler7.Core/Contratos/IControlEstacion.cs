using Kepler7.Core.Patrones.Bridge;

namespace Kepler7.Core.Contratos;

public sealed record EstadoSubsistema(string Nombre, string Estado);

public interface IControlEstacion
{
    IReadOnlyList<string> IniciarTurno();
    IReadOnlyList<string> EntrarEnEmergencia();
    IReadOnlyList<string> ApagarEstacion();
    IReadOnlyList<EstadoSubsistema> ConsultarEstado();
}

public interface IServicioComunicaciones
{
    TransmisionMensaje Enviar(TipoMensaje tipo, TipoCifrado cifrado, string contenido);
}
