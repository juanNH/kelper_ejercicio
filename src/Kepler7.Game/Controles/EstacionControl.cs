using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Bridge;

namespace Kepler7.Game.Controles;

public sealed class EstacionControl : UserControl
{
    public EstacionControl(IControlEstacion estacion, IServicioComunicaciones comunicaciones, Action<ResultadoAccion> registrar)
    {
        BindingContext = new BindingContext();
        var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var operaciones = ElementosUi.Columna();
        var estado = ElementosUi.Salida(420, 220);
        void ActualizarEstado() => estado.Text = string.Join(Environment.NewLine,
            estacion.ConsultarEstado().Select(s => $"{s.Nombre}: {s.Estado}"));
        void Ejecutar(string nombre, Func<IReadOnlyList<string>> operacion)
        {
            registrar(ResultadoAccion.Correcto(nombre, operacion().ToArray()));
            ActualizarEstado();
        }
        operaciones.Controls.Add(ElementosUi.Texto("ESTUDIANTE 2 · Facade"));
        operaciones.Controls.Add(ElementosUi.Texto("ControlCentral coordina seis subsistemas. Esta pantalla consume únicamente IControlEstacion."));
        operaciones.Controls.Add(ElementosUi.Boton("Iniciar turno de estación", () => Ejecutar("Estación operativa.", estacion.IniciarTurno)));
        operaciones.Controls.Add(ElementosUi.Boton("Entrar en emergencia", () => Ejecutar("Emergencia activada.", estacion.EntrarEnEmergencia)));
        operaciones.Controls.Add(ElementosUi.Boton("Apagar estación", () => Ejecutar("Estación apagada.", estacion.ApagarEstacion)));
        operaciones.Controls.Add(estado);
        var mensajes = ElementosUi.Columna();
        var tipo = ElementosUi.Opciones(Enum.GetValues<TipoMensaje>());
        var cifrado = ElementosUi.Opciones(Enum.GetValues<TipoCifrado>());
        var contenido = new TextBox { Width = 420, Height = 80, Multiline = true, Text = "Kepler-7 informa: misión de reconocimiento iniciada.", MaxLength = 2000 };
        var salida = ElementosUi.Salida(420, 210);
        mensajes.Controls.Add(ElementosUi.Texto("ESTUDIANTE 2 · Bridge"));
        mensajes.Controls.Add(ElementosUi.Texto("Combiná cualquiera de los tres tipos de mensaje con cualquiera de los tres cifrados."));
        mensajes.Controls.Add(tipo);
        mensajes.Controls.Add(cifrado);
        mensajes.Controls.Add(contenido);
        mensajes.Controls.Add(ElementosUi.Boton("Enviar mensaje", () =>
        {
            if (string.IsNullOrWhiteSpace(contenido.Text))
            {
                registrar(ResultadoAccion.Rechazado("Escribí un mensaje antes de enviarlo."));
                return;
            }
            TransmisionMensaje resultado = comunicaciones.Enviar((TipoMensaje)tipo.SelectedItem!, (TipoCifrado)cifrado.SelectedItem!, contenido.Text);
            salida.Text = $"{resultado.Tipo} · {resultado.Cifrado}\r\n\r\n{resultado.Resultado}";
            registrar(ResultadoAccion.Correcto($"Bridge envió {resultado.Tipo} usando {resultado.Cifrado}."));
        }));
        mensajes.Controls.Add(salida);
        mensajes.Controls.Add(ElementosUi.Texto("AES cifra con una clave local de sesión. Cuántico es una simulación educativa. Esta etapa muestra el envío; todavía no hay red ni intercambio de claves."));
        columnas.Controls.Add(operaciones, 0, 0);
        columnas.Controls.Add(mensajes, 1, 0);
        Controls.Add(columnas);
        ActualizarEstado();
    }
}
