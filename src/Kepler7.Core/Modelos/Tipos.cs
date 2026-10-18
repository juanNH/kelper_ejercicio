namespace Kepler7.Core.Modelos;

public enum RolUnidad { Soldado, Medico, Francotirador, Comandante }
public enum Equipo { Aliado, Enemigo }
public enum TipoEstado { Normal, Envenenado, Aturdido, Berserk }
public enum RolUsuario { Tripulante, Capitan, OficialSeguridad }
public enum TipoAccionIA { Esperar, Mover, Atacar, Curar }

public readonly record struct Posicion(int X, int Y)
{
    public bool EstaDentro(int ancho, int alto) => X >= 0 && Y >= 0 && X < ancho && Y < alto;
    public int DistanciaA(Posicion otra) => Math.Abs(X - otra.X) + Math.Abs(Y - otra.Y);
    public override string ToString() => $"({X}, {Y})";
}

public sealed record Usuario(Guid Id, string Nombre, RolUsuario Rol);

public sealed record ResultadoAccion(bool Exito, string Mensaje, IReadOnlyList<string> Eventos)
{
    public static ResultadoAccion Correcto(string mensaje, params string[] eventos) =>
        new(true, mensaje, eventos);

    public static ResultadoAccion Rechazado(string mensaje) => new(false, mensaje, []);
}
