using Kepler7.Core.Contratos;
using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Core.Servicios;

public sealed class ServicioPreparacion : IPreparacionMision
{
    private readonly DirectorEquipo _director = new();
    private readonly RegistroPrototipos _registro = new();
    private readonly FabricaEspecies _fabricaEspecies = CatalogoInicial.FabricaEspecies;
    public IReadOnlyList<IEspecie> Especies => CatalogoInicial.Especies;
    public IReadOnlyList<string> Plantillas => _registro.Identificadores;
    public IReadOnlyList<string> Escuadrones => ["Exploración", "Asalto"];

    public ServicioPreparacion()
    {
        foreach (var unidad in _director.CrearExploracion())
            _registro.Registrar(unidad.Rol.ToString(), unidad);
        _registro.Registrar("Comandante", _director.CrearAsalto()[0]);
        foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
        {
            Unidad enemigo = new UnidadBuilder()
                .DefinirIdentidad(rol == RolUnidad.Soldado ? "Intruso orbital" : $"Intruso {rol}",
                    Equipo.Enemigo, _fabricaEspecies.ObtenerEspecie("Marciana"))
                .SeleccionarRol(rol).ConfigurarEstadisticas(CatalogoInicial.EstadisticasPara(rol))
                .ConfigurarEquipamiento(CatalogoInicial.EquipamientoPara(rol))
                .Ubicar(new Posicion(6, (int)rol + 1)).Construir();
            _registro.Registrar($"Enemigo · {rol}", enemigo);
        }
    }

    public Unidad CrearUnidad(ConfiguracionUnidad configuracion) => new UnidadBuilder()
        .DefinirIdentidad(configuracion.Nombre, Equipo.Aliado,
            _fabricaEspecies.ObtenerEspecie(configuracion.Especie.Nombre))
        .SeleccionarRol(configuracion.Rol)
        .ConfigurarEstadisticas(configuracion.Estadisticas)
        .ConfigurarEquipamiento(configuracion.Equipamiento)
        .Ubicar(configuracion.Posicion)
        .Construir();

    public Unidad ClonarPlantilla(string id, Posicion? posicion = null)
    {
        Unidad copia = _registro.Clonar(id);
        if (posicion is { } destino)
        {
            if (!destino.EstaDentro(8, 8)) throw new ArgumentOutOfRangeException(nameof(posicion));
            if (!copia.Mover(destino)) throw new InvalidOperationException("La plantilla no admite cambiar su posición.");
        }
        return copia;
    }
    public VistaUnidad ConsultarPlantilla(string id) => _registro.Consultar(id);

    public Unidad CrearVariante(string id, ConfiguracionVariante configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        return _registro.CrearVariante(id, configuracion.Nombre, configuracion.Estadisticas,
            configuracion.Posicion, configuracion.Equipamiento);
    }

    public IReadOnlyList<Unidad> CrearEscuadron(string nombre, Posicion? origen = null) => nombre switch
    {
        "Exploración" => _director.CrearExploracion(origen),
        "Asalto" => _director.CrearAsalto(origen),
        _ => throw new ArgumentException("Escuadrón desconocido.", nameof(nombre))
    };

    public DemostracionFlyweight GenerarDemostracionTripulantes() =>
        new DemostradorFlyweight(_fabricaEspecies).Generar();
}
