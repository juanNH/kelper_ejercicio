using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.TemplateMethod;

public sealed class TurnoFrancotirador(ContextoTurno contexto, Action<string>? registrar = null)
    : TurnoUnidad(contexto.Unidad, registrar, RolUnidad.Francotirador)
{
    private Unidad? _objetivo;

    protected override void Decidir()
    {
        _objetivo = contexto.EnemigosVivos.Where(contexto.EstaEnAlcance)
            .OrderBy(u => u.Estadisticas.Vida).ThenBy(u => Unidad.Posicion.DistanciaA(u.Posicion))
            .ThenBy(u => u.Nombre, StringComparer.Ordinal).FirstOrDefault();
        Registrar(_objetivo is not null ? $"Decisión: disparar al enemigo con menos PV en alcance, {_objetivo.Nombre}."
            : "Decisión: conservar la posición de tiro; no hay objetivos en alcance.");
    }

    protected override void Actuar() => RegistrarResultado(_objetivo is not null
        ? contexto.Atacar(_objetivo)
        : ResultadoAccion.Correcto($"{Unidad.Nombre} vigila desde su posición."));
}
