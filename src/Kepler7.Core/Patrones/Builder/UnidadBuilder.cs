using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Patrones.Builder;

public interface IBuilderUnidad
{
    IBuilderUnidad DefinirIdentidad(string nombre, Equipo equipo, IEspecie especie);
    IBuilderUnidad SeleccionarRol(RolUnidad rol);
    IBuilderUnidad ConfigurarEstadisticas(Estadisticas estadisticas);
    IBuilderUnidad ConfigurarEquipamiento(IEnumerable<IItem> equipamiento);
    IBuilderUnidad Ubicar(Posicion posicion);
    Unidad Construir();
}

public sealed class UnidadBuilder : IBuilderUnidad
{
    private string? _nombre;
    private Equipo _equipo;
    private IEspecie? _especie;
    private RolUnidad? _rol;
    private Estadisticas? _estadisticas;
    private List<IItem>? _equipamiento;
    private Posicion _posicion;

    public IBuilderUnidad DefinirIdentidad(string nombre, Equipo equipo, IEspecie especie)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentNullException.ThrowIfNull(especie);
        if (!Enum.IsDefined(equipo)) throw new ArgumentOutOfRangeException(nameof(equipo));
        _nombre = nombre;
        _equipo = equipo;
        _especie = especie;
        return this;
    }

    public IBuilderUnidad SeleccionarRol(RolUnidad rol)
    {
        if (!Enum.IsDefined(rol)) throw new ArgumentOutOfRangeException(nameof(rol));
        _rol = rol;
        return this;
    }

    public IBuilderUnidad ConfigurarEstadisticas(Estadisticas estadisticas)
    {
        ArgumentNullException.ThrowIfNull(estadisticas);
        _estadisticas = estadisticas.Clonar();
        return this;
    }

    public IBuilderUnidad ConfigurarEquipamiento(IEnumerable<IItem> equipamiento)
    {
        ArgumentNullException.ThrowIfNull(equipamiento);
        _equipamiento = equipamiento.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            return item.Copiar();
        }).ToList();
        return this;
    }

    public IBuilderUnidad Ubicar(Posicion posicion)
    {
        if (!posicion.EstaDentro(8, 8)) throw new ArgumentOutOfRangeException(nameof(posicion), "El tablero es de 8 × 8.");
        _posicion = posicion;
        return this;
    }

    public Unidad Construir()
    {
        if (_nombre is null || _especie is null || _rol is null || _estadisticas is null || _equipamiento is null)
            throw new InvalidOperationException("Faltan identidad, rol, estadísticas o equipamiento para completar la unidad.");
        return new Unidad(_nombre, _rol.Value, _equipo, _especie, _estadisticas, _posicion, _equipamiento);
    }
}
