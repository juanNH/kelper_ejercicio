using Kepler7.Core.Patrones.Flyweight;

namespace Kepler7.Core.Modelos;

public sealed class Mision
{
    private readonly List<Unidad> _unidades = [];
    public Guid Id { get; } = Guid.NewGuid();
    public string Nombre { get; }
    public string Objetivo { get; }
    public int Ancho => 8;
    public int Alto => 8;
    public IReadOnlyList<Unidad> Unidades => _unidades.AsReadOnly();

    public Mision(string nombre, string objetivo)
    {
        Nombre = nombre;
        Objetivo = objetivo;
    }

    public bool EstaOcupada(Posicion posicion) => _unidades.Any(u => u.EstaViva && u.Posicion == posicion);

    public void AgregarUnidad(Unidad unidad)
    {
        if (_unidades.Any(u => u.Id == unidad.Id)) throw new InvalidOperationException("La unidad ya está en la misión.");
        if (!unidad.Posicion.EstaDentro(Ancho, Alto) || EstaOcupada(unidad.Posicion))
            throw new InvalidOperationException("La posición de la unidad no está disponible.");
        _unidades.Add(unidad);
    }

    public Unidad BuscarUnidad(Guid id) => _unidades.SingleOrDefault(u => u.Id == id)
        ?? throw new ArgumentException("La unidad no pertenece a esta misión.", nameof(id));
}

public sealed record Tripulante(Guid Id, string Nombre, string Rango, IEspecie Especie);
public sealed record Enemigo(Unidad Unidad);
public sealed record ContextoCombate(int Turno, IReadOnlyList<Unidad> Unidades, int Ancho, int Alto);
public sealed record AccionIA(TipoAccionIA Tipo, Guid UnidadId, Guid? ObjetivoId = null, Posicion? Destino = null);
