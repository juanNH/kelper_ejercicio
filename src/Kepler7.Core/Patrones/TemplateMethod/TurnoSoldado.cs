using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.TemplateMethod;

public sealed class TurnoSoldado(ContextoTurno contexto, Action<string>? registrar = null)
    : TurnoUnidad(contexto.Unidad, registrar, RolUnidad.Soldado)
{
    private Unidad? _objetivo;
    private Posicion? _destino;

    protected override void Decidir()
    {
        _destino = null;
        _objetivo = contexto.EnemigosVivos.OrderBy(u => Unidad.Posicion.DistanciaA(u.Posicion))
            .ThenBy(u => u.Nombre, StringComparer.Ordinal).FirstOrDefault();
        if (_objetivo is null) Registrar("Decisión: mantener la posición; no quedan enemigos vivos.");
        else if (contexto.EstaEnAlcance(_objetivo)) Registrar($"Decisión: atacar al enemigo más cercano, {_objetivo.Nombre}.");
        else
        {
            _destino = contexto.PasoHacia(Unidad, _objetivo.Posicion);
            Registrar(_destino.HasValue ? $"Decisión: avanzar una celda hacia {_objetivo.Nombre}."
                : "Decisión: mantener la posición; no hay un paso libre hacia el enemigo.");
        }
    }

    protected override void Actuar()
    {
        if (_objetivo is not null && contexto.EstaEnAlcance(_objetivo)) RegistrarResultado(contexto.Atacar(_objetivo));
        else if (_destino is { } destino) RegistrarResultado(contexto.Mover(Unidad, destino));
        else RegistrarResultado(ResultadoAccion.Correcto($"{Unidad.Nombre} mantiene su posición."));
    }
}
