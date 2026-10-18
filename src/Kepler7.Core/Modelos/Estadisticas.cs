namespace Kepler7.Core.Modelos;

public sealed class Estadisticas
{
    public int VidaMaxima { get; }
    public int Vida { get; private set; }
    public int Ataque { get; }
    public int Defensa { get; }
    public int Alcance { get; }
    public int Movimiento { get; }

    public Estadisticas(int vidaMaxima, int ataque, int defensa, int alcance, int movimiento)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(vidaMaxima);
        ArgumentOutOfRangeException.ThrowIfNegative(ataque);
        ArgumentOutOfRangeException.ThrowIfNegative(defensa);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alcance);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(movimiento);
        VidaMaxima = Vida = vidaMaxima;
        Ataque = ataque;
        Defensa = defensa;
        Alcance = alcance;
        Movimiento = movimiento;
    }

    public int AplicarDanio(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        int aplicado = Math.Min(Vida, cantidad);
        Vida -= aplicado;
        return aplicado;
    }

    public int Curar(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        int recuperado = Math.Min(VidaMaxima - Vida, cantidad);
        Vida += recuperado;
        return recuperado;
    }

    public Estadisticas Clonar()
    {
        var copia = new Estadisticas(VidaMaxima, Ataque, Defensa, Alcance, Movimiento);
        copia.Vida = Vida;
        return copia;
    }
}
