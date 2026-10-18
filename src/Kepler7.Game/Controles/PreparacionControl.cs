using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Flyweight;
using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Patrones.Visitor;
using Kepler7.Game.Formularios;

namespace Kepler7.Game.Controles;

public sealed class PreparacionControl : UserControl
{
    private readonly IPreparacionMision _servicio;
    private readonly Action<Unidad> _incorporar;
    private readonly Action<ResultadoAccion> _registrar;
    private readonly TextBox _nombre = new() { Text = "Recluta orbital", Width = 245, MaxLength = 50 };
    private readonly ComboBox _rol = ElementosUi.Opciones(Enum.GetValues<RolUnidad>());
    private readonly ComboBox _especie;
    private readonly ComboBox _plantilla;
    private readonly ComboBox _escuadron;
    private readonly NumericUpDown _vida = ElementosUi.Numero(1, 500, 100);
    private readonly NumericUpDown _ataque = ElementosUi.Numero(1, 100, 24);
    private readonly NumericUpDown _defensa = ElementosUi.Numero(0, 50, 4);
    private readonly NumericUpDown _alcance = ElementosUi.Numero(1, 14, 3);
    private readonly NumericUpDown _movimiento = ElementosUi.Numero(1, 14, 2);
    private readonly NumericUpDown _x = ElementosUi.Numero(0, 7, 0);
    private readonly NumericUpDown _y = ElementosUi.Numero(0, 7, 0);
    private readonly CheckedListBox _equipo = new() { Width = 300, Height = 90, IntegralHeight = false, CheckOnClick = true };
    private readonly ListBox _candidatas = new() { Width = 420, Height = 85, HorizontalScrollbar = true };
    private readonly TextBox _original = ElementosUi.Salida(420, 140);
    private readonly TextBox _vista = ElementosUi.Salida(420, 140);

    public PreparacionControl(IPreparacionMision servicio, Action<Unidad> incorporar,
        Action<ResultadoAccion> registrar, IServicioTurnos turnos)
    {
        BindingContext = new BindingContext();
        _servicio = servicio;
        _incorporar = incorporar;
        _registrar = registrar;
        _especie = ElementosUi.Opciones(servicio.Especies);
        _especie.DisplayMember = nameof(IEspecie.Nombre);
        _plantilla = ElementosUi.Opciones(servicio.Plantillas);
        _escuadron = ElementosUi.Opciones(servicio.Escuadrones);
        _equipo.Items.AddRange(new object[] { "Rifle de servicio", "Botiquín", "Traje orbital" });
        _equipo.SetItemChecked(0, true);
        _equipo.SetItemChecked(2, true);

        var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var construir = ElementosUi.Columna();
        construir.Controls.Add(ElementosUi.Texto("ESTUDIANTE 1 · Builder / Prototype"));
        construir.Controls.Add(ElementosUi.Texto("1. Identidad y rol"));
        construir.Controls.Add(_nombre);
        construir.Controls.Add(_rol);
        construir.Controls.Add(_especie);
        construir.Controls.Add(ElementosUi.Texto("2. Estadísticas editables"));
        AgregarNumero(construir, "Vida", _vida);
        AgregarNumero(construir, "Ataque", _ataque);
        AgregarNumero(construir, "Defensa", _defensa);
        AgregarNumero(construir, "Alcance", _alcance);
        AgregarNumero(construir, "Movimiento", _movimiento);
        construir.Controls.Add(ElementosUi.Texto("3. Equipamiento"));
        construir.Controls.Add(_equipo);
        construir.Controls.Add(ElementosUi.Texto("4. Posición inicial de unidad, clon o escuadrón (X, Y)"));
        var posicion = new FlowLayoutPanel { AutoSize = true };
        posicion.Controls.AddRange([_x, _y]);
        construir.Controls.Add(posicion);
        construir.Controls.Add(ElementosUi.Boton("Construir unidad", () => Intentar(Construir)));

        var plantillas = ElementosUi.Columna();
        plantillas.Controls.Add(ElementosUi.Texto("Plantilla original (permanece intacta)"));
        plantillas.Controls.Add(_plantilla);
        plantillas.Controls.Add(_original);
        plantillas.Controls.Add(ElementosUi.Boton("Clonar en la posición elegida", () => Intentar(Clonar)));
        plantillas.Controls.Add(ElementosUi.Boton("Crear variante editable…", CrearVariante));
        plantillas.Controls.Add(ElementosUi.Texto("Configuraciones del director"));
        plantillas.Controls.Add(_escuadron);
        plantillas.Controls.Add(ElementosUi.Texto("La posición elegida inicia tres celdas verticales del escuadrón. Podés usar (4,0) en la práctica inicial."));
        plantillas.Controls.Add(ElementosUi.Boton("Preparar escuadrón", () => Intentar(PrepararEscuadron)));
        plantillas.Controls.Add(ElementosUi.Texto("Unidades preparadas · seleccioná una para incorporarla"));
        plantillas.Controls.Add(_candidatas);
        plantillas.Controls.Add(_vista);
        plantillas.Controls.Add(ElementosUi.Boton("Agregar seleccionada a la misión", () =>
        {
            if (_candidatas.SelectedItem is Unidad unidad) _incorporar(unidad);
            else _registrar(ResultadoAccion.Rechazado("Primero construí, cloná o prepará un escuadrón."));
        }));
        plantillas.Controls.Add(ElementosUi.Texto("Práctica inicial: Soldado herido (1,1), Médico (1,2), Francotirador (0,3), Comandante (0,1) y enemigo (3,1). Elegí otra celda para unidades nuevas."));
        _plantilla.SelectedIndexChanged += (_, _) => ActualizarOriginal();
        _candidatas.SelectedIndexChanged += (_, _) =>
        {
            ActualizarCandidata();
        };
        _candidatas.FormattingEnabled = true;
        _candidatas.Format += (_, e) =>
        {
            if (e.ListItem is Unidad unidad) e.Value = $"{unidad.Nombre} · {unidad.Equipo} · {unidad.Rol} · {unidad.Estadisticas.Vida} PV · {unidad.Posicion}";
        };
        columnas.Controls.Add(construir, 0, 0);
        columnas.Controls.Add(plantillas, 1, 0);
        var navegacion = new TabControl { Dock = DockStyle.Fill };
        var unidades = new TabPage("Unidades y plantillas");
        unidades.Controls.Add(columnas);
        var tripulantes = new TabPage("Tripulantes · Flyweight");
        tripulantes.Controls.Add(new TripulantesControl(servicio, registrar) { Dock = DockStyle.Fill });
        var turnosUnidad = new TabPage("Turnos · Template Method");
        turnosUnidad.Controls.Add(new TurnosUnidadControl(turnos, registrar) { Dock = DockStyle.Fill });
        navegacion.TabPages.AddRange([unidades, tripulantes, turnosUnidad]);
        Controls.Add(navegacion);
        Load += (_, _) => { ActualizarOriginal(); ActualizarCandidata(); };
        VisibleChanged += (_, _) => { if (Visible) { ActualizarOriginal(); ActualizarCandidata(); } };
        ActualizarOriginal();
    }

