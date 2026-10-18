# Primera etapa y tareas de los tres estudiantes

La estructura, una implementación inicial de seis patrones y las implementaciones de Flyweight y Template Method del estudiante 1 ya permiten trabajar sobre una aplicación ejecutable. El objetivo inmediato del equipo es comprender ese recorrido y conectar los módulos mediante los contratos compartidos.

## Primer paso para empezar hoy

1. Abrir `Kepler7.sln`, elegir `Kepler7.Game` como inicio y ejecutar, o usar `dotnet run --project src/Kepler7.Game/Kepler7.Game.csproj` desde la raíz.
2. Leer `Contratos/` y `Modelos/` de Core. Revisar entre los tres la posición por coordenadas, las estadísticas y qué devuelve cada acción. Anotar cualquier cambio de contrato antes de modificar consumidores.
3. En Preparación, construir una unidad en `(0,0)` y agregarla a la misión. Clonar otra plantilla en `(0,4)`, comparar su ID y posición con la original y agregarla también.
4. En Estación, iniciar los seis subsistemas, activar emergencia y apagarlos. Probar un mensaje con cada cifrado y comprobar el resultado y el registro.
5. En Combate, seleccionar al soldado de `(1,1)`, atacar al enemigo y observar State → defensas → vida. Cambiar el estado a Aturdido y comprobar el rechazo. Cambiar las capas, su orden y las opciones de esquiva/bloqueo para observar el efecto.
6. Ejecutar `dotnet run --project tests/Kepler7.Tests/Kepler7.Tests.csproj`. Cada estudiante lee las comprobaciones de sus patrones y explica por qué se espera ese resultado.

## Reparto de los doce patrones

| Estudiante | Cuatro patrones a su cargo | Carpeta/controles principales |
| --- | --- | --- |
| 1 | Builder, Prototype, Flyweight, Template Method | Modelos compartidos, preparación y turnos; `PreparacionControl` |
| 2 | Bridge, Facade, Proxy, Strategy | Estación, comunicaciones, bóveda, IA y composición; `EstacionControl`, `MainForm` |
| 3 | Command, Chain of Responsibility, State, Visitor | Combate e inventario; `CombateControl`, `InventarioControl` |

El primer estudiante coordina los modelos; los tres acuerdan sus cambios. El segundo mantiene el contenedor de la aplicación; cada estudiante conecta su módulo y participa en la integración. Los nombres del segundo y tercer estudiante se completarán cuando el equipo los confirme.

## Estudiante 1

La base de Builder y Prototype está en `Patrones/Builder`, `Patrones/Prototype` y `ServicioPreparacion`. Primero recorrer la construcción manual, los dos escuadrones del director y las copias del registro. Verificar que cada clon cambie estadísticas, inventario, posición y estado sin tocar la plantilla.

La revisión del enunciado está detallada en `AUDITORIA_ESTUDIANTE_1.md`. Para probar un escuadrón completo, elegir origen `(4,0)` antes de prepararlo y agregar sus tres integrantes desde la lista. El registro contiene los cuatro roles de ambos equipos. Para cambiar los datos de una copia, elegir una plantilla y pulsar **Crear variante editable…**: se pueden ajustar nombre, estadísticas, posición e inventario mientras se comparan original y variante.

**Flyweight implementado.** `FabricaEspecies` utiliza un `Dictionary<string, IEspecie>`, normaliza claves, valida el catálogo y conserva una referencia por especie ante solicitudes repetidas o concurrentes. Preparación, plantillas y equipos usan las mismas instancias.

1. Abrir **Preparación → Tripulantes · Flyweight** y generar la población. Confirmar 10.000 tripulantes, cinco especies y 2.000 tripulantes por cada objeto compartido.
2. Seleccionar filas, incluida la última, para inspeccionar ID, nombre y rango particulares junto con los datos compartidos de especie.
3. Revisar la comparación real de referencias: cinco objetos de especie frente a 10.000 individuales. La reducción de 9.995 objetos (99,95 %) se refiere a objetos de especie; no es una medición de memoria RAM.
4. Repetir la generación y confirmar que la fábrica sigue teniendo cinco instancias; la población anterior se reemplaza. Ejecutar las comprobaciones de normalización, concurrencia, independencia e integración.
5. Revisar el UML de Flyweight agregado a `PATRONES.md` y explicar dónde se comparte el estado intrínseco y dónde queda el estado extrínseco.

