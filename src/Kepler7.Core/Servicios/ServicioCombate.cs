using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Servicios;

public sealed class ServicioCombate(Mision mision) : IServicioCombate
{
    public Mision Mision { get; } = mision;

    public ResultadoAccion IncorporarUnidad(Unidad unidad)
    {
        if (!unidad.EstaViva) return ResultadoAccion.Rechazado("La unidad debe estar viva para entrar en la misión.");
        if (Mision.Unidades.Any(u => u.Id == unidad.Id))
            return ResultadoAccion.Rechazado("Esta unidad ya está en la misión; construí o cloná otra.");
        if (!unidad.Posicion.EstaDentro(Mision.Ancho, Mision.Alto) || Mision.EstaOcupada(unidad.Posicion))
            return ResultadoAccion.Rechazado("La posición está ocupada o fuera del tablero. Elegí otra posición en Preparación.");
        Mision.AgregarUnidad(unidad);
        return ResultadoAccion.Correcto($"{unidad.Nombre} se incorporó en {unidad.Posicion}.");
    }

    public ResultadoAccion Mover(Guid unidadId, Posicion destino)
    {
        Unidad unidad = Mision.BuscarUnidad(unidadId);
        if (!unidad.EstaViva) return ResultadoAccion.Rechazado("La unidad está fuera de combate.");
        if (!destino.EstaDentro(Mision.Ancho, Mision.Alto)) return ResultadoAccion.Rechazado("Destino fuera del tablero.");
        if (unidad.Posicion == destino) return ResultadoAccion.Rechazado("La unidad ya está en esa celda.");
        if (Mision.EstaOcupada(destino)) return ResultadoAccion.Rechazado("La celda está ocupada.");
        if (unidad.Posicion.DistanciaA(destino) > unidad.Estadisticas.Movimiento)
            return ResultadoAccion.Rechazado($"Movimiento máximo: {unidad.Estadisticas.Movimiento} celdas por acción.");
        Posicion origen = unidad.Posicion;
        if (!unidad.Mover(destino)) return ResultadoAccion.Rechazado($"El estado {unidad.Estado.Tipo} impide moverse.");
        return ResultadoAccion.Correcto($"{unidad.Nombre}: {origen} → {destino}.");
    }

    public ResultadoAccion Atacar(Guid atacanteId, Guid objetivoId, ConfiguracionDefensa defensa)
    {
        Unidad atacante = Mision.BuscarUnidad(atacanteId);
        Unidad objetivo = Mision.BuscarUnidad(objetivoId);
        if (!atacante.EstaViva || !objetivo.EstaViva)
            return ResultadoAccion.Rechazado("Ambas unidades deben estar vivas.");
        if (atacante.Equipo == objetivo.Equipo) return ResultadoAccion.Rechazado("No se puede atacar al mismo equipo.");
        if (atacante.Posicion.DistanciaA(objetivo.Posicion) > atacante.Estadisticas.Alcance)
            return ResultadoAccion.Rechazado("El objetivo está fuera de alcance.");
        int ataque = atacante.Atacar();
        if (ataque <= 0) return ResultadoAccion.Rechazado($"El estado {atacante.Estado.Tipo} impide atacar.");

        // Validar la cadena antes de modificar cualquier estadística.
        IManejadorDanio cadena = CadenaDefensas.Crear(defensa.Orden);
        int ajustado = objetivo.RecibirDanio(ataque);
        var danio = new Danio(objetivo, ajustado, defensa.Esquivar, defensa.Bloquear);
        danio.Registrar($"State: ataque {ataque}, daño entrante {ajustado} ({objetivo.Estado.Tipo}).");
        cadena.Manejar(danio);
        return ResultadoAccion.Correcto($"{atacante.Nombre} atacó a {objetivo.Nombre}.", danio.Recorrido.ToArray());
    }

    public ResultadoAccion CambiarEstado(Guid unidadId, TipoEstado estado)
    {
        Unidad unidad = Mision.BuscarUnidad(unidadId);
        unidad.CambiarEstado(EstadosPersonaje.Crear(estado));
        return ResultadoAccion.Correcto($"{unidad.Nombre}: estado {estado}.");
    }
}
