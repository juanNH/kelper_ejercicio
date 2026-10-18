using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Datos;

public static class CatalogoInicial
{
    // Fuente compartida para preparación, recetas del director y escenario inicial.
    public static FabricaEspecies FabricaEspecies { get; } = new();
    public static IReadOnlyList<IEspecie> Especies { get; } = FabricaEspecies.ObtenerTodas();

    public static Estadisticas EstadisticasPara(RolUnidad rol) => rol switch
    {
        RolUnidad.Soldado => new(100, 24, 4, 3, 2),
        RolUnidad.Medico => new(80, 12, 2, 2, 3),
        RolUnidad.Francotirador => new(70, 32, 1, 5, 2),
        RolUnidad.Comandante => new(110, 20, 5, 3, 2),
        _ => throw new ArgumentOutOfRangeException(nameof(rol))
    };

    public static IReadOnlyList<IItem> EquipamientoPara(RolUnidad rol) => rol == RolUnidad.Medico
        ? [new Arma("Pistola médica", 20, 12), new Pocion("Botiquín", 25, 30)]
        : [new Arma("Rifle de servicio", 40, 24), new Armadura("Traje orbital", 30, 4)];
}
