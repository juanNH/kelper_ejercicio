using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Patrones.Prototype;

public sealed record VistaUnidad(Guid Id, string Nombre, RolUnidad Rol, string Especie,
    int Vida, int Ataque, Posicion Posicion, TipoEstado Estado, IReadOnlyList<string> Inventario,
    Equipo Equipo, int VidaMaxima, int Defensa, int Alcance, int Movimiento, int Escudo, IReadOnlyList<IItem> Equipamiento)
{
    public static VistaUnidad Desde(Unidad unidad) => new(unidad.Id, unidad.Nombre, unidad.Rol,
        unidad.Especie.Nombre, unidad.Estadisticas.Vida, unidad.Estadisticas.Ataque,
        unidad.Posicion, unidad.Estado.Tipo, Array.AsReadOnly(unidad.Inventario.Select(i => i.Nombre).ToArray()),
        unidad.Equipo, unidad.Estadisticas.VidaMaxima, unidad.Estadisticas.Defensa, unidad.Estadisticas.Alcance,
        unidad.Estadisticas.Movimiento, unidad.Escudo, Array.AsReadOnly(unidad.Inventario.Select(i => i.Copiar()).ToArray()));
}

public sealed class RegistroPrototipos
{
    private readonly Dictionary<string, Unidad> _plantillas = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<string> Identificadores => _plantillas.Keys.Order().ToArray();

    public void Registrar(string id, Unidad plantilla)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(plantilla);
        // El registro conserva su propia copia para que tampoco lo altere quien lo carga.
        _plantillas[id] = plantilla.Clonar();
    }

    public Unidad Clonar(string id) => Buscar(id).Clonar();
    public Unidad CrearVariante(string id, string nombre, Estadisticas estadisticas, Posicion posicion, IEnumerable<IItem> equipamiento) =>
        Buscar(id).ClonarConAjustes(nombre, estadisticas, posicion, equipamiento);
    public VistaUnidad Consultar(string id) => VistaUnidad.Desde(Buscar(id));

    private Unidad Buscar(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _plantillas.TryGetValue(id, out var unidad)
            ? unidad : throw new ArgumentException("No existe esa plantilla.", nameof(id));
    }
}
