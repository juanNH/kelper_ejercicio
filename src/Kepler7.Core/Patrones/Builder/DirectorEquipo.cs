using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Builder;

public sealed class DirectorEquipo
{
    public IReadOnlyList<Unidad> CrearExploracion(Posicion? origen = null) => Crear(
        [RolUnidad.Soldado, RolUnidad.Medico, RolUnidad.Francotirador], origen ?? new Posicion(1, 1));

    public IReadOnlyList<Unidad> CrearAsalto(Posicion? origen = null) => Crear(
        [RolUnidad.Comandante, RolUnidad.Soldado, RolUnidad.Soldado], origen ?? new Posicion(1, 1));

    private static IReadOnlyList<Unidad> Crear(RolUnidad[] roles, Posicion origen)
    {
        if (!origen.EstaDentro(8, 8) || !new Posicion(origen.X, origen.Y + roles.Length - 1).EstaDentro(8, 8))
            throw new ArgumentOutOfRangeException(nameof(origen), "El escuadrón ocupa tres celdas verticales y debe caber en el tablero.");
        return roles.Select((rol, indice) => new UnidadBuilder()
            .DefinirIdentidad($"{rol} {indice + 1}", Equipo.Aliado, CatalogoInicial.Especies[0])
            .SeleccionarRol(rol)
            .ConfigurarEstadisticas(CatalogoInicial.EstadisticasPara(rol))
            .ConfigurarEquipamiento(CatalogoInicial.EquipamientoPara(rol))
            .Ubicar(new Posicion(origen.X, origen.Y + indice))
            .Construir()).ToArray();
    }
}
