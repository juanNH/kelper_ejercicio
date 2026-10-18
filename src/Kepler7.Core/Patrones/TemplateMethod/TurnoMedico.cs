using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.TemplateMethod;

public sealed class TurnoMedico(ContextoTurno contexto, Action<string>? registrar = null)
    : TurnoUnidad(contexto.Unidad, registrar, RolUnidad.Medico)
{
    public const int CuracionPorTurno = 25;
    private Unidad? _paciente;
    private Posicion? _destino;

    protected override void Decidir()
    {
        _destino = null;
        var heridos = contexto.AliadosVivos.Where(u => u.Estadisticas.Vida < u.Estadisticas.VidaMaxima).ToArray();
        _paciente = heridos.Where(contexto.EstaEnAlcance)
            .OrderByDescending(u => u.Estadisticas.VidaMaxima - u.Estadisticas.Vida)
            .ThenBy(u => Unidad.Posicion.DistanciaA(u.Posicion)).ThenBy(u => u.Nombre, StringComparer.Ordinal).FirstOrDefault();
        if (_paciente is not null) Registrar($"Decisión: curar al aliado con más vida perdida en alcance, {_paciente.Nombre}.");
        else
        {
            _paciente = heridos.OrderBy(u => Unidad.Posicion.DistanciaA(u.Posicion))
                .ThenBy(u => u.Nombre, StringComparer.Ordinal).FirstOrDefault();
            if (_paciente is not null) _destino = contexto.PasoHacia(Unidad, _paciente.Posicion);
            Registrar(_destino.HasValue ? $"Decisión: acercarse una celda a {_paciente!.Nombre}."
                : "Decisión: permanecer disponible; no hay un herido accesible.");
        }
    }

    protected override void Actuar()
    {
        if (_paciente is not null && contexto.EstaEnAlcance(_paciente)) RegistrarResultado(contexto.Curar(_paciente, CuracionPorTurno));
        else if (_destino is { } destino) RegistrarResultado(contexto.Mover(Unidad, destino));
        else RegistrarResultado(ResultadoAccion.Correcto($"{Unidad.Nombre} permanece en apoyo médico."));
    }
}
