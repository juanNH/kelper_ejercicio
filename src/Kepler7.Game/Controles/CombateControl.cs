using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Game.Controles;

public sealed class CombateControl : UserControl
{
    private readonly IServicioCombate _servicio;
    private readonly Action<ResultadoAccion> _registrar;
    private readonly Dictionary<Posicion, Button> _celdas = [];
    private readonly ComboBox _unidad = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330 };
    private readonly ComboBox _objetivo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330 };
    private readonly ComboBox _estado = ElementosUi.Opciones(Enum.GetValues<TipoEstado>());
    private readonly TextBox _detalle = ElementosUi.Salida(330, 95);
    private readonly CheckedListBox _capas = new() { Width = 245, Height = 108, IntegralHeight = false, CheckOnClick = true };
    private readonly CheckBox _esquivar = new() { Text = "El objetivo esquiva", AutoSize = true };
    private readonly CheckBox _bloquear = new() { Text = "El objetivo bloquea", AutoSize = true };
    private readonly Button _atacar;
    private bool _actualizando;

    public CombateControl(IServicioCombate servicio, Action<ResultadoAccion> registrar)
    {
        BindingContext = new BindingContext();
        _servicio = servicio;
        _registrar = registrar;
        var estructura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var tablero = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 8, RowCount = 8, Padding = new Padding(12) };
        for (int i = 0; i < 8; i++)
        {
            tablero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tablero.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
        }
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            var posicion = new Posicion(x, y);
            var celda = new Button { Dock = DockStyle.Fill, Margin = new Padding(2), FlatStyle = FlatStyle.Flat };
            celda.Click += (_, _) => SeleccionarCelda(posicion);
            _celdas.Add(posicion, celda);
            tablero.Controls.Add(celda, x, y);
        }
        var acciones = ElementosUi.Columna();
        acciones.Controls.Add(ElementosUi.Texto("ESTUDIANTE 3 · State / Chain", 340));
        acciones.Controls.Add(ElementosUi.Texto("Seleccioná una unidad. Pulsá una celda vacía para moverla. A = aliado; E = enemigo. Coordenadas de 0 a 7.", 340));
        acciones.Controls.Add(_unidad);
        acciones.Controls.Add(_detalle);
        acciones.Controls.Add(_estado);
        acciones.Controls.Add(ElementosUi.Boton("Aplicar estado", () =>
        {
            if (_unidad.SelectedItem is Unidad unidad)
                Ejecutar(() => _servicio.CambiarEstado(unidad.Id, (TipoEstado)_estado.SelectedItem!));
        }));
        acciones.Controls.Add(ElementosUi.Texto("Capas activas · desmarcá o reordená. Vida cierra la cadena.", 340));
        foreach (var capa in new[] { CapaDefensa.Esquiva, CapaDefensa.Bloqueo, CapaDefensa.Armadura, CapaDefensa.Escudo })
            _capas.Items.Add(capa, true);
        acciones.Controls.Add(_capas);
        var ordenar = new FlowLayoutPanel { AutoSize = true };
        ordenar.Controls.Add(ElementosUi.Boton("Subir capa", () => Reordenar(-1)));
        ordenar.Controls.Add(ElementosUi.Boton("Bajar capa", () => Reordenar(1)));
        acciones.Controls.Add(ordenar);
        acciones.Controls.Add(_esquivar);
        acciones.Controls.Add(_bloquear);
        acciones.Controls.Add(ElementosUi.Texto("Objetivo del ataque", 340));
        acciones.Controls.Add(_objetivo);
        _atacar = ElementosUi.Boton("Atacar objetivo", Atacar);
        acciones.Controls.Add(_atacar);
        acciones.Controls.Add(ElementosUi.Texto("Para ver el efecto de State, seleccioná también al enemigo y atacá a un aliado. Aturdido no puede actuar; Berserk aumenta ataque y daño recibido.", 340));
        _unidad.SelectedIndexChanged += (_, _) => { if (!_actualizando) ActualizarSeleccion(); };
        estructura.Controls.Add(tablero, 0, 0);
        estructura.Controls.Add(acciones, 1, 0);
        Controls.Add(estructura);
        Actualizar();
    }

    public void Actualizar()
    {
        Guid? id = (_unidad.SelectedItem as Unidad)?.Id;
        _actualizando = true;
        _unidad.DataSource = _servicio.Mision.Unidades.ToArray();
        if (id.HasValue)
            _unidad.SelectedItem = _servicio.Mision.Unidades.FirstOrDefault(u => u.Id == id);
        _actualizando = false;
        ActualizarSeleccion();
    }

    private void ActualizarSeleccion()
    {
        if (_unidad.SelectedItem is not Unidad seleccionada) return;
        _detalle.Text = $"{seleccionada.Nombre} · {seleccionada.Equipo}\r\n" +
            $"PV {seleccionada.Estadisticas.Vida}/{seleccionada.Estadisticas.VidaMaxima} · Escudo {seleccionada.Escudo}\r\n" +
            $"Ataque base {seleccionada.Estadisticas.Ataque} · Alcance {seleccionada.Estadisticas.Alcance}\r\n" +
            $"Estado {seleccionada.Estado.Tipo} · Posición {seleccionada.Posicion}";
        _estado.SelectedItem = seleccionada.Estado.Tipo;
        Guid? objetivoId = (_objetivo.SelectedItem as Unidad)?.Id;
        var objetivos = _servicio.Mision.Unidades.Where(u => u.Equipo != seleccionada.Equipo && u.EstaViva).ToArray();
        _objetivo.DataSource = objetivos;
        if (objetivos.Any(u => u.Id == objetivoId)) _objetivo.SelectedItem = objetivos.First(u => u.Id == objetivoId);
        _atacar.Enabled = seleccionada.EstaViva && objetivos.Length > 0;
        foreach ((Posicion posicion, Button celda) in _celdas)
        {
            Unidad? ocupante = _servicio.Mision.Unidades.FirstOrDefault(u => u.Posicion == posicion && u.EstaViva);
            celda.Text = ocupante is null ? $"{posicion.X},{posicion.Y}"
                : $"{(ocupante.Equipo == Equipo.Aliado ? "A" : "E")} · {ocupante.Rol}\r\n{ocupante.Estadisticas.Vida} PV\r\n{ocupante.Estado.Tipo}";
            celda.BackColor = ocupante is null ? Color.FromArgb(239, 242, 246)
                : ocupante.Id == seleccionada.Id ? Color.FromArgb(33, 72, 111)
                : ocupante.Equipo == Equipo.Aliado ? Color.FromArgb(201, 230, 246) : Color.FromArgb(247, 208, 200);
            celda.ForeColor = ocupante?.Id == seleccionada.Id ? Color.White : Color.Black;
            celda.AccessibleName = $"Celda {posicion}: {ocupante?.Nombre ?? "vacía"}";
        }
    }

    private void SeleccionarCelda(Posicion posicion)
    {
        Unidad? ocupante = _servicio.Mision.Unidades.FirstOrDefault(u => u.Posicion == posicion && u.EstaViva);
        if (ocupante is not null) _unidad.SelectedItem = ocupante;
        else if (_unidad.SelectedItem is Unidad unidad) Ejecutar(() => _servicio.Mover(unidad.Id, posicion));
    }

    private void Atacar()
    {
        if (_unidad.SelectedItem is not Unidad atacante || _objetivo.SelectedItem is not Unidad objetivo) return;
        var orden = _capas.Items.Cast<CapaDefensa>().Where((_, i) => _capas.GetItemChecked(i))
            .Append(CapaDefensa.Vida).ToArray();
        Ejecutar(() => _servicio.Atacar(atacante.Id, objetivo.Id, new(orden, _esquivar.Checked, _bloquear.Checked)));
    }

    private void Ejecutar(Func<ResultadoAccion> accion)
    {
        _registrar(accion());
        Actualizar();
    }

    private void Reordenar(int desplazamiento)
    {
        int origen = _capas.SelectedIndex;
        int destino = origen + desplazamiento;
        if (origen < 0 || destino < 0 || destino >= _capas.Items.Count) return;
        object capa = _capas.Items[origen];
        bool activa = _capas.GetItemChecked(origen);
        _capas.Items.RemoveAt(origen);
        _capas.Items.Insert(destino, capa);
        _capas.SetItemChecked(destino, activa);
        _capas.SelectedIndex = destino;
    }
}
