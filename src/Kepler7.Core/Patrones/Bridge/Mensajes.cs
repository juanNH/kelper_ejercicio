using System.Security.Cryptography;
using System.Text;

namespace Kepler7.Core.Patrones.Bridge;

public enum TipoMensaje { Texto, Alerta, Emergencia }
public enum TipoCifrado { SinCifrado, AES, Cuantico }
public sealed record TransmisionMensaje(string Tipo, string Cifrado, string Original, string Resultado);

public interface ICifrado
{
    string Nombre { get; }
    string Cifrar(string contenido);
}

public sealed class SinCifrado : ICifrado
{
    public string Nombre => "Sin cifrado";
    public string Cifrar(string contenido) => contenido;
}

public sealed class CifradoAES : ICifrado
{
    private readonly byte[] _clave = RandomNumberGenerator.GetBytes(32);
    public string Nombre => "AES-256 (sesión local)";

    public string Cifrar(string contenido)
    {
        using var aes = Aes.Create();
        aes.Key = _clave;
        aes.GenerateIV();
        byte[] cifrado = aes.EncryptCbc(Encoding.UTF8.GetBytes(contenido), aes.IV);
        return $"IV={Convert.ToBase64String(aes.IV)}; DATOS={Convert.ToBase64String(cifrado)}";
    }
}

public sealed class CifradoCuantico : ICifrado
{
    public string Nombre => "Cuántico (simulación educativa)";

    public string Cifrar(string contenido)
    {
        // Simula una clave de un solo uso. No implementa un protocolo cuántico real.
        byte[] datos = Encoding.UTF8.GetBytes(contenido);
        byte[] clave = RandomNumberGenerator.GetBytes(datos.Length);
        for (int i = 0; i < datos.Length; i++) datos[i] ^= clave[i];
        return $"Simulación: {Convert.ToBase64String(datos)}";
    }
}

public abstract class Mensaje(ICifrado cifrado)
{
    public ICifrado Cifrado { get; set; } = cifrado;
    protected abstract string Tipo { get; }
    protected abstract string Preparar(string contenido);

    public TransmisionMensaje Enviar(string contenido)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contenido);
        return new(Tipo, Cifrado.Nombre, contenido, Cifrado.Cifrar(Preparar(contenido)));
    }
}

public sealed class MensajeTexto(ICifrado cifrado) : Mensaje(cifrado)
{
    protected override string Tipo => "Texto";
    protected override string Preparar(string contenido) => contenido;
}

public sealed class MensajeAlerta(ICifrado cifrado) : Mensaje(cifrado)
{
    protected override string Tipo => "Alerta";
    protected override string Preparar(string contenido) => $"[ALERTA] {contenido}";
}

public sealed class MensajeEmergencia(ICifrado cifrado) : Mensaje(cifrado)
{
    protected override string Tipo => "Emergencia";
    protected override string Preparar(string contenido) => $"[PRIORIDAD MÁXIMA] {contenido}";
}
