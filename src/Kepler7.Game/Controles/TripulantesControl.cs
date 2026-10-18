using System.Globalization;
using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Flyweight;

namespace Kepler7.Game.Controles;

public sealed class TripulantesControl : UserControl
{
    private static readonly CultureInfo Formato = CultureInfo.GetCultureInfo("es-AR");
    private readonly IPreparacionMision _servicio;
    private readonly Action<ResultadoAccion> _registrar;
    private readonly Button _generar = new() { Text = "Generar 10.000 tripulantes", AutoSize = true, Height = 34 };
    private readonly Label _resumen = ElementosUi.Texto("Todavía no se generó la población.", 1050);
    private readonly Label _comparacion = ElementosUi.Texto("La comparación cuenta objetos Especie reales en dos poblaciones equivalentes.", 1050);
    private readonly Label _detalle = ElementosUi.Texto("Seleccioná un tripulante para distinguir sus datos propios de la especie compartida.", 1050);
    private readonly DataGridView _especies = CrearTabla();
    private readonly DataGridView _tripulantes = CrearTabla();
    private DemostracionFlyweight? _demostracion;

    public TripulantesControl(IPreparacionMision servicio, Action<ResultadoAccion> registrar)
    {
        _servicio = servicio;
        _registrar = registrar;
        BindingContext = new BindingContext();
        var estructura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        var encabezado = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(0, 0, 0, 8)
        };
        encabezado.Controls.Add(ElementosUi.Texto("ESTUDIANTE 1 · Flyweight · Biblioteca de tripulantes", 1050));
        encabezado.Controls.Add(_generar);
        encabezado.Controls.Add(_resumen);
        encabezado.Controls.Add(_comparacion);

        var tablas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        tablas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 345));
        tablas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tablas.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tablas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tablas.Controls.Add(ElementosUi.Texto("Especies compartidas · una instancia por especie", 335), 0, 0);
        tablas.Controls.Add(ElementosUi.Texto("Población completa · desplazate para ver los 10.000", 650), 1, 0);

        _especies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Especie", DataPropertyName = nameof(ResumenEspecie.Nombre), FillWeight = 50 });
        _especies.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tripulantes", DataPropertyName = nameof(ResumenEspecie.CantidadTripulantes), FillWeight = 32,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", FormatProvider = Formato }
        });
        _especies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Objetos", DataPropertyName = nameof(ResumenEspecie.InstanciasCompartidas), FillWeight = 25 });
        _tripulantes.VirtualMode = true;
        _tripulantes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N.º", FillWeight = 16 });
        _tripulantes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", FillWeight = 58 });
        _tripulantes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rango", FillWeight = 35 });
        _tripulantes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Especie", FillWeight = 45 });
        foreach (DataGridViewColumn columna in _tripulantes.Columns) columna.SortMode = DataGridViewColumnSortMode.NotSortable;
        _tripulantes.CellValueNeeded += MostrarCelda;
        _tripulantes.CurrentCellChanged += (_, _) => MostrarDetalle();
        tablas.Controls.Add(_especies, 0, 1);
        tablas.Controls.Add(_tripulantes, 1, 1);
        var pie = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), AutoScroll = true };
        _detalle.Dock = DockStyle.Top;
        pie.Controls.Add(_detalle);
        estructura.Controls.Add(encabezado, 0, 0);
        estructura.Controls.Add(tablas, 0, 1);
        estructura.Controls.Add(pie, 0, 2);
        Controls.Add(estructura);
        _generar.Click += async (_, _) => await GenerarAsync();
    }

    private async Task GenerarAsync()
    {
        _generar.Enabled = false;
        _resumen.Text = "Generando y contando referencias de especie…";
        try
        {
            DemostracionFlyweight resultado = await Task.Run(_servicio.GenerarDemostracionTripulantes);
            if (IsDisposed || Disposing) return;
            _tripulantes.RowCount = 0;
            _demostracion = resultado;
            _especies.DataSource = resultado.Especies.ToArray();
            _tripulantes.RowCount = resultado.TotalTripulantes;
            _tripulantes.Invalidate();
            _resumen.Text = $"Tripulantes: {resultado.TotalTripulantes.ToString("N0", Formato)} · " +
                $"Especies compartidas: {resultado.EspeciesConFlyweight} · Creadas en la fábrica: {resultado.EspeciesCreadasEnFabrica}";
            _comparacion.Text = $"Con Flyweight: {resultado.EspeciesConFlyweight} objetos Especie. " +
                $"Sin Flyweight: {resultado.EspeciesSinFlyweight.ToString("N0", Formato)}. " +
                $"Se evitan {resultado.ObjetosEspecieEvitados.ToString("N0", Formato)} objetos " +
                $"({resultado.PorcentajeReduccion.ToString("N2", Formato)} %). Es un conteo de objetos, no de memoria RAM.";
            MostrarDetalle();
            _registrar(ResultadoAccion.Correcto("Flyweight: 10.000 tripulantes comparten cinco objetos de especie.",
                $"Comparación real: {resultado.EspeciesConFlyweight} objetos compartidos / {resultado.EspeciesSinFlyweight.ToString("N0", Formato)} individuales."));
        }
        catch (InvalidOperationException ex)
        {
            if (IsDisposed || Disposing) return;
            _resumen.Text = "No se pudo generar la demostración.";
            _registrar(ResultadoAccion.Rechazado(ex.Message));
        }
        finally
        {
            if (!IsDisposed && !Disposing) _generar.Enabled = true;
        }
    }

    private void MostrarCelda(object? sender, DataGridViewCellValueEventArgs e)
    {
        if (_demostracion is null || e.RowIndex < 0 || e.RowIndex >= _demostracion.TotalTripulantes) return;
        Tripulante tripulante = _demostracion.Tripulantes[e.RowIndex];
        e.Value = e.ColumnIndex switch
        {
            0 => e.RowIndex + 1,
            1 => tripulante.Nombre,
            2 => tripulante.Rango,
            3 => tripulante.Especie.Nombre,
            _ => null
        };
    }

    private void MostrarDetalle()
    {
        int indice = _tripulantes.CurrentCell?.RowIndex ?? -1;
        if (_demostracion is null || indice < 0 || indice >= _demostracion.TotalTripulantes) return;
        Tripulante tripulante = _demostracion.Tripulantes[indice];
        _detalle.Text = $"Datos propios: {tripulante.Nombre} · {tripulante.Rango} · ID {tripulante.Id}\r\n" +
            $"Datos compartidos: {tripulante.Especie.Nombre} · Color {tripulante.Especie.Color} · " +
            $"Planeta {tripulante.Especie.PlanetaOrigen} · Dieta {tripulante.Especie.Dieta}";
    }

    private static DataGridView CrearTabla() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AllowUserToOrderColumns = false, RowHeadersVisible = false, MultiSelect = false,
        AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = SystemColors.Window, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
    };
}
