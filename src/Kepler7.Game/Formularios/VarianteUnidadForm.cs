using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Patrones.Visitor;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Formularios;

public sealed class VarianteUnidadForm : Form
{
    private readonly IPreparacionMision _servicio;
    private readonly string _plantillaId;
    private readonly VistaUnidad _original;
    private readonly TextBox _nombre = new() { Width = 300, MaxLength = 50 };
    private readonly NumericUpDown _vidaMaxima;
    private readonly NumericUpDown _vidaActual;
    private readonly NumericUpDown _ataque;
    private readonly NumericUpDown _defensa;
    private readonly NumericUpDown _alcance;
    private readonly NumericUpDown _movimiento;
    private readonly NumericUpDown _x;
    private readonly NumericUpDown _y;
    private readonly CheckedListBox _equipamiento = new()
    {
        Width = 330, Height = 120, IntegralHeight = false, CheckOnClick = true, DisplayMember = nameof(IItem.Nombre)
    };
    private readonly TextBox _vista = ElementosUi.Salida(440, 190);
    private readonly Label _error = ElementosUi.Texto("", 440);
    public Unidad? UnidadCreada { get; private set; }

    public VarianteUnidadForm(IPreparacionMision servicio, string plantillaId, Posicion posicion)
    {
        _servicio = servicio;
        _plantillaId = plantillaId;
        _original = servicio.ConsultarPlantilla(plantillaId);
        _nombre.Text = $"{_original.Nombre} - variante";
        _vidaMaxima = ElementosUi.Numero(1, 500, _original.VidaMaxima);
        _vidaActual = ElementosUi.Numero(0, _original.VidaMaxima, _original.Vida);
        _ataque = ElementosUi.Numero(0, 100, _original.Ataque);
        _defensa = ElementosUi.Numero(0, 50, _original.Defensa);
        _alcance = ElementosUi.Numero(1, 14, _original.Alcance);
        _movimiento = ElementosUi.Numero(1, 14, _original.Movimiento);
        _x = ElementosUi.Numero(0, 7, posicion.X);
        _y = ElementosUi.Numero(0, 7, posicion.Y);
        foreach (IItem item in _original.Equipamiento) _equipamiento.Items.Add(item, true);
        IItem[] opcionales = [new Arma("Rifle de servicio", 40, 24), new Pocion("Botiquín", 25, 30), new Armadura("Traje orbital", 30, 4)];
        foreach (IItem item in opcionales)
            if (!_original.Equipamiento.Any(i => i.GetType() == item.GetType() && i.Nombre == item.Nombre))
                _equipamiento.Items.Add(item, false);

        var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var editar = ElementosUi.Columna();
        editar.Controls.Add(ElementosUi.Texto("Datos propios de la variante", 330));
        editar.Controls.Add(_nombre);
        AgregarNumero(editar, "Vida máxima", _vidaMaxima);
        AgregarNumero(editar, "Vida actual", _vidaActual);
        AgregarNumero(editar, "Ataque", _ataque);
        AgregarNumero(editar, "Defensa", _defensa);
        AgregarNumero(editar, "Alcance", _alcance);
        AgregarNumero(editar, "Movimiento", _movimiento);
        AgregarNumero(editar, "Posición X", _x);
        AgregarNumero(editar, "Posición Y", _y);
        editar.Controls.Add(ElementosUi.Texto("Equipamiento de la copia", 330));
        editar.Controls.Add(_equipamiento);
        var comparar = ElementosUi.Columna();
        comparar.Controls.Add(ElementosUi.Texto("Plantilla original · conserva su configuración", 440));
        var original = ElementosUi.Salida(440, 165);
        original.Text = DescripcionUnidad.Mostrar(_original);
        comparar.Controls.Add(original);
        comparar.Controls.Add(ElementosUi.Texto("Vista previa de la variante", 440));
        comparar.Controls.Add(_vista);
        comparar.Controls.Add(ElementosUi.Texto("La copia recibe un ID nuevo y conserva rol, equipo, especie, escudo y una copia independiente del estado de la plantilla.", 440));
        _error.ForeColor = Color.DarkRed;
        comparar.Controls.Add(_error);
        var crear = ElementosUi.Boton("Crear variante", Crear);
        var cancelar = ElementosUi.Boton("Cancelar", () => { DialogResult = DialogResult.Cancel; Close(); });
        comparar.Controls.Add(crear);
        comparar.Controls.Add(cancelar);
        AcceptButton = crear;
        CancelButton = cancelar;
        columnas.Controls.Add(editar, 0, 0);
        columnas.Controls.Add(comparar, 1, 0);
        Controls.Add(columnas);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 690);
        MinimumSize = new Size(880, 650);
        StartPosition = FormStartPosition.CenterParent;
        Text = $"Prototype · Variante de {plantillaId}";

        _vidaMaxima.ValueChanged += (_, _) => _vidaActual.Maximum = _vidaMaxima.Value;
        foreach (NumericUpDown numero in new[] { _vidaMaxima, _vidaActual, _ataque, _defensa, _alcance, _movimiento, _x, _y })
            numero.ValueChanged += (_, _) => ActualizarVista();
        _nombre.TextChanged += (_, _) => ActualizarVista();
        _equipamiento.ItemCheck += (_, e) => ActualizarVista(e.Index, e.NewValue == CheckState.Checked);
        ActualizarVista();
    }

    private IReadOnlyList<IItem> EquipamientoSeleccionado(int pendiente = -1, bool seleccionado = false) =>
        _equipamiento.Items.Cast<IItem>().Where((_, i) => i == pendiente ? seleccionado : _equipamiento.GetItemChecked(i)).ToArray();

    private void ActualizarVista(int pendiente = -1, bool seleccionado = false)
    {
        _error.Text = "";
        _vista.Text = $"{_nombre.Text} · {_original.Rol} · {_original.Equipo} · {_original.Especie}\r\n" +
            $"ID nuevo al confirmar\r\nPV {_vidaActual.Value}/{_vidaMaxima.Value} · Ataque {_ataque.Value} · Defensa {_defensa.Value}\r\n" +
            $"Alcance {_alcance.Value} · Movimiento {_movimiento.Value} · Escudo {_original.Escudo}\r\n" +
            $"Posición ({_x.Value}, {_y.Value}) · Estado {_original.Estado}\r\n" +
            $"Equipamiento: {string.Join(", ", EquipamientoSeleccionado(pendiente, seleccionado).Select(i => i.Nombre))}";
    }

    private void Crear()
    {
        if (string.IsNullOrWhiteSpace(_nombre.Text))
        {
            _error.Text = "Ingresá un nombre para la variante.";
            return;
        }
        try
        {
            var estadisticas = new Estadisticas((int)_vidaMaxima.Value, (int)_ataque.Value, (int)_defensa.Value,
                (int)_alcance.Value, (int)_movimiento.Value);
            estadisticas.AplicarDanio(estadisticas.VidaMaxima - (int)_vidaActual.Value);
            UnidadCreada = _servicio.CrearVariante(_plantillaId, new(_nombre.Text, estadisticas,
                new Posicion((int)_x.Value, (int)_y.Value), EquipamientoSeleccionado()));
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex) { _error.Text = ex.Message; }
    }

    private static void AgregarNumero(Control columna, string etiqueta, NumericUpDown numero)
    {
        var fila = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        fila.Controls.Add(new Label { Text = etiqueta, Width = 140, Padding = new Padding(0, 4, 0, 0) });
        fila.Controls.Add(numero);
        columna.Controls.Add(fila);
    }
}
