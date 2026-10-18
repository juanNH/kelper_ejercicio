using Kepler7.Core.Contratos;
using Kepler7.Core.Datos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Facade;
using Kepler7.Core.Servicios;

namespace Kepler7.Game;

internal sealed record ServiciosAplicacion(IPreparacionMision Preparacion, IControlEstacion Estacion,
    IServicioComunicaciones Comunicaciones, IServicioCombate Combate, IServicioTurnos Turnos);

internal static class ComposicionAplicacion
{
    public static ServiciosAplicacion Crear()
    {
        var preparacion = new ServicioPreparacion();
        var mision = new Mision("Primer contacto", "Preparar el equipo y verificar las defensas de Kepler-7.");
        // El escenario utiliza Builder y Prototype desde su creación.
        Unidad soldado = preparacion.CrearEscuadron("Exploración")[0];
        // Herida de práctica para que el turno del médico tenga un efecto observable.
        soldado.Estadisticas.AplicarDanio(35);
        Unidad medico = preparacion.ClonarPlantilla("Medico");
        mision.AgregarUnidad(soldado);
        mision.AgregarUnidad(medico);
        mision.AgregarUnidad(preparacion.ClonarPlantilla("Francotirador", new Posicion(0, 3)));
        mision.AgregarUnidad(preparacion.CrearUnidad(new("Comandante de misión", RolUnidad.Comandante,
            preparacion.Especies[0], CatalogoInicial.EstadisticasPara(RolUnidad.Comandante), new Posicion(0, 1),
            CatalogoInicial.EquipamientoPara(RolUnidad.Comandante))));
        Unidad enemigo = preparacion.ClonarPlantilla("Enemigo · Soldado", new Posicion(3, 1));
        mision.AgregarUnidad(enemigo);
        var combate = new ServicioCombate(mision);
        return new(preparacion, new ControlCentral(), new ServicioComunicaciones(), combate, new ServicioTurnos(combate));
    }
}
