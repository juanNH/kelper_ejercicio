namespace Kepler7.Core.Patrones.Flyweight;

public sealed class FabricaEspecies
{
    private sealed record Definicion(string Nombre, string Color, string Planeta, string Dieta);

    private static readonly Dictionary<string, Definicion> Definiciones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Humana"] = new("Humana", "#5CA9E6", "Tierra", "Omnívora"),
        ["Marciana"] = new("Marciana", "#EB8874", "Marte", "Herbívora"),
        ["Andromedana"] = new("Andromedana", "#BE8AE8", "Andrómeda", "Mineral"),
        ["Titaniana"] = new("Titaniana", "#E7C665", "Titán", "Omnívora"),
        ["Europa"] = new("Europa", "#77CDB2", "Europa", "Algas")
    };

    private readonly Dictionary<string, IEspecie> _especies = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sincronizacion = new();

    public IReadOnlyList<string> NombresDisponibles { get; } = Array.AsReadOnly(Definiciones.Keys.ToArray());

    // No se eliminan entradas: Count también es la cantidad total de objetos creados.
    public int CantidadCreadas
    {
        get { lock (_sincronizacion) return _especies.Count; }
    }

    public IEspecie ObtenerEspecie(string nombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        string clave = nombre.Trim();
        if (!Definiciones.TryGetValue(clave, out Definicion? definicion))
            throw new ArgumentException($"La especie '{clave}' no está en el catálogo de Kepler-7.", nameof(nombre));

        // La demostración se genera en segundo plano; dos solicitudes simultáneas
        // tampoco deben crear dos objetos para la misma especie.
        lock (_sincronizacion)
        {
            if (!_especies.TryGetValue(clave, out IEspecie? especie))
            {
                especie = new Especie(definicion.Nombre, definicion.Color, definicion.Planeta, definicion.Dieta);
                _especies.Add(definicion.Nombre, especie);
            }
            return especie;
        }
    }

    public IReadOnlyList<IEspecie> ObtenerTodas() =>
        Array.AsReadOnly(NombresDisponibles.Select(ObtenerEspecie).ToArray());
}