**Template Method implementado.** `TurnoSoldado`, `TurnoMedico`, `TurnoFrancotirador` y `TurnoComandante` comparten el método no virtual `EjecutarTurno()` de `TurnoUnidad`. Cada variante redefine decisión y acción. `ServicioTurnos` elige la variante por rol y conserva un historial de eventos con las cuatro etapas. Los ataques y movimientos usan el contrato de combate que ya existe.

1. Desde una nueva ejecución, abrir **Preparación → Turnos · Template Method**. La práctica incluye los cuatro roles y un soldado con 65/100 PV.
2. Ejecutar primero el Médico: el soldado queda en 90 PV. Ejecutar Soldado y Francotirador: el intruso pasa de 100 a 90 y luego a 62 PV. Ejecutar Comandante: el francotirador se acerca desde `(0,3)` a `(0,2)`.
3. Abrir Combate para observar las mismas unidades con sus valores actualizados. Agregar otra unidad desde preparación y comprobar que aparece en el selector de turnos.
4. Revisar cada ejecución anterior en el historial. Comprobar inicio → decisión → acción → finalización en los cuatro roles, incluidos los turnos omitidos de unidades aturdidas o caídas.
5. Revisar los cuatro UML del estudiante 1 y ejecutar las comprobaciones de turnos en `TemplateMethodTests.cs`. Queda por acordar con los demás responsables cómo conectar sus futuros comandos y respuesta enemiga al ciclo completo de rondas.

El selector de turnos también admite unidades enemigas: ejecutarlas manualmente conserva la secuencia de Template Method y el historial. Los casos de `Estudiante1AuditTests.cs` comprueban los ocho pares rol/equipo, variantes, vistas completas y copia profunda con datos mutables de prueba.

## Estudiante 2

La base de Bridge y Facade está en sus carpetas de patrones, `ServicioComunicaciones` y `EstacionControl`. Primero recorrer las nueve combinaciones y las tres operaciones de la fachada. Los subsistemas permanecen internos a Core.

**Primera tarea nueva: implementar `BovedaDatos` y `ProxyBoveda`.** Usar `IBoveda` y `Usuario`. El proxy recibe el usuario actual y controla lectura y escritura antes de delegarlas; permite solo Capitán y Oficial de Seguridad.

