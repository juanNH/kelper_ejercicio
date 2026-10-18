using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.TemplateMethod;

namespace Kepler7.Core.Contratos;

public sealed record ResultadoTurno(int Numero, Guid UnidadId, string NombreUnidad, RolUnidad Rol,
    bool Exito, string Resumen, IReadOnlyList<EventoTurno> Eventos)
{
    public string Descripcion => $"{Numero}. {NombreUnidad} · {Rol} · {(Exito ? "completado" : "omitido")}";
}

public interface IServicioTurnos
{
    IReadOnlyList<Unidad> Unidades { get; }
    IReadOnlyList<ResultadoTurno> Historial { get; }
    ResultadoTurno EjecutarTurno(Guid unidadId);
}
