using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.TemplateMethod;

public enum EtapaTurno { Inicio, Decision, Accion, Finalizacion }
public sealed record EventoTurno(EtapaTurno Etapa, string Mensaje);

public abstract class TurnoUnidad
{
    private readonly List<EventoTurno> _eventos = [];
    private readonly Action<string> _registrar;
    private EtapaTurno _etapa;
    private ResultadoAccion _resultado = ResultadoAccion.Correcto("Turno en espera.");
    protected Unidad Unidad { get; }
    public IReadOnlyList<EventoTurno> Eventos => _eventos.AsReadOnly();

    protected TurnoUnidad(Unidad unidad, Action<string>? registrar = null, RolUnidad? rolEsperado = null)
    {
        ArgumentNullException.ThrowIfNull(unidad);
        if (rolEsperado.HasValue && unidad.Rol != rolEsperado)
            throw new ArgumentException($"Este turno requiere una unidad con rol {rolEsperado}.", nameof(unidad));
        Unidad = unidad;
        _registrar = registrar ?? (_ => { });
    }

    // No virtual: las variantes cambian los pasos, pero conservan el orden.
    public ResultadoAccion EjecutarTurno()
    {
        _eventos.Clear();
        _resultado = ResultadoAccion.Correcto($"{Unidad.Nombre} espera.");
        string? bloqueo = !Unidad.EstaViva ? "La unidad está fuera de combate."
            : Unidad.Estado.Tipo == TipoEstado.Aturdido ? "La unidad está aturdida y no puede actuar." : null;

        EjecutarEtapa(EtapaTurno.Inicio, Iniciar);
        EjecutarEtapa(EtapaTurno.Decision, () =>
        {
            if (bloqueo is null) Decidir();
            else Registrar($"Decisión: omitir la acción. {bloqueo}");
        });
        EjecutarEtapa(EtapaTurno.Accion, () =>
        {
            if (bloqueo is null) Actuar();
            else RegistrarResultado(ResultadoAccion.Rechazado(bloqueo));
        });
        EjecutarEtapa(EtapaTurno.Finalizacion, Finalizar);
        return _resultado;
    }

    protected void Registrar(string mensaje)
    {
        _eventos.Add(new(_etapa, mensaje));
        _registrar(mensaje);
    }

    protected void RegistrarResultado(ResultadoAccion resultado)
    {
        _resultado = resultado;
        Registrar($"Acción: {resultado.Mensaje}");
        foreach (string evento in resultado.Eventos) Registrar(evento);
    }

    private void EjecutarEtapa(EtapaTurno etapa, Action paso)
    {
        _etapa = etapa;
        paso();
    }

    protected virtual void Iniciar() => Registrar($"Inicio del turno de {Unidad.Nombre}.");
    protected abstract void Decidir();
    protected abstract void Actuar();
    protected virtual void Finalizar() => Registrar($"Fin del turno de {Unidad.Nombre}.");
}