1. Crear la bóveda real con datos de ejemplo y un proxy que proteja las dos operaciones.
2. Añadir comprobaciones de permisos para los tres roles; confirmar que un rechazo no modifica el archivo.
3. Persistir cada intento denegado con fecha, usuario, operación e ID de archivo en una subcarpeta de `%AppData%\Kepler7\`. Mostrar la auditoría en Estación y comprobar que se conserva al reiniciar.
4. Implementar `IAAgresiva`, `IADefensiva`, `IAAleatoria` e `IACalculadora` con `IEstrategiaIA`; comparar sus decisiones ante un mismo contexto.
5. Crear `IServicioIA` y permitir el intercambio de estrategia desde Estación. Conectar cada decisión a las acciones/comandos de combate junto al Estudiante 3.
6. Mantener composición y navegación; después integrar inicio, tutorial, progreso JSON, logros y resumen con aportes de los tres estudiantes.

Cerrar Proxy cuando ambos roles autorizados operen, el resto sea rechazado en ambas operaciones y la auditoría continúe disponible después de reiniciar. El formulario solo recibe el proxy; nunca debe recibir la bóveda real.

## Estudiante 3

La base de State y Chain está en sus carpetas de patrones y `ServicioCombate`. Primero comprobar que Aturdido no actúe, que Berserk modifique los resultados y que reordenar o quitar defensas cambie el daño sin alterar la plantilla de la unidad.

**Primera tarea nueva: implementar `MoverComando` y `GestorComandos`.** Empezar por una acción con un estado anterior pequeño: ID de unidad y posición previa. Validar el movimiento antes de incorporarlo al historial.

1. Acordar un mecanismo de captura/restauración de estado en Core. Al deshacer, restaurar el estado previo incluso si el personaje cambió de estado después del movimiento.
2. Implementar ejecución, deshacer, rehacer y un historial visible. Un comando rechazado no entra en las pilas; ejecutar una acción nueva después de deshacer vacía la pila de rehacer.
3. Añadir `AtacarComando`, `CurarComando` y `ConstruirComando`. Definir con el equipo qué construye esta última acción y qué recursos consume. Restaurar vida, escudo, posición, estado, inventario y recursos afectados; incluir efectos sobre otras unidades.
4. Conectar los botones de combate al gestor. Mantener el servicio como receptor de las acciones y evitar reglas en eventos de botones.
5. Implementar `CalculadorPrecio`, `ContadorArmas` y `AplicadorVeneno` con `IVisitor`. Como los objetos actuales son inmutables, acordar una transformación del inventario o un efecto aplicado a su dueño para el veneno; registrar la decisión antes de cambiar `IItem`.
6. Publicar la implementación de `IServicioInventario`, agregar selección de visitante y mostrar resultados en Inventario. Preparar los cuatro UML e integrar los comandos con IA y turnos.

Cerrar Command cuando se pueda ejecutar → deshacer → rehacer cada acción y recuperar exactamente los valores anteriores, incluidas las acciones fallidas y el vaciado de rehacer al abrir una nueva rama de acciones.

## Primer recorrido integrado después de esas tareas

1. Preparar unidades con Builder/Prototype y especies de la fábrica Flyweight.
2. Iniciar sistemas de estación con Facade y empezar el turno de la unidad con Template Method.
3. Ejecutar una acción del jugador mediante Command; aplicar State y Chain si hay daño.
4. Pedir a Strategy una decisión enemiga y convertirla en una acción del mismo sistema.
5. Registrar etapas, resultados y uso de inventario con Visitor. Mantener las comunicaciones Bridge y la bóveda Proxy accesibles en la misión.
6. Resolver victoria/derrota y progreso, guardar XP/logros y mostrar resumen final.

Antes de integrar deshacer con IA, el equipo debe fijar su alcance: una acción o un turno completo. Repetir una acción aleatoria debe usar el resultado registrado para no generar otro desenlace al rehacer.

## Trabajo con Git

Crear una rama por tarea, por ejemplo `codex/estudiante-1-flyweight`, `codex/estudiante-2-proxy` o `codex/estudiante-3-command`. Los ejemplos no se crean automáticamente. Cada estudiante hace sus propios commits significativos y explica los cambios; evitar reemplazar archivos compartidos completos para integrar una función pequeña.

Antes de integrar, compilar, ejecutar las comprobaciones pertinentes y recorrer desde la interfaz la acción cambiada. Los tres revisan los contratos. Compartir los cambios de modelos y composición pronto para evitar que la integración se acumule al final.

## Pendientes de la entrega final

- [ ] Los doce patrones funcionan durante una partida y cada estudiante puede explicar sus cuatro.
- [ ] Inicio con el nombre del juego y los tres integrantes.
- [ ] Tutorial de al menos tres pantallas y feedback visual con controles de Windows Forms.
- [ ] Ciclo de turnos con respuesta enemiga, progreso, logros, victoria y derrota.
- [ ] JSON en `%AppData%\Kepler7\` y auditoría de accesos denegados persistente.
- [ ] Un UML general y doce UML de patrones, revisados contra el código final.
- [ ] Informe de hasta diez páginas, capturas reales en README y presentación conjunta.
- [ ] Publicación Release completa, instrucciones y aportes significativos de los tres en Git.
