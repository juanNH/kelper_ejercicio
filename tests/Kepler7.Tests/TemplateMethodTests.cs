using Kepler7.Core.Contratos;
using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.TemplateMethod;
using Kepler7.Core.Servicios;

internal static class TemplateMethodTests
{
    public static (string Nombre, Action Verificar)[] Casos() =>
    [
        ("Template Method conserva cuatro etapas y el método no es virtual en los cuatro roles", () =>
        {
            Exigir(!typeof(TurnoUnidad).GetMethod(nameof(TurnoUnidad.EjecutarTurno))!.IsVirtual,
                "Las variantes pueden reemplazar el orden del algoritmo.");
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
            {
                Unidad actor = Crear("Actor", rol, Equipo.Aliado, new(1, 1));
                Unidad aliado = Crear("Aliado", RolUnidad.Soldado, Equipo.Aliado, new(1, 3));
                aliado.Estadisticas.AplicarDanio(30);
                Unidad enemigo = Crear("Enemigo", RolUnidad.Soldado, Equipo.Enemigo, new(3, 1));
                var servicio = new ServicioTurnos(Escenario(actor, aliado, enemigo));
                ResultadoTurno resultado = servicio.EjecutarTurno(actor.Id);
                Exigir(resultado.Exito && resultado.Rol == rol, $"No se ejecutó el turno {rol}.");
                VerificarSecuencia(resultado);
            }
        }),
        ("Turno Soldado ataca al enemigo vivo más cercano y conserva su posición", () =>
        {
            Unidad actor = Crear("Soldado", RolUnidad.Soldado, Equipo.Aliado, new(1, 1));
            Unidad cercano = Crear("Cercano", RolUnidad.Soldado, Equipo.Enemigo, new(2, 1));
            Unidad lejano = Crear("Lejano", RolUnidad.Soldado, Equipo.Enemigo, new(3, 1));
            var servicio = new ServicioTurnos(Escenario(actor, cercano, lejano));
            ResultadoTurno resultado = servicio.EjecutarTurno(actor.Id);
            Exigir(resultado.Exito && cercano.Estadisticas.Vida == 90 && lejano.Estadisticas.Vida == 100,
                "El soldado no aplicó el ataque al objetivo más cercano.");
            Exigir(actor.Posicion == new Posicion(1, 1) && resultado.Eventos.Any(e => e.Mensaje.StartsWith("Vida:")),
                "El ataque no conservó la posición o perdió el registro del servicio de combate.");
        }),
        ("Turno Soldado avanza una sola celda o espera si está rodeado", () =>
        {
            Unidad actor = Crear("Soldado", RolUnidad.Soldado, Equipo.Aliado, new(1, 1));
            Unidad enemigo = Crear("Enemigo", RolUnidad.Soldado, Equipo.Enemigo, new(5, 1));
            var servicio = new ServicioTurnos(Escenario(actor, enemigo));
            servicio.EjecutarTurno(actor.Id);
            Exigir(actor.Posicion == new Posicion(2, 1) && enemigo.Estadisticas.Vida == 100 && enemigo.Escudo == 10,
                "El soldado combinó movimiento y ataque o avanzó más de una celda.");

            actor = Crear("Rodeado", RolUnidad.Soldado, Equipo.Aliado, new(1, 1));
            var obstaculos = new[] { new Posicion(0, 1), new(2, 1), new(1, 0), new(1, 2) }
                .Select((p, i) => Crear($"Aliado {i}", RolUnidad.Soldado, Equipo.Aliado, p));
            servicio = new ServicioTurnos(Escenario(new[] { actor, enemigo }.Concat(obstaculos).ToArray()));
            Exigir(servicio.EjecutarTurno(actor.Id).Exito && actor.Posicion == new Posicion(1, 1), "El soldado atravesó una celda ocupada.");
        }),
        ("Turno Médico prioriza al aliado con mayor pérdida de vida y puede curarse", () =>
        {
            Unidad actor = Crear("Médico", RolUnidad.Medico, Equipo.Aliado, new(1, 1));
            Unidad grave = Crear("Grave", RolUnidad.Soldado, Equipo.Aliado, new(2, 1));
            Unidad leve = Crear("Leve", RolUnidad.Soldado, Equipo.Aliado, new(1, 2));
            grave.Estadisticas.AplicarDanio(45);
            leve.Estadisticas.AplicarDanio(20);
            var servicio = new ServicioTurnos(Escenario(actor, grave, leve));
            servicio.EjecutarTurno(actor.Id);
            Exigir(grave.Estadisticas.Vida == 80 && leve.Estadisticas.Vida == 80, "No se priorizó la mayor pérdida de vida.");
            actor.Estadisticas.AplicarDanio(70);
            servicio.EjecutarTurno(actor.Id);
            Exigir(actor.Estadisticas.Vida == 35 && grave.Estadisticas.Vida == 80, "El médico no pudo priorizar su propia herida.");
        }),
        ("Turno Médico respeta la vida máxima y no resucita ni cura enemigos", () =>
        {
            Unidad actor = Crear("Médico", RolUnidad.Medico, Equipo.Aliado, new(1, 1));
            Unidad paciente = Crear("Paciente", RolUnidad.Soldado, Equipo.Aliado, new(2, 1));
            Unidad caido = Crear("Caído", RolUnidad.Soldado, Equipo.Aliado, new(1, 2));
            Unidad enemigo = Crear("Enemigo", RolUnidad.Soldado, Equipo.Enemigo, new(0, 1));
            paciente.Estadisticas.AplicarDanio(5);
            caido.Estadisticas.AplicarDanio(100);
            enemigo.Estadisticas.AplicarDanio(90);
            var servicio = new ServicioTurnos(Escenario(actor, paciente, caido, enemigo));
            ResultadoTurno resultado = servicio.EjecutarTurno(actor.Id);
            Exigir(resultado.Resumen.Contains("curó 5 PV") && paciente.Estadisticas.Vida == 100
                && caido.Estadisticas.Vida == 0 && enemigo.Estadisticas.Vida == 10, "La curación excedió sus límites.");
        }),
        ("Turno Médico se acerca a un herido fuera de alcance sin curarlo en esa acción", () =>
        {
            Unidad actor = Crear("Médico", RolUnidad.Medico, Equipo.Aliado, new(1, 1));
            Unidad paciente = Crear("Paciente", RolUnidad.Soldado, Equipo.Aliado, new(5, 1));
            paciente.Estadisticas.AplicarDanio(30);
            var servicio = new ServicioTurnos(Escenario(actor, paciente));
            servicio.EjecutarTurno(actor.Id);
            Exigir(actor.Posicion == new Posicion(2, 1) && paciente.Estadisticas.Vida == 70, "El médico actuó fuera de alcance.");
        }),
        ("Turno Francotirador prioriza menos PV entre enemigos vivos en alcance", () =>
        {
            Unidad actor = Crear("Francotirador", RolUnidad.Francotirador, Equipo.Aliado, new(1, 1));
            Unidad cercano = Crear("Cercano", RolUnidad.Soldado, Equipo.Enemigo, new(2, 1));
            Unidad debil = Crear("Débil", RolUnidad.Soldado, Equipo.Enemigo, new(4, 1));
            Unidad lejano = Crear("Lejano", RolUnidad.Soldado, Equipo.Enemigo, new(7, 7));
            Unidad caido = Crear("Caído", RolUnidad.Soldado, Equipo.Enemigo, new(2, 2));
            debil.Estadisticas.AplicarDanio(60);
            lejano.Estadisticas.AplicarDanio(99);
            caido.Estadisticas.AplicarDanio(100);
            var servicio = new ServicioTurnos(Escenario(actor, cercano, debil, lejano, caido));
            servicio.EjecutarTurno(actor.Id);
            Exigir(debil.Estadisticas.Vida == 22 && cercano.Estadisticas.Vida == 100 && lejano.Estadisticas.Vida == 1
                && caido.Estadisticas.Vida == 0 && actor.Posicion == new Posicion(1, 1), "Se eligió un objetivo inválido.");
        }),
        ("Turno Comandante acerca al aliado más alejado dentro del alcance de mando", () =>
        {
            Unidad actor = Crear("Comandante", RolUnidad.Comandante, Equipo.Aliado, new(1, 1));
            Unidad aliado = Crear("Reagrupar", RolUnidad.Soldado, Equipo.Aliado, new(4, 1));
            Unidad cercano = Crear("Cercano", RolUnidad.Soldado, Equipo.Aliado, new(1, 2));
            Unidad lejano = Crear("Fuera de alcance", RolUnidad.Soldado, Equipo.Aliado, new(7, 1));
            var servicio = new ServicioTurnos(Escenario(actor, aliado, cercano, lejano));
            servicio.EjecutarTurno(actor.Id);
            Exigir(aliado.Posicion == new Posicion(3, 1) && cercano.Posicion == new Posicion(1, 2)
                && lejano.Posicion == new Posicion(7, 1) && actor.Posicion == new Posicion(1, 1), "El comandante movió una unidad incorrecta.");
        }),
        ("Los turnos omiten acciones de unidades aturdidas o caídas y registran las cuatro etapas", () =>
        {
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
            foreach (bool caida in new[] { false, true })
            {
                Unidad actor = Crear("Actor", rol, Equipo.Aliado, new(1, 1));
                Unidad aliado = Crear("Aliado", RolUnidad.Soldado, Equipo.Aliado, new(1, 3));
                Unidad enemigo = Crear("Enemigo", RolUnidad.Soldado, Equipo.Enemigo, new(2, 1));
                aliado.Estadisticas.AplicarDanio(30);
                if (caida) actor.Estadisticas.AplicarDanio(actor.Estadisticas.VidaMaxima);
                else actor.CambiarEstado(new Aturdido());
                ServicioCombate combate = Escenario(actor, aliado, enemigo);
                var antes = combate.Mision.Unidades.Select(u => (u.Posicion, u.Estadisticas.Vida, u.Escudo)).ToArray();
                var servicio = new ServicioTurnos(combate);
                ResultadoTurno resultado = servicio.EjecutarTurno(actor.Id);
                Exigir(!resultado.Exito && antes.SequenceEqual(combate.Mision.Unidades.Select(u => (u.Posicion, u.Estadisticas.Vida, u.Escudo))),
                    $"La unidad {rol} bloqueada tuvo efectos.");
                VerificarSecuencia(resultado);
            }
        }),
        ("Los turnos esperan sin objetivos y el comandante respeta las restricciones del aliado", () =>
        {
            foreach (RolUnidad rol in Enum.GetValues<RolUnidad>())
            {
                Unidad actor = Crear("Solo", rol, Equipo.Aliado, new(1, 1));
                var servicio = new ServicioTurnos(Escenario(actor));
                ResultadoTurno resultado = servicio.EjecutarTurno(actor.Id);
                Exigir(resultado.Exito && actor.Posicion == new Posicion(1, 1) && actor.Estadisticas.Vida == actor.Estadisticas.VidaMaxima,
                    $"El rol {rol} sin objetivos produjo efectos inesperados.");
                VerificarSecuencia(resultado);
            }
            Unidad comandante = Crear("Comandante", RolUnidad.Comandante, Equipo.Aliado, new(1, 1));
            Unidad aliado = Crear("Aturdido", RolUnidad.Soldado, Equipo.Aliado, new(3, 1));
            aliado.CambiarEstado(new Aturdido());
            ResultadoTurno bloqueado = new ServicioTurnos(Escenario(comandante, aliado)).EjecutarTurno(comandante.Id);
            Exigir(!bloqueado.Exito && aliado.Posicion == new Posicion(3, 1), "El comandante ignoró el estado del aliado.");
            VerificarSecuencia(bloqueado);
        }),
        ("Servicio de turnos conserva el historial y admite unidades incorporadas después", () =>
        {
            ServicioCombate combate = Escenario();
            var servicio = new ServicioTurnos(combate);
            Exigir(servicio.Unidades.Count == 0, "La misión vacía tiene unidades.");
            Unidad actor = Crear("Nuevo", RolUnidad.Soldado, Equipo.Aliado, new(1, 1));
            Unidad enemigo = Crear("Enemigo", RolUnidad.Soldado, Equipo.Enemigo, new(3, 1));
            combate.IncorporarUnidad(actor);
            combate.IncorporarUnidad(enemigo);
            Exigir(servicio.Unidades.Count == 2 && servicio.Unidades.Any(u => u.Id == actor.Id)
                && servicio.Unidades.Any(u => u.Id == enemigo.Id), "No se actualizó el catálogo de unidades de ambos equipos.");
            ResultadoTurno primero = servicio.EjecutarTurno(actor.Id);
            string[] registroInicial = primero.Eventos.Select(e => e.Mensaje).ToArray();
            ResultadoTurno segundo = servicio.EjecutarTurno(actor.Id);
            Exigir(primero.Numero == 1 && segundo.Numero == 2 && servicio.Historial.Count == 2
                && registroInicial.SequenceEqual(primero.Eventos.Select(e => e.Mensaje)), "El historial perdió su secuencia o cambió un registro anterior.");
            DebeFallar<ArgumentException>(() => servicio.EjecutarTurno(Guid.NewGuid()));
            DebeFallar<ArgumentException>(() => new TurnoMedico(new ContextoTurno(actor.Id, combate)));
            Exigir(servicio.Historial.Count == 2, "Una selección inválida se agregó al historial.");
            ResultadoTurno respuestaManual = servicio.EjecutarTurno(enemigo.Id);
            Exigir(respuestaManual.Exito && respuestaManual.UnidadId == enemigo.Id && servicio.Historial.Count == 3,
                "El turno de una unidad enemiga no se pudo ejecutar manualmente.");
            VerificarSecuencia(respuestaManual);
        })
    ];

    private static Unidad Crear(string nombre, RolUnidad rol, Equipo equipo, Posicion posicion) => new UnidadBuilder()
        .DefinirIdentidad(nombre, equipo, CatalogoInicial.Especies[0]).SeleccionarRol(rol)
        .ConfigurarEstadisticas(CatalogoInicial.EstadisticasPara(rol)).ConfigurarEquipamiento([]).Ubicar(posicion).Construir();

    private static ServicioCombate Escenario(params Unidad[] unidades)
    {
        var mision = new Mision("Práctica", "Verificar turnos de unidad");
        foreach (Unidad unidad in unidades) mision.AgregarUnidad(unidad);
        return new ServicioCombate(mision);
    }

    private static void VerificarSecuencia(ResultadoTurno resultado)
    {
        var etapas = resultado.Eventos.Select(e => e.Etapa).ToArray();
        Exigir(etapas.Distinct().SequenceEqual(Enum.GetValues<EtapaTurno>())
            && etapas.SequenceEqual(etapas.OrderBy(e => e)), "Las cuatro etapas no conservan el orden.");
    }

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
}
