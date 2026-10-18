namespace Kepler7.Game.Controles;

public sealed class ProximosPasosControl : UserControl
{
    public ProximosPasosControl()
    {
        var columna = ElementosUi.Columna();
        columna.Controls.Add(ElementosUi.Texto("Primer paso del equipo", 850));
        columna.Controls.Add(ElementosUi.Texto("Ejecutar la base, revisar los contratos y recorrer Preparación → Estación → Combate. Cada estudiante debe explicar los patrones disponibles de su módulo y registrar una mejora pequeña en su propia rama.", 850));
        columna.Controls.Add(ElementosUi.Texto("Estudiante 1: Builder, Prototype, Flyweight y los cuatro turnos de Template Method disponibles. Probarlos desde Preparación y revisar sus etapas en el historial.", 850));
        columna.Controls.Add(ElementosUi.Texto("Estudiante 2: Bridge y Facade → Proxy con permisos y auditoría persistente → cuatro estrategias de IA.", 850));
        columna.Controls.Add(ElementosUi.Texto("Estudiante 3: State y Chain of Responsibility → cuatro comandos con deshacer/rehacer → tres visitantes de inventario.", 850));
        columna.Controls.Add(ElementosUi.Texto("Después: integrar un ciclo de turnos con respuesta enemiga, inicio y nombres, tutorial de tres pantallas, progreso y logros, JSON en AppData, victoria/derrota y resumen final.", 850));
        columna.Controls.Add(ElementosUi.Texto("Esta base usa un escenario en memoria. La estación y el tablero todavía se operan manualmente; el ciclo de partida y el guardado se construyen en las siguientes etapas.", 850));
        Controls.Add(columna);
    }
}
