using Kepler7.Core.Contratos;
using Kepler7.Core.Patrones.Bridge;

namespace Kepler7.Core.Servicios;

public sealed class ServicioComunicaciones : IServicioComunicaciones
{
    public TransmisionMensaje Enviar(TipoMensaje tipo, TipoCifrado cifrado, string contenido)
    {
        ICifrado implementacion = cifrado switch
        {
            TipoCifrado.SinCifrado => new SinCifrado(),
            TipoCifrado.AES => new CifradoAES(),
            TipoCifrado.Cuantico => new CifradoCuantico(),
            _ => throw new ArgumentOutOfRangeException(nameof(cifrado))
        };
        Mensaje mensaje = tipo switch
        {
            TipoMensaje.Texto => new MensajeTexto(implementacion),
            TipoMensaje.Alerta => new MensajeAlerta(implementacion),
            TipoMensaje.Emergencia => new MensajeEmergencia(implementacion),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
        return mensaje.Enviar(contenido);
    }
}
