using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.TemplateMethod;

namespace Kepler7.Core.Servicios;

public sealed class ServicioTurnos(IServicioCombate combate) : IServicioTurnos
{
    private readonly List<ResultadoTurno> _historial = [];
    public IReadOnlyList<Unidad> Unidades => combate.Mision.Unidades.ToArray();
    public IReadOnlyList<ResultadoTurno> Historial => _historial.AsReadOnly();

    public ResultadoTurno EjecutarTurno(Guid unidadId)
    {
        if (!Unidades.Any(u => u.Id == unidadId))
            throw new ArgumentException("Seleccioná una unidad de la misión.", nameof(unidadId));
        var contexto = new ContextoTurno(unidadId, combate);
        TurnoUnidad turno = contexto.Unidad.Rol switch
        {
            RolUnidad.Soldado => new TurnoSoldado(contexto),
            RolUnidad.Medico => new TurnoMedico(contexto),
            RolUnidad.Francotirador => new TurnoFrancotirador(contexto),
            RolUnidad.Comandante => new TurnoComandante(contexto),
            _ => throw new ArgumentOutOfRangeException(nameof(unidadId), "El rol no tiene un turno definido.")
        };
        ResultadoAccion accion = turno.EjecutarTurno();
        var resultado = new ResultadoTurno(_historial.Count + 1, unidadId, contexto.Unidad.Nombre, contexto.Unidad.Rol,
            accion.Exito, accion.Mensaje, Array.AsReadOnly(turno.Eventos.ToArray()));
        _historial.Add(resultado);
        return resultado;
    }
}
