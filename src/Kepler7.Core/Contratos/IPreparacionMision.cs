using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Contratos;

public sealed record ConfiguracionUnidad(string Nombre, RolUnidad Rol, IEspecie Especie,
    Estadisticas Estadisticas, Posicion Posicion, IReadOnlyList<IItem> Equipamiento);

public sealed record ConfiguracionVariante(string Nombre, Estadisticas Estadisticas,
    Posicion Posicion, IReadOnlyList<IItem> Equipamiento);

public interface IPreparacionMision
{
    IReadOnlyList<IEspecie> Especies { get; }
    IReadOnlyList<string> Plantillas { get; }
    IReadOnlyList<string> Escuadrones { get; }
    Unidad CrearUnidad(ConfiguracionUnidad configuracion);
    Unidad ClonarPlantilla(string id, Posicion? posicion = null);
    Unidad CrearVariante(string id, ConfiguracionVariante configuracion);
    VistaUnidad ConsultarPlantilla(string id);
    IReadOnlyList<Unidad> CrearEscuadron(string nombre, Posicion? origen = null);
    DemostracionFlyweight GenerarDemostracionTripulantes();
}
