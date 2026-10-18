using Kepler7.Core.Contratos;
using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.TemplateMethod;
using Kepler7.Core.Patrones.Visitor;
using Kepler7.Core.Servicios;

internal static class Estudiante1AuditTests
{
    public static (string Nombre, Action Verificar)[] Casos() =>
    [
        ("Auditoría Builder: roles, validaciones y productos independientes", () =>
        {
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
                Exigir(Crear("Unidad", rol, Equipo.Aliado, new(0, 0)).Rol == rol, "No se puede construir un rol requerido.");
            var estadisticas = new Estadisticas(100, 24, 4, 3, 2);
            var item = new ItemMutable("Original");
            var builder = new UnidadBuilder().DefinirIdentidad("Prueba", Equipo.Aliado, CatalogoInicial.Especies[0])
                .SeleccionarRol(RolUnidad.Soldado).ConfigurarEstadisticas(estadisticas)
                .ConfigurarEquipamiento([item]).Ubicar(new(0, 0));
            item.Nombre = "Cambiado fuera del builder";
            estadisticas.AplicarDanio(20);
            Unidad uno = builder.Construir();
            Unidad dos = builder.Construir();
            uno.Estadisticas.AplicarDanio(10);
            ((ItemMutable)uno.Inventario[0]).Nombre = "Cambio de una unidad";
            Exigir(uno.Id != dos.Id && dos.Estadisticas.Vida == 100 && dos.Inventario[0].Nombre == "Original",
                "El builder comparte datos mutables del cliente o entre productos.");
            DebeFallar<ArgumentException>(() => new UnidadBuilder().DefinirIdentidad(" ", Equipo.Aliado, CatalogoInicial.Especies[0]));
            DebeFallar<ArgumentException>(() => new UnidadBuilder().SeleccionarRol((RolUnidad)999));
            DebeFallar<ArgumentException>(() => new UnidadBuilder().ConfigurarEstadisticas(null!));
            DebeFallar<ArgumentException>(() => new UnidadBuilder().ConfigurarEquipamiento(null!));
            DebeFallar<ArgumentException>(() => new UnidadBuilder().ConfigurarEquipamiento([null!]));
            DebeFallar<ArgumentException>(() => new UnidadBuilder().Ubicar(new(8, 0)));
        }),
        ("Auditoría Builder: dos composiciones completas con origen configurable", () =>
        {
            var preparacion = new ServicioPreparacion();
            foreach (string nombre in preparacion.Escuadrones)
            {
                var equipo = preparacion.CrearEscuadron(nombre, new Posicion(4, 0));
                Exigir(equipo.Count == 3 && equipo.Select(u => u.Id).Distinct().Count() == 3
                    && equipo.Select(u => u.Posicion).SequenceEqual(new[] { new Posicion(4, 0), new(4, 1), new(4, 2) }),
                    "La composición no se ubicó desde el origen elegido.");
                var mision = new Mision("Preparación", "Incorporar un escuadrón completo");
                foreach (Unidad unidad in equipo) mision.AgregarUnidad(unidad);
                Exigir(mision.Unidades.Count == 3 && equipo.All(u => u.Inventario.Count > 0), "El escuadrón no está listo para usar.");
            }
            DebeFallar<ArgumentException>(() => preparacion.CrearEscuadron("Exploración", new(4, 7)));
            DebeFallar<ArgumentException>(() => preparacion.CrearEscuadron("Asalto", new(-1, 0)));
            DebeFallar<ArgumentException>(() => preparacion.CrearEscuadron("Desconocido"));
        }),
        ("Auditoría Prototype: plantillas aliadas y enemigas para los cuatro roles", () =>
        {
            var preparacion = new ServicioPreparacion();
            Exigir(preparacion.Plantillas.Count == 8, "Faltan plantillas de alguno de los equipos o roles.");
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
            foreach (Equipo equipo in Enum.GetValues<Equipo>())
            {
                string id = equipo == Equipo.Aliado ? rol.ToString() : $"Enemigo · {rol}";
                VistaUnidad original = preparacion.ConsultarPlantilla(id);
                Unidad copia = preparacion.ClonarPlantilla(id, new(5, 5));
                Exigir(copia.Rol == rol && copia.Equipo == equipo && copia.Id != original.Id && copia.Posicion == new Posicion(5, 5),
                    "La copia no conserva el tipo/equipo o reutiliza la identidad de la plantilla.");
                var mision = new Mision("Prototipos", "Usar copias en una misión");
                mision.AgregarUnidad(copia);
                Exigir(mision.Unidades[0].Id == copia.Id, "La copia no puede utilizarse en una misión.");
            }
            var intruso = preparacion.ClonarPlantilla("Enemigo · Soldado", new(3, 1));
            Exigir(intruso.Nombre == "Intruso orbital" && intruso.Estadisticas.Vida == 100 && intruso.Estadisticas.Ataque == 24,
                "Cambió la configuración del enemigo usado por la práctica inicial.");
        }),
        ("Auditoría Prototype: copia profunda de estado, estadísticas, inventario y escudo", () =>
        {
            Unidad plantilla = Crear("Base", RolUnidad.Soldado, Equipo.Aliado, new(2, 2), [new ItemMutable("Equipo original")]);
            plantilla.CambiarEstado(new EstadoMutable(3));
            plantilla.Estadisticas.AplicarDanio(15);
            plantilla.AbsorberConEscudo(4);
            var registro = new RegistroPrototipos();
            registro.Registrar("Base", plantilla);
            plantilla.Estadisticas.AplicarDanio(20);
            ((EstadoMutable)plantilla.Estado).Turnos = 99;
            ((ItemMutable)plantilla.Inventario[0]).Nombre = "Cambio externo";
            Unidad una = registro.Clonar("Base");
            Unidad otra = registro.Clonar("Base");
            Exigir(una.Id != otra.Id && !ReferenceEquals(una.Estado, otra.Estado)
                && !ReferenceEquals(una.Estadisticas, otra.Estadisticas) && !ReferenceEquals(una.Inventario[0], otra.Inventario[0])
                && ReferenceEquals(una.Especie, otra.Especie), "No se separaron los datos mutables o se duplicó la especie inmutable.");
            una.Estadisticas.AplicarDanio(10);
            una.AbsorberConEscudo(6);
            una.Mover(new(3, 2));
            ((EstadoMutable)una.Estado).Turnos = 1;
            ((ItemMutable)una.Inventario[0]).Nombre = "Cambio del clon";
            Exigir(otra.Estadisticas.Vida == 85 && otra.Escudo == 6 && otra.Posicion == new Posicion(2, 2)
                && ((EstadoMutable)otra.Estado).Turnos == 3 && otra.Inventario[0].Nombre == "Equipo original",
                "Modificar un clon alteró otra copia.");
            Exigir(registro.Consultar("Base").Vida == 85 && registro.Consultar("Base").Escudo == 6,
                "Modificar el original del cliente o un clon alteró la plantilla protegida.");
        }),
        ("Auditoría Prototype: variantes editables conservan tipo y no alteran la plantilla", () =>
        {
            var preparacion = new ServicioPreparacion();
            foreach (string id in new[] { "Soldado", "Enemigo · Soldado" })
            {
                VistaUnidad original = preparacion.ConsultarPlantilla(id);
                Unidad copiaSimple = preparacion.ClonarPlantilla(id);
                var estadisticas = new Estadisticas(150, 35, 7, 4, 3);
                estadisticas.AplicarDanio(25);
                var item = new ItemMutable("Equipo variante");
                Unidad variante = preparacion.CrearVariante(id, new("  Variante propia  ", estadisticas, new(5, 5), [item]));
                item.Nombre = "Cambio del cliente";
                estadisticas.AplicarDanio(50);
                VistaUnidad despues = preparacion.ConsultarPlantilla(id);
                Exigir(variante.Id != original.Id && variante.Id != copiaSimple.Id && variante.Nombre == "Variante propia"
                    && variante.Rol == original.Rol && variante.Equipo == original.Equipo
                    && ReferenceEquals(variante.Especie, copiaSimple.Especie), "La variante cambió la identidad o el tipo compartido incorrectamente.");
                Exigir(variante.Estadisticas.Vida == 125 && variante.Estadisticas.VidaMaxima == 150
                    && variante.Estadisticas.Ataque == 35 && variante.Estadisticas.Defensa == 7
                    && variante.Estadisticas.Alcance == 4 && variante.Estadisticas.Movimiento == 3
                    && variante.Inventario.Single().Nombre == "Equipo variante" && variante.Posicion == new Posicion(5, 5),
                    "La variante ignoró ajustes o conservó referencias mutables del cliente.");
                Exigir(despues.Id == original.Id && despues.Nombre == original.Nombre && despues.Vida == original.Vida
                    && despues.Ataque == original.Ataque && despues.Posicion == original.Posicion
                    && despues.Inventario.SequenceEqual(original.Inventario), "La variante alteró la plantilla original.");
            }
        }),
        ("Auditoría Prototype: entradas inválidas se rechazan sin modificar el registro", () =>
        {
            var preparacion = new ServicioPreparacion();
            VistaUnidad original = preparacion.ConsultarPlantilla("Soldado");
            DebeFallar<ArgumentException>(() => preparacion.CrearVariante("Soldado", new(" ", new(100, 24, 4, 3, 2), new(0, 0), [])));
            DebeFallar<ArgumentException>(() => preparacion.CrearVariante("Soldado", new("Variante", new(100, 24, 4, 3, 2), new(8, 0), [])));
            DebeFallar<ArgumentException>(() => preparacion.CrearVariante("Soldado", null!));
            DebeFallar<ArgumentException>(() => preparacion.CrearVariante("Desconocida", new("Variante", new(100, 24, 4, 3, 2), new(0, 0), [])));
            DebeFallar<ArgumentException>(() => preparacion.ClonarPlantilla(" "));
            Exigir(preparacion.ConsultarPlantilla("Soldado").Id == original.Id && preparacion.Plantillas.Count == 8,
                "Una operación inválida modificó las plantillas.");
        }),
        ("Auditoría preparación: vista previa completa y equipamiento aislado del registro", () =>
        {
            var registro = new RegistroPrototipos();
            registro.Registrar("Vista", Crear("Vista", RolUnidad.Soldado, Equipo.Enemigo, new(2, 2), [new ItemMutable("Original")]));
            VistaUnidad vista = registro.Consultar("Vista");
            Exigir(vista.Equipo == Equipo.Enemigo && vista.Vida == 100 && vista.VidaMaxima == 100
                && vista.Ataque == 24 && vista.Defensa == 4 && vista.Alcance == 3 && vista.Movimiento == 2
                && vista.Escudo == 10 && vista.Estado == TipoEstado.Normal && vista.Posicion == new Posicion(2, 2),
                "La vista previa omite datos de la unidad completa.");
            ((ItemMutable)vista.Equipamiento[0]).Nombre = "Cambio desde una vista";
            Exigir(registro.Consultar("Vista").Equipamiento[0].Nombre == "Original", "La vista expone el inventario mutable protegido.");
        }),
        ("Auditoría Template Method: los cuatro roles ejecutan turnos de ambos equipos", () =>
        {
            foreach (Equipo equipo in Enum.GetValues<Equipo>())
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
            {
                Unidad actor = Crear("Actor", rol, equipo, new(1, 1));
                Unidad aliado = Crear("Apoyo", RolUnidad.Soldado, equipo, new(1, 3));
                aliado.Estadisticas.AplicarDanio(30);
                Unidad oponente = Crear("Oponente", RolUnidad.Soldado,
                    equipo == Equipo.Aliado ? Equipo.Enemigo : Equipo.Aliado, new(2, 1));
                var mision = new Mision("Turnos", "Turnos manuales de todas las unidades");
                foreach (Unidad unidad in new[] { actor, aliado, oponente }) mision.AgregarUnidad(unidad);
                ResultadoTurno resultado = new ServicioTurnos(new ServicioCombate(mision)).EjecutarTurno(actor.Id);
                Exigir(resultado.Exito && resultado.Rol == rol && resultado.UnidadId == actor.Id
                    && resultado.Eventos.Select(e => e.Etapa).Distinct().SequenceEqual(Enum.GetValues<EtapaTurno>()),
                    $"El turno {rol} de {equipo} no completa las cuatro etapas.");
            }
        })
    ];

