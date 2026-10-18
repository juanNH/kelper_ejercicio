using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Modelos;

public sealed class Unidad : IPrototipoUnidad
{
    private readonly List<IItem> _inventario;
    public Guid Id { get; } = Guid.NewGuid();
    public string Nombre { get; private set; }
    public RolUnidad Rol { get; }
    public Equipo Equipo { get; }
    public IEspecie Especie { get; }
    public Estadisticas Estadisticas { get; private set; }
    public Posicion Posicion { get; private set; }
    public int Escudo { get; private set; }
    public IEstadoPersonaje Estado { get; private set; } = new Normal();
    public bool EstaViva => Estadisticas.Vida > 0;
    public IReadOnlyList<IItem> Inventario => _inventario.AsReadOnly();

    public Unidad(string nombre, RolUnidad rol, Equipo equipo, IEspecie especie,
        Estadisticas estadisticas, Posicion posicion, IEnumerable<IItem> inventario, int escudo = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentNullException.ThrowIfNull(especie);
        ArgumentNullException.ThrowIfNull(estadisticas);
        ArgumentNullException.ThrowIfNull(inventario);
        ArgumentOutOfRangeException.ThrowIfNegative(escudo);
        if (!Enum.IsDefined(rol)) throw new ArgumentOutOfRangeException(nameof(rol));
        if (!Enum.IsDefined(equipo)) throw new ArgumentOutOfRangeException(nameof(equipo));
        if (!posicion.EstaDentro(8, 8)) throw new ArgumentOutOfRangeException(nameof(posicion));
        Nombre = nombre.Trim();
        Rol = rol;
        Equipo = equipo;
        Especie = especie;
        Estadisticas = estadisticas.Clonar();
        Posicion = posicion;
        Escudo = escudo;
        _inventario = inventario.Select(item => item.Copiar()).ToList();
    }

    public int Atacar() => EstaViva ? Estado.Atacar(this) : 0;

    public bool Mover(Posicion destino)
    {
        if (!EstaViva || !Estado.Mover(this, destino)) return false;
        Posicion = destino;
        return true;
    }

    // State modifica el daño entrante; la cadena resuelve después las defensas y la vida.
    public int RecibirDanio(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        return Estado.RecibirDanio(this, cantidad);
    }

    public void CambiarEstado(IEstadoPersonaje estado) => Estado = estado ?? throw new ArgumentNullException(nameof(estado));
    public void AgregarItem(IItem item) => _inventario.Add(item.Copiar());

    public int AbsorberConEscudo(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        int absorbido = Math.Min(Escudo, cantidad);
        Escudo -= absorbido;
        return absorbido;
    }

    public Unidad Clonar()
    {
        var copia = new Unidad(Nombre, Rol, Equipo, Especie, Estadisticas, Posicion, Inventario, Escudo);
        copia.CambiarEstado(Estado.Clonar());
        return copia;
    }

    public Unidad ClonarConAjustes(string nombre, Estadisticas estadisticas, Posicion posicion, IEnumerable<IItem> equipamiento)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentNullException.ThrowIfNull(estadisticas);
        ArgumentNullException.ThrowIfNull(equipamiento);
        if (!posicion.EstaDentro(8, 8)) throw new ArgumentOutOfRangeException(nameof(posicion));
        IItem[] items = equipamiento.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            return item.Copiar();
        }).ToArray();

        // La variante parte de una copia del prototipo: conserva rol, equipo,
        // especie, escudo y una copia independiente del estado, sin usar Builder.
        Unidad copia = Clonar();
        copia.Nombre = nombre.Trim();
        copia.Estadisticas = estadisticas.Clonar();
        copia.Posicion = posicion;
        copia._inventario.Clear();
        copia._inventario.AddRange(items);
        return copia;
    }

    public override string ToString() => $"{Nombre} · {Rol} · {Estadisticas.Vida}/{Estadisticas.VidaMaxima} PV";
}
