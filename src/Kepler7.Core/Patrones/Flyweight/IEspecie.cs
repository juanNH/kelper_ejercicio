namespace Kepler7.Core.Patrones.Flyweight;

// Estado intrínseco e inmutable compartido por unidades y tripulantes.
public interface IEspecie
{
    string Nombre { get; }
    string Color { get; }
    string PlanetaOrigen { get; }
    string Dieta { get; }
}

public sealed record Especie(string Nombre, string Color, string PlanetaOrigen, string Dieta) : IEspecie;