    private void Construir()
    {
        var items = new List<IItem>();
        if (_equipo.GetItemChecked(0)) items.Add(new Arma("Rifle de servicio", 40, 24));
        if (_equipo.GetItemChecked(1)) items.Add(new Pocion("Botiquín", 25, 30));
        if (_equipo.GetItemChecked(2)) items.Add(new Armadura("Traje orbital", 30, 4));
        var configuracion = new ConfiguracionUnidad(_nombre.Text, (RolUnidad)_rol.SelectedItem!,
            (IEspecie)_especie.SelectedItem!, new Estadisticas((int)_vida.Value, (int)_ataque.Value,
                (int)_defensa.Value, (int)_alcance.Value, (int)_movimiento.Value),
            new Posicion((int)_x.Value, (int)_y.Value), items);
        Mostrar([_servicio.CrearUnidad(configuracion)]);
        _registrar(ResultadoAccion.Correcto("Builder completó identidad, rol, estadísticas y equipamiento."));
    }

    private void Clonar()
    {
        Mostrar([_servicio.ClonarPlantilla((string)_plantilla.SelectedItem!, new Posicion((int)_x.Value, (int)_y.Value))]);
        ActualizarOriginal();
        _registrar(ResultadoAccion.Correcto("Prototype creó una copia con ID nuevo y posición independiente."));
    }

    private void PrepararEscuadron()
    {
        Mostrar(_servicio.CrearEscuadron((string)_escuadron.SelectedItem!, new Posicion((int)_x.Value, (int)_y.Value)));
        _registrar(ResultadoAccion.Correcto("DirectorEquipo preparó el escuadrón mediante Builder."));
    }

    private void CrearVariante()
    {
        if (_plantilla.SelectedItem is not string id) return;
        using var editor = new VarianteUnidadForm(_servicio, id, new Posicion((int)_x.Value, (int)_y.Value));
        if (editor.ShowDialog(this) != DialogResult.OK || editor.UnidadCreada is not { } unidad) return;
        Mostrar([unidad]);
        ActualizarOriginal();
        _registrar(ResultadoAccion.Correcto($"Prototype creó la variante {unidad.Nombre}; la plantilla {id} permanece intacta."));
    }

    private void Mostrar(IReadOnlyList<Unidad> unidades) => _candidatas.DataSource = unidades.ToArray();
    private void ActualizarOriginal()
    {
        if (_plantilla.SelectedItem is string id) _original.Text = DescripcionUnidad.Mostrar(_servicio.ConsultarPlantilla(id));
    }
    private void ActualizarCandidata()
    {
        if (_candidatas.SelectedItem is Unidad unidad) _vista.Text = DescripcionUnidad.Mostrar(VistaUnidad.Desde(unidad));
    }

    private static void AgregarNumero(Control columna, string etiqueta, NumericUpDown numero)
    {
        var fila = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        fila.Controls.Add(new Label { Text = etiqueta, Width = 110, Padding = new Padding(0, 4, 0, 0) });
        fila.Controls.Add(numero);
        columna.Controls.Add(fila);
    }

    private void Intentar(Action accion)
    {
        try { accion(); }
        catch (ArgumentException ex) { _registrar(ResultadoAccion.Rechazado(ex.Message)); }
        catch (InvalidOperationException ex) { _registrar(ResultadoAccion.Rechazado(ex.Message)); }
    }
}
