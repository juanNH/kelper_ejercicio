using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.State;

public interface IEstadoPersonaje
{
    TipoEstado Tipo { get; }
    int Atacar(Unidad personaje);
    bool Mover(Unidad personaje, Posicion destino);
    int RecibirDanio(Unidad personaje, int cantidad);
    IEstadoPersonaje Clonar();
}

public sealed class Normal : IEstadoPersonaje
{
    public TipoEstado Tipo => TipoEstado.Normal;
    public int Atacar(Unidad personaje) => personaje.Estadisticas.Ataque;
    public bool Mover(Unidad personaje, Posicion destino) => true;
    public int RecibirDanio(Unidad personaje, int cantidad) => cantidad;
    public IEstadoPersonaje Clonar() => new Normal();
}

public sealed class Envenenado : IEstadoPersonaje
{
    public TipoEstado Tipo => TipoEstado.Envenenado;
    public int Atacar(Unidad personaje) => personaje.Estadisticas.Ataque * 3 / 4;
    public bool Mover(Unidad personaje, Posicion destino) => true;
    public int RecibirDanio(Unidad personaje, int cantidad) => cantidad == 0 ? 0 : cantidad + 2;
    public IEstadoPersonaje Clonar() => new Envenenado();
}

public sealed class Aturdido : IEstadoPersonaje
{
    public TipoEstado Tipo => TipoEstado.Aturdido;
    public int Atacar(Unidad personaje) => 0;
    public bool Mover(Unidad personaje, Posicion destino) => false;
    public int RecibirDanio(Unidad personaje, int cantidad) => cantidad;
    public IEstadoPersonaje Clonar() => new Aturdido();
}

public sealed class Berserk : IEstadoPersonaje
{
    public TipoEstado Tipo => TipoEstado.Berserk;
    public int Atacar(Unidad personaje) => personaje.Estadisticas.Ataque * 3 / 2;
    public bool Mover(Unidad personaje, Posicion destino) => true;
    public int RecibirDanio(Unidad personaje, int cantidad) => (int)Math.Ceiling(cantidad * 1.25);
    public IEstadoPersonaje Clonar() => new Berserk();
}

public static class EstadosPersonaje
{
    public static IEstadoPersonaje Crear(TipoEstado tipo) => tipo switch
    {
        TipoEstado.Normal => new Normal(),
        TipoEstado.Envenenado => new Envenenado(),
        TipoEstado.Aturdido => new Aturdido(),
        TipoEstado.Berserk => new Berserk(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