    private static Unidad Crear(string nombre, RolUnidad rol, Equipo equipo, Posicion posicion, IReadOnlyList<IItem>? items = null) =>
        new UnidadBuilder().DefinirIdentidad(nombre, equipo, CatalogoInicial.Especies[0]).SeleccionarRol(rol)
            .ConfigurarEstadisticas(CatalogoInicial.EstadisticasPara(rol)).ConfigurarEquipamiento(items ?? [])
            .Ubicar(posicion).Construir();

    private static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion) throw new InvalidOperationException(mensaje);
    }

    private static void DebeFallar<T>(Action accion) where T : Exception
    {
        try { accion(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Se esperaba {typeof(T).Name}.");
    }

    private sealed class ItemMutable(string nombre) : IItem
    {
        public string Nombre { get; set; } = nombre;
        public int Precio => 1;
        public IItem Copiar() => new ItemMutable(Nombre);
        public void Aceptar(IVisitor visitante) => throw new NotSupportedException("Objeto de prueba para copia profunda.");
    }

    private sealed class EstadoMutable(int turnos) : IEstadoPersonaje
    {
        public int Turnos { get; set; } = turnos;
        public TipoEstado Tipo => TipoEstado.Normal;
        public int Atacar(Unidad personaje) => personaje.Estadisticas.Ataque;
        public bool Mover(Unidad personaje, Posicion destino) => true;
        public int RecibirDanio(Unidad personaje, int cantidad) => cantidad;
        public IEstadoPersonaje Clonar() => new EstadoMutable(Turnos);
    }
}
