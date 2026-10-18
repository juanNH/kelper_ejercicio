namespace Kepler7.Game.Controles;

internal static class ElementosUi
{
    public static FlowLayoutPanel Columna() => new()
    {
        Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown,
        WrapContents = false, Padding = new Padding(12)
    };

    public static Label Texto(string texto, int ancho = 420) => new()
    {
        Text = texto, AutoSize = true, MaximumSize = new Size(ancho, 0), Margin = new Padding(3, 6, 3, 6)
    };

    public static Button Boton(string texto, Action accion)
    {
        var boton = new Button { Text = texto, AutoSize = true, MinimumSize = new Size(130, 32), Margin = new Padding(3, 5, 3, 5) };
        boton.Click += (_, _) => accion();
        return boton;
    }

    public static ComboBox Opciones<T>(IEnumerable<T> opciones) => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList, Width = 245, DataSource = opciones.ToArray()
    };

    public static NumericUpDown Numero(int minimo, int maximo, int inicial) => new()
    {
        Minimum = minimo, Maximum = maximo, Value = inicial, Width = 90
    };

    public static TextBox Salida(int ancho = 420, int alto = 160) => new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        Width = ancho, Height = alto, BackColor = SystemColors.Window
    };
}
