using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Flyweight;

public sealed record ResumenEspecie(IEspecie Especie, int CantidadTripulantes, int InstanciasCompartidas)
{
    public string Nombre => Especie.Nombre;
}

public sealed record DemostracionFlyweight(
    IReadOnlyList<Tripulante> Tripulantes,
    IReadOnlyList<ResumenEspecie> Especies,
    int EspeciesSinFlyweight,
    int EspeciesCreadasEnFabrica)
{
    public int TotalTripulantes => Tripulantes.Count;
    public int EspeciesConFlyweight => Especies.Sum(e => e.InstanciasCompartidas);
    public int ObjetosEspecieEvitados => EspeciesSinFlyweight - EspeciesConFlyweight;
    public decimal PorcentajeReduccion => EspeciesSinFlyweight == 0
        ? 0 : ObjetosEspecieEvitados * 100m / EspeciesSinFlyweight;
}

public sealed class DemostradorFlyweight(FabricaEspecies fabrica)
{
    public const int CantidadTripulantes = 10_000;
    private static readonly string[] Rangos = ["Cadete", "Técnico", "Oficial", "Capitán"];

    public DemostracionFlyweight Generar()
    {
        var compartidos = new Tripulante[CantidadTripulantes];
        var individuales = new Tripulante[CantidadTripulantes];
        IReadOnlyList<string> nombres = fabrica.NombresDisponibles;

        for (int i = 0; i < CantidadTripulantes; i++)
        {
            IEspecie especie = fabrica.ObtenerEspecie(nombres[i % nombres.Count]);
            compartidos[i] = new Tripulante(Guid.NewGuid(), $"Tripulante {i + 1:D5}", Rangos[i % Rangos.Length], especie);

            // Escenario equivalente sin compartir: mismos datos particulares,
            // pero se crea un objeto Especie nuevo para cada tripulante.
            individuales[i] = compartidos[i] with
            {
                Especie = new Especie(especie.Nombre, especie.Color, especie.PlanetaOrigen, especie.Dieta)
            };
        }

        // Especie es un record: su igualdad por valor NO demuestra que se comparta
        // el objeto. Se cuentan referencias reales en ambos escenarios.
        int sinCompartir = individuales.Select(t => t.Especie).Distinct(ReferenceEqualityComparer.Instance).Count();
        var resumen = compartidos.GroupBy(t => t.Especie.Nombre)
            .Select(grupo => new ResumenEspecie(grupo.First().Especie, grupo.Count(),
                grupo.Select(t => t.Especie).Distinct(ReferenceEqualityComparer.Instance).Count()))
            .ToArray();

        // Se conserva la población compartida para inspeccionarla desde la pantalla.
        // El escenario de comparación ya fue contado y puede liberarse.
        return new(Array.AsReadOnly(compartidos), Array.AsReadOnly(resumen), sinCompartir, fabrica.CantidadCreadas);
    }
}
