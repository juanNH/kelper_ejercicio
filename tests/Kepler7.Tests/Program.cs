using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Bridge;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Facade;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.TemplateMethod;
using Kepler7.Core.Patrones.Visitor;
using Kepler7.Core.Servicios;

var casos = new (string Nombre, Action Verificar)[]
{
    ("Builder rechaza una configuración incompleta", () =>
        DebeFallar<InvalidOperationException>(() => new UnidadBuilder().Construir())),
    ("Builder no comparte estadísticas mutables y el director ofrece dos equipos", () =>
    {
        var stats = new Estadisticas(100, 24, 4, 3, 2);
        Unidad unidad = new UnidadBuilder().DefinirIdentidad("Prueba", Equipo.Aliado, CatalogoInicial.Especies[0])
            .SeleccionarRol(RolUnidad.Soldado).ConfigurarEstadisticas(stats)
            .ConfigurarEquipamiento([]).Ubicar(new(1, 1)).Construir();
        stats.AplicarDanio(50);
        Exigir(unidad.Estadisticas.Vida == 100, "Builder conservó una referencia mutable del cliente.");
        var director = new DirectorEquipo();
        Exigir(director.CrearExploracion().Select(u => u.Rol).SequenceEqual(
            new[] { RolUnidad.Soldado, RolUnidad.Medico, RolUnidad.Francotirador }), "Equipo de exploración incorrecto.");
        Exigir(director.CrearAsalto().Count == 3 && director.CrearAsalto()[0].Rol == RolUnidad.Comandante, "Equipo de asalto incorrecto.");
    }),
    ("Prototype aísla ID, vida, estado, inventario y posición", () =>
    {
        var preparacion = new ServicioPreparacion();
        var original = preparacion.ConsultarPlantilla("Soldado");
        Unidad copia = preparacion.ClonarPlantilla("Soldado", new(0, 0));
        Unidad otra = preparacion.ClonarPlantilla("Soldado");
        copia.Estadisticas.AplicarDanio(20);
        copia.CambiarEstado(new Berserk());
        copia.AgregarItem(new Pocion("Extra", 1, 1));
        var despues = preparacion.ConsultarPlantilla("Soldado");
        Exigir(copia.Id != otra.Id && copia.Id != original.Id, "El clon reutilizó un ID.");
        Exigir(despues.Vida == original.Vida && otra.Estadisticas.Vida == original.Vida, "El daño modificó la plantilla.");
        Exigir(despues.Estado == TipoEstado.Normal && otra.Estado.Tipo == TipoEstado.Normal, "El estado se compartió.");
        Exigir(despues.Inventario.Count == original.Inventario.Count && copia.Inventario.Count == original.Inventario.Count + 1,
            "El inventario no es independiente.");
        Exigir(despues.Posicion == original.Posicion && copia.Posicion != original.Posicion, "La posición de la plantilla cambió.");
        Exigir(ReferenceEquals(copia.Especie, otra.Especie), "Los datos inmutables de especie pueden compartirse.");
    }),
    ("Flyweight normaliza claves y crea una sola instancia por especie", () =>
    {
        var fabrica = new FabricaEspecies();
        Exigir(fabrica.CantidadCreadas == 0, "La fábrica creó especies antes de necesitarlas.");
        IEspecie humana = fabrica.ObtenerEspecie(" humana ");
        Exigir(ReferenceEquals(humana, fabrica.ObtenerEspecie("HUMANA")) && fabrica.CantidadCreadas == 1,
            "Mayúsculas o espacios generaron otra instancia.");
        Exigir(humana.Nombre == "Humana" && humana.Color == "#5CA9E6" && humana.PlanetaOrigen == "Tierra"
            && humana.Dieta == "Omnívora", "Se perdieron los datos intrínsecos.");
        DebeFallar<ArgumentException>(() => fabrica.ObtenerEspecie(" "));
        DebeFallar<ArgumentException>(() => fabrica.ObtenerEspecie("Desconocida"));
        Exigir(fabrica.CantidadCreadas == 1, "Una búsqueda inválida modificó la caché.");
        var todas = fabrica.ObtenerTodas();
        Exigir(todas.Count == 5 && fabrica.CantidadCreadas == 5
            && todas.Distinct(ReferenceEqualityComparer.Instance).Count() == 5, "El catálogo no contiene exactamente cinco instancias.");
        Exigir(todas.All(e => ReferenceEquals(e, fabrica.ObtenerEspecie(e.Nombre))), "El catálogo devuelve copias.");
    }),
    ("Flyweight conserva la identidad ante solicitudes simultáneas", () =>
    {
        var fabrica = new FabricaEspecies();
        var referencias = new IEspecie[5_000];
        Parallel.For(0, referencias.Length, i => referencias[i] = fabrica.ObtenerEspecie(
            fabrica.NombresDisponibles[i % fabrica.NombresDisponibles.Count]));
        Exigir(fabrica.CantidadCreadas == 5, "Las solicitudes concurrentes crearon duplicados.");
        Exigir(referencias.GroupBy(e => e.Nombre).All(g => g.All(e => ReferenceEquals(e, g.First()))),
            "Dos solicitudes recibieron objetos distintos para la misma especie.");
    }),
    ("Flyweight genera 10.000 tripulantes independientes y compara referencias reales", () =>
    {
        var fabrica = new FabricaEspecies();
        var demostrador = new DemostradorFlyweight(fabrica);
        DemostracionFlyweight resultado = demostrador.Generar();
        Exigir(resultado.TotalTripulantes == 10_000 && resultado.Tripulantes.Select(t => t.Id).Distinct().Count() == 10_000,
            "La población no tiene 10.000 identidades independientes.");
        Exigir(resultado.Tripulantes.Select(t => t.Nombre).Distinct().Count() == 10_000, "Se repitieron los nombres particulares.");
        Exigir(resultado.Especies.Count == 5 && resultado.Especies.All(e => e.CantidadTripulantes == 2_000 && e.InstanciasCompartidas == 1),
            "No se repartieron 2.000 tripulantes por cada objeto compartido.");
        Exigir(resultado.Tripulantes.Select(t => t.Especie).Distinct(ReferenceEqualityComparer.Instance).Count() == 5,
            "La población contiene más de cinco objetos Especie.");
        Exigir(resultado.EspeciesConFlyweight == 5 && resultado.EspeciesSinFlyweight == 10_000
            && resultado.ObjetosEspecieEvitados == 9_995 && resultado.PorcentajeReduccion == 99.95m,
            "La comparación de objetos es incorrecta.");
        Tripulante primero = resultado.Tripulantes[0];
        Tripulante otroDeLaEspecie = resultado.Tripulantes[5];
        Tripulante cambiado = primero with { Nombre = "Nombre propio nuevo", Rango = "Rango nuevo" };
        Exigir(cambiado.Nombre != otroDeLaEspecie.Nombre && cambiado.Rango != otroDeLaEspecie.Rango
            && ReferenceEquals(cambiado.Especie, otroDeLaEspecie.Especie) && primero.Nombre == "Tripulante 00001",
            "Los datos particulares se confundieron con el estado compartido.");
        DemostracionFlyweight repetida = demostrador.Generar();
        Exigir(repetida.TotalTripulantes == 10_000 && fabrica.CantidadCreadas == 5
            && ReferenceEquals(repetida.Tripulantes[0].Especie, primero.Especie)
            && repetida.Tripulantes[0].Id != primero.Id, "Regenerar acumuló especies o reutilizó identidades de tripulantes.");
    }),
    ("Preparación usa los mismos flyweights en unidades, plantillas y población", () =>
    {
        var servicio = new ServicioPreparacion();
        Unidad unidad = servicio.CrearUnidad(new("Prueba Flyweight", RolUnidad.Soldado,
            new Especie(" humana ", "ignorado", "ignorado", "ignorado"), new(100, 24, 4, 3, 2), new(0, 0), []));
        IEspecie humana = servicio.Especies.Single(e => e.Nombre == "Humana");
        Exigir(ReferenceEquals(unidad.Especie, humana) && ReferenceEquals(servicio.ClonarPlantilla("Soldado").Especie, humana),
            "Una unidad o plantilla eludió la fábrica.");
        Exigir(servicio.CrearEscuadron("Exploración").All(u => ReferenceEquals(u.Especie, humana)), "El director duplicó especies.");
        DemostracionFlyweight poblacion = servicio.GenerarDemostracionTripulantes();
        Exigir(poblacion.Tripulantes.Where(t => t.Especie.Nombre == "Humana").All(t => ReferenceEquals(t.Especie, humana))
            && poblacion.EspeciesCreadasEnFabrica == 5, "La demostración usa otra caché diferente a la preparación.");
    }),
    ("Bridge permite nueve combinaciones y cambiar cifrado sin cambiar mensaje", () =>
    {
        var servicio = new ServicioComunicaciones();
        int combinaciones = 0;
        foreach (TipoMensaje tipo in Enum.GetValues<TipoMensaje>())
        foreach (TipoCifrado cifrado in Enum.GetValues<TipoCifrado>())
        {
            Exigir(!string.IsNullOrWhiteSpace(servicio.Enviar(tipo, cifrado, "Órbita estable").Resultado), "Resultado vacío.");
            combinaciones++;
        }
        var mensaje = new MensajeTexto(new SinCifrado());
        Exigir(mensaje.Enviar("Kepler").Resultado == "Kepler", "SinCifrado modificó el contenido.");
        mensaje.Cifrado = new CifradoAES();
        Exigir(mensaje.Enviar("Kepler").Resultado.StartsWith("IV="), "No se intercambió el cifrado.");
        Exigir(combinaciones == 9, "Falta una combinación.");
        DebeFallar<ArgumentException>(() => servicio.Enviar(TipoMensaje.Texto, TipoCifrado.AES, " "));
    }),
    ("Facade cambia seis subsistemas y apaga energía al final", () =>
    {
        var central = new ControlCentral();
        Exigir(central.IniciarTurno().Count == 6 && central.ConsultarEstado().All(s => s.Estado != "Apagado"), "Inicio incompleto.");
        Exigir(central.EntrarEnEmergencia().Count == 6, "Emergencia incompleta.");
        var apagado = central.ApagarEstacion();
        Exigir(apagado[0].StartsWith("Defensa") && apagado[^1].StartsWith("Energía"), "Orden de apagado incorrecto.");
        Exigir(central.ConsultarEstado().All(s => s.Estado == "Apagado"), "Quedó un subsistema activo.");
    }),
    ("State cambia acciones sin modificar el cliente", () =>
    {
        Unidad unidad = CrearUnidad("Aliado", Equipo.Aliado, new(1, 1));
        Exigir(unidad.Atacar() == 24, "Ataque normal incorrecto.");
        unidad.CambiarEstado(new Aturdido());
        Exigir(unidad.Atacar() == 0 && !unidad.Mover(new(1, 2)), "Aturdido actuó.");
        unidad.CambiarEstado(new Berserk());
        Exigir(unidad.Atacar() == 36 && unidad.RecibirDanio(24) == 30, "Berserk no modificó el comportamiento.");
        unidad.CambiarEstado(new Envenenado());
        Exigir(unidad.Atacar() == 18 && unidad.RecibirDanio(24) == 26 && unidad.RecibirDanio(0) == 0, "Envenenado incorrecto.");
    }),
    ("Chain aplica armadura, escudo y vida sin daño negativo", () =>
    {
        Unidad unidad = CrearUnidad("Objetivo", Equipo.Enemigo, new(3, 1));
        var danio = new Danio(unidad, 24);
        CadenaDefensas.Crear(ConfiguracionDefensa.Predeterminada.Orden).Manejar(danio);
        Exigir(unidad.Estadisticas.Vida == 90 && unidad.Escudo == 0 && danio.Restante == 0, "Resolución incorrecta.");
        Exigir(danio.Recorrido.Any(s => s.StartsWith("Vida:")), "No se registró el recorrido.");
        var minimo = new Danio(unidad, 1);
        CadenaDefensas.Crear([CapaDefensa.Armadura, CapaDefensa.Vida]).Manejar(minimo);
        Exigir(unidad.Estadisticas.Vida == 90, "La defensa produjo daño negativo o adicional.");
    }),
    ("Chain permite quitar y reordenar capas; esquiva detiene la cadena", () =>
    {
        Unidad uno = CrearUnidad("Uno", Equipo.Enemigo, new(3, 1));
        Unidad dos = uno.Clonar();
        CadenaDefensas.Crear([CapaDefensa.Bloqueo, CapaDefensa.Armadura, CapaDefensa.Vida]).Manejar(new(uno, 24, bloquear: true));
        CadenaDefensas.Crear([CapaDefensa.Armadura, CapaDefensa.Bloqueo, CapaDefensa.Vida]).Manejar(new(dos, 24, bloquear: true));
        Exigir(uno.Estadisticas.Vida == 86 && dos.Estadisticas.Vida == 85, "Cambiar el orden no produjo la resolución esperada.");
        var esquivado = new Danio(dos, 100, esquivar: true);
        CadenaDefensas.Crear(ConfiguracionDefensa.Predeterminada.Orden).Manejar(esquivado);
        Exigir(dos.Estadisticas.Vida == 85 && dos.Escudo == 10 && esquivado.Recorrido.Count == 1, "La cadena continuó después de esquivar.");
        DebeFallar<ArgumentException>(() => CadenaDefensas.Crear([CapaDefensa.Vida, CapaDefensa.Escudo]));
    }),
    ("Combate valida movimiento, ocupación y estado antes de mutar", () =>
    {
        var (servicio, aliado, enemigo) = CrearEscenario();
        Exigir(!servicio.Mover(aliado.Id, enemigo.Posicion).Exito, "Se movió a una celda ocupada.");
        Exigir(!servicio.Mover(aliado.Id, new(-1, 1)).Exito, "Salió del tablero.");
        Exigir(!servicio.Mover(aliado.Id, new(7, 7)).Exito, "Superó su movimiento.");
        servicio.CambiarEstado(aliado.Id, TipoEstado.Aturdido);
        Exigir(!servicio.Mover(aliado.Id, new(1, 2)).Exito && aliado.Posicion == new Posicion(1, 1), "Aturdido se desplazó.");
        servicio.CambiarEstado(aliado.Id, TipoEstado.Normal);
        Exigir(servicio.Mover(aliado.Id, new(1, 2)).Exito, "No se pudo mover una unidad normal.");
        Exigir(!servicio.IncorporarUnidad(aliado).Exito, "Se duplicó una unidad de la misión.");
    }),
    ("Combate integra State y Chain y rechaza acciones inválidas sin efectos", () =>
    {
        var (servicio, aliado, enemigo) = CrearEscenario();
        Exigir(!servicio.Atacar(aliado.Id, aliado.Id, ConfiguracionDefensa.Predeterminada).Exito, "Permitió fuego aliado.");
        servicio.CambiarEstado(aliado.Id, TipoEstado.Aturdido);
        Exigir(!servicio.Atacar(aliado.Id, enemigo.Id, ConfiguracionDefensa.Predeterminada).Exito, "Atacó aturdido.");
        Exigir(enemigo.Estadisticas.Vida == 100 && enemigo.Escudo == 10, "Un ataque inválido tuvo efectos.");
        servicio.CambiarEstado(aliado.Id, TipoEstado.Normal);
        servicio.CambiarEstado(enemigo.Id, TipoEstado.Berserk);
        var resultado = servicio.Atacar(aliado.Id, enemigo.Id, ConfiguracionDefensa.Predeterminada);
        Exigir(resultado.Exito && enemigo.Estadisticas.Vida == 84 && enemigo.Escudo == 0, "Falló State → Chain → Vida.");
    }),
    ("Template Method conserva el orden en la base compartida", () =>
    {
        var etapas = new List<string>();
        new TurnoDePrueba(CrearUnidad("Prueba", Equipo.Aliado, new(1, 1)), etapas.Add).EjecutarTurno();
        Exigir(etapas.Count == 4 && etapas[0].StartsWith("Inicio") && etapas[1] == "Decisión"
            && etapas[2] == "Acción" && etapas[3].StartsWith("Fin"), "El orden del turno cambió.");
    }),
    ("Core no referencia Windows Forms", () =>
        Exigir(!typeof(Unidad).Assembly.GetReferencedAssemblies().Any(a => a.Name == "System.Windows.Forms"), "Core depende de controles visuales."))
};

