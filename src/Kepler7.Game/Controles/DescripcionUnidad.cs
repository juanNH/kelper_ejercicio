using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Game.Controles;

internal static class DescripcionUnidad
{
    public static string Mostrar(VistaUnidad unidad) =>
        $"{unidad.Nombre} · {unidad.Rol} · {unidad.Equipo} · {unidad.Especie}\r\n" +
        $"ID {unidad.Id}\r\nPV {unidad.Vida}/{unidad.VidaMaxima} · Ataque {unidad.Ataque} · Defensa {unidad.Defensa}\r\n" +
        $"Alcance {unidad.Alcance} · Movimiento {unidad.Movimiento} · Escudo {unidad.Escudo}\r\n" +
        $"Posición {unidad.Posicion} · Estado {unidad.Estado}\r\n" +
        $"Equipamiento: {(unidad.Inventario.Count == 0 ? "sin objetos" : string.Join(", ", unidad.Inventario))}";
}
