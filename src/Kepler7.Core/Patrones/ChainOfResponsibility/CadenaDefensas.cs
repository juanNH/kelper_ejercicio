using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.ChainOfResponsibility;

public enum CapaDefensa { Esquiva, Bloqueo, Armadura, Escudo, Vida }

public sealed record ConfiguracionDefensa(IReadOnlyList<CapaDefensa> Orden, bool Esquivar = false, bool Bloquear = false)
{
    public static ConfiguracionDefensa Predeterminada => new(
        [CapaDefensa.Esquiva, CapaDefensa.Bloqueo, CapaDefensa.Armadura, CapaDefensa.Escudo, CapaDefensa.Vida]);
}

public sealed class Danio
{
    private readonly List<string> _recorrido = [];
    public Unidad Objetivo { get; }
    public int CantidadInicial { get; }
    public int Restante { get; private set; }
    public bool Esquivar { get; }
    public bool Bloquear { get; }
    public IReadOnlyList<string> Recorrido => _recorrido.AsReadOnly();

    public Danio(Unidad objetivo, int cantidad, bool esquivar = false, bool bloquear = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        Objetivo = objetivo;
        CantidadInicial = Restante = cantidad;
        Esquivar = esquivar;
        Bloquear = bloquear;
    }

    public void Registrar(string mensaje) => _recorrido.Add(mensaje);

    public void Absorber(int cantidad, string capa)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        int absorbido = Math.Min(Restante, cantidad);
        Restante -= absorbido;
        Registrar($"{capa}: absorbió {absorbido}; quedan {Restante} de daño.");
    }
}

public interface IManejadorDanio
{
    IManejadorDanio Enlazar(IManejadorDanio siguiente);
    void Manejar(Danio danio);
}

public abstract class ManejadorDanio : IManejadorDanio
{
    private IManejadorDanio? _siguiente;

    public IManejadorDanio Enlazar(IManejadorDanio siguiente)
    {
        _siguiente = siguiente;
        return siguiente;
    }

    public void Manejar(Danio danio)
    {
        Resolver(danio);
        if (danio.Restante > 0) _siguiente?.Manejar(danio);
    }

    protected abstract void Resolver(Danio danio);
}

public sealed class Esquiva : ManejadorDanio
{
    protected override void Resolver(Danio danio)
    {
        if (danio.Esquivar) danio.Absorber(danio.Restante, "Esquiva");
        else danio.Registrar("Esquiva: el ataque continúa.");
    }
}

public sealed class Bloqueo : ManejadorDanio
{
    protected override void Resolver(Danio danio) =>
        danio.Absorber(danio.Bloquear ? danio.Restante / 4 : 0, "Bloqueo");
}

public sealed class Armadura : ManejadorDanio
{
    protected override void Resolver(Danio danio) => danio.Absorber(danio.Objetivo.Estadisticas.Defensa, "Armadura");
}

public sealed class Escudo : ManejadorDanio
{
    protected override void Resolver(Danio danio) =>
        danio.Absorber(danio.Objetivo.AbsorberConEscudo(danio.Restante), "Escudo");
}

public sealed class Vida : ManejadorDanio
{
    protected override void Resolver(Danio danio)
    {
        int aplicado = danio.Objetivo.Estadisticas.AplicarDanio(danio.Restante);
        danio.Registrar($"Vida: perdió {aplicado} PV; conserva {danio.Objetivo.Estadisticas.Vida} PV.");
        danio.Absorber(danio.Restante, "Fin de la cadena");
    }
}

public static class CadenaDefensas
{
    public static IManejadorDanio Crear(IReadOnlyList<CapaDefensa> orden)
    {
        if (orden.Count == 0 || orden[^1] != CapaDefensa.Vida || orden.Distinct().Count() != orden.Count)
            throw new ArgumentException("La cadena debe terminar en Vida y no puede repetir capas.", nameof(orden));
        var manejadores = orden.Select(CrearManejador).ToArray();
        for (int i = 0; i < manejadores.Length - 1; i++) manejadores[i].Enlazar(manejadores[i + 1]);
        return manejadores[0];
    }

    private static IManejadorDanio CrearManejador(CapaDefensa capa) => capa switch
    {
        CapaDefensa.Esquiva => new Esquiva(),
        CapaDefensa.Bloqueo => new Bloqueo(),
        CapaDefensa.Armadura => new Armadura(),
        CapaDefensa.Escudo => new Escudo(),
        CapaDefensa.Vida => new Vida(),
        _ => throw new ArgumentOutOfRangeException(nameof(capa))
    };
}
