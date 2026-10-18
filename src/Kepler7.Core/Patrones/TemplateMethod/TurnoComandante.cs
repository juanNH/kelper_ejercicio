using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.TemplateMethod;

public sealed class TurnoComandante(ContextoTurno contexto, Action<string>? registrar = null)
    : TurnoUnidad(contexto.Unidad, registrar, RolUnidad.Comandante)
{
    private Unidad? _aliado;
    private Posicion? _destino;

    protected override void Decidir()
    {
        _aliado = null;
        _destino = null;
        var candidatos = contexto.AliadosVivos.Where(u => u.Id != Unidad.Id && contexto.EstaEnAlcance(u)
                && Unidad.Posicion.DistanciaA(u.Posicion) > 1)
            .OrderByDescending(u => Unidad.Posicion.DistanciaA(u.Posicion))
            .ThenBy(u => u.Nombre, StringComparer.Ordinal);
        foreach (Unidad candidato in candidatos)
        {
            if (contexto.PasoHacia(candidato, Unidad.Posicion) is not { } paso) continue;
            _aliado = candidato;
            _destino = paso;
            break;
        }
        Registrar(_aliado is not null ? $"Decisión: reagrupar a {_aliado.Nombre} una celda hacia el comandante."
            : "Decisión: conservar la formación; no hay un aliado que pueda acercarse.");
    }

    protected override void Actuar() => RegistrarResultado(_aliado is not null && _destino is { } destino
        ? contexto.Mover(_aliado, destino)
        : ResultadoAccion.Correcto($"{Unidad.Nombre} mantiene la formación."));
}
