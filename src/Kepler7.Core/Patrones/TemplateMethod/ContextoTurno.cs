using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.TemplateMethod;

public sealed class ContextoTurno
{
    private readonly IServicioCombate _combate;
    public Unidad Unidad { get; }
    public IEnumerable<Unidad> AliadosVivos => _combate.Mision.Unidades.Where(u => u.Equipo == Unidad.Equipo && u.EstaViva);
    public IEnumerable<Unidad> EnemigosVivos => _combate.Mision.Unidades.Where(u => u.Equipo != Unidad.Equipo && u.EstaViva);

    public ContextoTurno(Guid unidadId, IServicioCombate combate)
    {
        ArgumentNullException.ThrowIfNull(combate);
        _combate = combate;
        Unidad = combate.Mision.BuscarUnidad(unidadId);
    }

    public bool EstaEnAlcance(Unidad objetivo) =>
        Unidad.Posicion.DistanciaA(objetivo.Posicion) <= Unidad.Estadisticas.Alcance;

    public Posicion? PasoHacia(Unidad movil, Posicion destino)
    {
        var vecinas = new[]
        {
            new Posicion(movil.Posicion.X - 1, movil.Posicion.Y), new Posicion(movil.Posicion.X + 1, movil.Posicion.Y),
            new Posicion(movil.Posicion.X, movil.Posicion.Y - 1), new Posicion(movil.Posicion.X, movil.Posicion.Y + 1)
        };
        return vecinas.Where(p => p.EstaDentro(_combate.Mision.Ancho, _combate.Mision.Alto)
                && !_combate.Mision.EstaOcupada(p) && p.DistanciaA(destino) < movil.Posicion.DistanciaA(destino))
            .OrderBy(p => p.DistanciaA(destino)).ThenBy(p => p.X).ThenBy(p => p.Y)
            .Select(p => (Posicion?)p).FirstOrDefault();
    }

    public ResultadoAccion Mover(Unidad movil, Posicion destino) => _combate.Mover(movil.Id, destino);
    public ResultadoAccion Atacar(Unidad objetivo) =>
        _combate.Atacar(Unidad.Id, objetivo.Id, ConfiguracionDefensa.Predeterminada);

    public ResultadoAccion Curar(Unidad objetivo, int cantidad)
    {
        if (objetivo.Equipo != Unidad.Equipo || !objetivo.EstaViva || !EstaEnAlcance(objetivo))
            return ResultadoAccion.Rechazado("La curación requiere un aliado vivo dentro del alcance del médico.");
        int recuperado = objetivo.Estadisticas.Curar(cantidad);
        return ResultadoAccion.Correcto($"{Unidad.Nombre} curó {recuperado} PV a {objetivo.Nombre}.",
            $"Vida de {objetivo.Nombre}: {objetivo.Estadisticas.Vida}/{objetivo.Estadisticas.VidaMaxima}.");
    }
}
