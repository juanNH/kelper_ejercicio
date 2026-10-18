# Revisión de los cuatro patrones del estudiante 1

Revisión del 18 de octubre de 2026 contra las secciones 4.1.1, 4.1.2, 4.2.3, 4.3.5 y 11.3 del [enunciado del TP](https://docs.google.com/document/d/1vVW6Cx8qfFPTv6uYRMMYDLgfy0bxDR26ACcPQ2I3nQo). El alcance comprende Builder, Prototype, Flyweight, Template Method, sus modelos, preparación, contratos, diagramas y conexión con la misión existente.

## Builder

| Requisito | Evidencia y resultado |
| --- | --- |
| `IBuilderUnidad` con rol, estadísticas, equipamiento y construcción | Interfaz y `UnidadBuilder` en `Patrones/Builder`; configuración por pasos |
| Validar y entregar una unidad completa | Se rechazan configuración incompleta, identidad vacía, rol inválido, posición fuera del tablero y entradas nulas; estadísticas valida sus límites |
| Dos composiciones predefinidas | `DirectorEquipo`: Exploración y Asalto, tres unidades por composición |
| Configuración manual o selección predefinida | Ambos recorridos están disponibles en Preparación; las recetas admiten un origen y validan sus tres celdas verticales |
| Pasos y vista previa de la unidad/escuadrón | Pasos numerados, lista del escuadrón y vista completa de cada integrante, con identidad, equipo, estadísticas, inventario y posición |

Prueba desde la interfaz: elegir `(4,0)`, preparar Exploración y agregar sus integrantes. Deben ocupar `(4,0)`, `(4,1)` y `(4,2)`. Construir otra unidad manualmente en `(0,0)` y comprobar que conserva los valores ingresados.

## Prototype

| Requisito | Evidencia y resultado |
| --- | --- |
| Plantillas aliadas y enemigas | Ocho entradas en `RegistroPrototipos`: los cuatro roles de ambos equipos |
| Interfaz o método de clonación | `IPrototipoUnidad`, `Unidad.Clonar()` y `Unidad.ClonarConAjustes()` |
| Buscar y clonar por tipo | `RegistroPrototipos` permite consultar, clonar y crear variantes identificadas por tipo |
| ID nuevo y copia independiente de datos mutables | Nueva identidad, estadísticas y estado clonados, posición por valor, lista e ítems copiados; se conserva el escudo actual |
| Mantener el original y compartir solo datos inmutables | El registro guarda una copia propia. Las pruebas usan estado e ítems mutables para comprobar aislamiento; la especie mantiene la misma referencia |
| Elegir plantilla, crear variante y compararlas | `VarianteUnidadForm` muestra original y vista previa; edita nombre, vida, ataque, defensa, alcance, movimiento, posición y equipamiento |
| Uso en preparación/partida | Las copias se incorporan a la misión; el enemigo inicial también se obtiene de una plantilla clonada |

Prueba desde la interfaz: elegir **Enemigo · Soldado**, abrir **Crear variante editable…** y configurar nombre propio, 150 PV máximos, 125 actuales, ataque 35, defensa 7, alcance 4, movimiento 3, posición `(5,5)` y un botiquín. El original conserva 100/100 PV, ataque 24 y posición `(6,1)`. Incorporar la variante y moverla en Combate; su vista se actualiza al volver y la plantilla sigue intacta.

## Flyweight

| Requisito | Evidencia y resultado |
| --- | --- |
| `IEspecie` con nombre, color, planeta y dieta | Estado intrínseco inmutable implementado por `Especie` |
| Tripulante con nombre, ID, rango y referencia a especie | `Tripulante` mantiene datos particulares y una referencia a `IEspecie` |
| Fábrica con `Dictionary<string, IEspecie>` y unicidad | `FabricaEspecies` normaliza claves y protege la caché frente a solicitudes simultáneas |
| 10.000 tripulantes y contador de cinco especies | `DemostradorFlyweight` genera 2.000 tripulantes por especie; la pantalla muestra 10.000, cinco instancias y distribución por especie |
| Comparación con/sin Flyweight | Crea una población equivalente sin compartir y cuenta referencias reales: cinco frente a 10.000; la reducción se refiere a objetos de especie, no a RAM |

Prueba desde la interfaz: generar en **Tripulantes · Flyweight**, recorrer la grilla virtual y regenerar. La fábrica permanece en cinco instancias, con IDs particulares para cada tripulante.

## Template Method

| Requisito | Evidencia y resultado |
| --- | --- |
| Clase abstracta `TurnoUnidad` | Base común en `Patrones/TemplateMethod` |
| Método no virtual con orden fijo | `EjecutarTurno()` conserva inicio → decisión → acción → finalización; una comprobación verifica también que no sea virtual |
| Cuatro subclases requeridas | `TurnoSoldado`, `TurnoMedico`, `TurnoFrancotirador`, `TurnoComandante` redefinen decisión y acción |
| Turnos para todas las unidades | El servicio permite ejecución manual de aliados y enemigos; se comprueban los cuatro roles de ambos equipos |
| Registro visible de cada etapa | `ResultadoTurno` conserva eventos por etapa y la pantalla permite consultar ejecuciones anteriores, incluso acciones omitidas |
| Efectos en la misión | Ataque, movimiento, curación y reagrupamiento afectan las mismas unidades que muestra Combate |

Prueba reproducible desde una aplicación recién abierta: Médico lleva al soldado de 65 a 90 PV; Soldado lleva al intruso de 100 a 90 PV; Francotirador lo lleva a 62 PV; Comandante acerca al francotirador de `(0,3)` a `(0,2)`. Seleccionar después al intruso permite ejecutar manualmente su turno. La coordinación automática de rondas permanece como integración futura del equipo.

## Validación y entrega del módulo

- Compilación de la solución en Release sin errores ni advertencias.
- 35 comprobaciones del núcleo: casos existentes más `TemplateMethodTests.cs` y `Estudiante1AuditTests.cs`.
- Recorrido de interfaz comprobado: construcción manual, incorporación de escuadrón, edición/validación de variante enemiga, comparación con el original, movimiento en misión, turnos de ambos equipos y población Flyweight 10.000/5.
- Los cuatro diagramas del estudiante 1 están en `PATRONES.md`; el diagrama general y los contratos están en `ARQUITECTURA.md`.
- La sección completada del informe, los recorridos de prueba y los cuatro UML incorporados al documento del trabajo se versionan en `INFORME_ESTUDIANTE_1.md` y `diagramas/` (PNG, SVG y Mermaid). Los apartados de los demás integrantes quedan pendientes en el informe.
- No se modificaron las implementaciones de los ocho patrones asignados a los estudiantes 2 y 3.

Los requisitos generales de la entrega —juego completo con progreso, persistencia, tutorial, victoria/derrota, informe, presentación y contribuciones reales de cada estudiante— se revisan en conjunto. Esta revisión acredita el módulo del estudiante 1 y su integración manual actual; no acredita la entrega completa del equipo.