int fallos = 0;
var pruebas = casos.Concat(TemplateMethodTests.Casos()).Concat(Estudiante1AuditTests.Casos()).ToArray();
foreach (var caso in pruebas)
{
    try { caso.Verificar(); Console.WriteLine($"OK  {caso.Nombre}"); }
    catch (Exception ex) { fallos++; Console.Error.WriteLine($"ERROR  {caso.Nombre}: {ex.Message}"); }
}
Console.WriteLine($"{pruebas.Length - fallos}/{pruebas.Length} comprobaciones correctas.");
return fallos == 0 ? 0 : 1;

static void Exigir(bool condicion, string mensaje)
{
    if (!condicion) throw new InvalidOperationException(mensaje);
}

static void DebeFallar<T>(Action accion) where T : Exception
{
    try { accion(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Se esperaba {typeof(T).Name}.");
}

static Unidad CrearUnidad(string nombre, Equipo equipo, Posicion posicion) => new UnidadBuilder()
    .DefinirIdentidad(nombre, equipo, CatalogoInicial.Especies[0]).SeleccionarRol(RolUnidad.Soldado)
    .ConfigurarEstadisticas(new(100, 24, 4, 3, 2)).ConfigurarEquipamiento([]).Ubicar(posicion).Construir();

static (ServicioCombate Servicio, Unidad Aliado, Unidad Enemigo) CrearEscenario()
{
    var mision = new Mision("Prueba", "Verificar el contrato de combate");
    Unidad aliado = CrearUnidad("Aliado", Equipo.Aliado, new(1, 1));
    Unidad enemigo = CrearUnidad("Enemigo", Equipo.Enemigo, new(3, 1));
    mision.AgregarUnidad(aliado);
    mision.AgregarUnidad(enemigo);
    return (new(mision), aliado, enemigo);
}

sealed class TurnoDePrueba(Unidad unidad, Action<string> registrar) : TurnoUnidad(unidad, registrar)
{
    protected override void Decidir() => Registrar("Decisión");
    protected override void Actuar() => Registrar("Acción");
}
