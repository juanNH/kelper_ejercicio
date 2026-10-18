namespace Kepler7.Core.Patrones.Proxy;

public interface IBoveda
{
    string LeerArchivo(string id);
    void EscribirArchivo(string id, string contenido);
}
