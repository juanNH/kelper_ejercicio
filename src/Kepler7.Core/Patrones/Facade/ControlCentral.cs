using Kepler7.Core.Contratos;

namespace Kepler7.Core.Patrones.Facade;

internal abstract class SubsistemaEstacion(string nombre)
{
    public string Nombre { get; } = nombre;
    public string Estado { get; private set; } = "Apagado";
    public string CambiarModo(string modo)
    {
        Estado = modo;
        return $"{Nombre}: {modo}.";
    }
}

internal sealed class Energia() : SubsistemaEstacion("Energía");
internal sealed class SoporteVital() : SubsistemaEstacion("Soporte vital");
internal sealed class Navegacion() : SubsistemaEstacion("Navegación");
internal sealed class Comunicaciones() : SubsistemaEstacion("Comunicaciones");
internal sealed class Seguridad() : SubsistemaEstacion("Seguridad");
internal sealed class Defensa() : SubsistemaEstacion("Defensa");

public sealed class ControlCentral : IControlEstacion
{
    private readonly Energia _energia = new();
    private readonly SoporteVital _soporte = new();
    private readonly Navegacion _navegacion = new();
    private readonly Comunicaciones _comunicaciones = new();
    private readonly Seguridad _seguridad = new();
    private readonly Defensa _defensa = new();
    private SubsistemaEstacion[] Subsistemas => [_energia, _soporte, _navegacion, _comunicaciones, _seguridad, _defensa];

    public IReadOnlyList<string> IniciarTurno() =>
    [
        _energia.CambiarModo("Operativa"), _soporte.CambiarModo("Oxígeno estable"),
        _navegacion.CambiarModo("Ruta calculada"), _comunicaciones.CambiarModo("Enlace disponible"),
        _seguridad.CambiarModo("Control de acceso activo"), _defensa.CambiarModo("En espera")
    ];

    public IReadOnlyList<string> EntrarEnEmergencia() =>
    [
        _energia.CambiarModo("Reserva de emergencia"), _soporte.CambiarModo("Prioridad vital"),
        _navegacion.CambiarModo("Maniobras suspendidas"), _comunicaciones.CambiarModo("Canal de emergencia"),
        _seguridad.CambiarModo("Accesos restringidos"), _defensa.CambiarModo("Alerta máxima")
    ];

    public IReadOnlyList<string> ApagarEstacion() =>
    [
        _defensa.CambiarModo("Apagado"), _seguridad.CambiarModo("Apagado"),
        _comunicaciones.CambiarModo("Apagado"), _navegacion.CambiarModo("Apagado"),
        _soporte.CambiarModo("Apagado"), _energia.CambiarModo("Apagado")
    ];

    public IReadOnlyList<EstadoSubsistema> ConsultarEstado() =>
        Subsistemas.Select(s => new EstadoSubsistema(s.Nombre, s.Estado)).ToArray();
}
