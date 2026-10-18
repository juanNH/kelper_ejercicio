using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.TemplateMethod;

namespace Kepler7.Game.Controles;

public sealed class TurnosUnidadControl : UserControl
{
    private readonly IServicioTurnos _servicio;
    private readonly Action<ResultadoAccion> _registrar;
    private readonly ComboBox _unidad = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280, DropDownWidth = 530 };
    private readonly TextBox _detalle = ElementosUi.Salida(280, 135);
    private readonly Button _ejecutar;
    private readonly ComboBox _historial = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TreeView _etapas = new() { Dock = DockStyle.Fill, HideSelection = false };
    private bool _actualizando;

    public TurnosUnidadControl(IServicioTurnos servicio, Action<ResultadoAccion> registrar)
    {
        _servicio = servicio;
        _registrar = registrar;
        BindingContext = new BindingContext();
        var estructura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.Controls.Add(ElementosUi.Texto("ESTUDIANTE 1 · Template Method\r\nInicio → Decisión → Acción → Finalización. Las cuatro variantes conservan este orden.", 1050), 0, 0);
        var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var seleccion = ElementosUi.Columna();
        seleccion.Controls.Add(ElementosUi.Texto("Unidad de la misión · aliada o enemiga", 285));
        seleccion.Controls.Add(_unidad);
        seleccion.Controls.Add(_detalle);
        _ejecutar = ElementosUi.Boton("Ejecutar turno de unidad", Ejecutar);
        seleccion.Controls.Add(_ejecutar);
        seleccion.Controls.Add(ElementosUi.Boton("Actualizar unidades", Actualizar));
        seleccion.Controls.Add(ElementosUi.Texto("Soldado: ataca al más cercano o avanza una celda.\r\nMédico: cura hasta 25 PV o se acerca a un herido.\r\nFrancotirador: prioriza al enemigo con menos PV en alcance.\r\nComandante: acerca un aliado para reagruparlo.", 285));
        seleccion.Controls.Add(ElementosUi.Texto("La práctica inicial incluye los cuatro roles y un soldado herido. Los efectos se ven también en Combate. Podés agregar unidades desde Unidades y plantillas.", 285));

        var registro = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        registro.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        registro.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        registro.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        registro.Controls.Add(ElementosUi.Texto("Historial de turnos · seleccioná una ejecución para revisar sus etapas", 750), 0, 0);
        _historial.DisplayMember = nameof(ResultadoTurno.Descripcion);
        registro.Controls.Add(_historial, 0, 1);
        registro.Controls.Add(_etapas, 0, 2);
        columnas.Controls.Add(seleccion, 0, 0);
        columnas.Controls.Add(registro, 1, 0);
        estructura.Controls.Add(columnas, 0, 1);
        Controls.Add(estructura);
        _unidad.SelectedIndexChanged += (_, _) => { if (!_actualizando) MostrarUnidad(); };
        _unidad.FormattingEnabled = true;
        _unidad.Format += (_, e) =>
        {
            if (e.ListItem is Unidad unidad) e.Value = $"{unidad.Nombre} · {unidad.Equipo} · {unidad.Rol} · {unidad.Posicion}";
        };
        _historial.SelectedIndexChanged += (_, _) =>
        {
            if (_historial.SelectedItem is ResultadoTurno resultado) MostrarResultado(resultado);
        };
        Load += (_, _) => Actualizar();
        VisibleChanged += (_, _) => { if (Visible) Actualizar(); };
        Actualizar();
    }

    private void Actualizar()
    {
        Guid? id = (_unidad.SelectedItem as Unidad)?.Id;
        _actualizando = true;
        _unidad.DataSource = _servicio.Unidades.ToArray();
        if (id.HasValue && _servicio.Unidades.Any(u => u.Id == id))
            _unidad.SelectedItem = _servicio.Unidades.First(u => u.Id == id);
        _actualizando = false;
        MostrarUnidad();
    }

    private void MostrarUnidad()
    {
        _ejecutar.Enabled = _unidad.SelectedItem is Unidad;
        _detalle.Text = _unidad.SelectedItem is Unidad unidad
            ? $"{unidad.Nombre} · {unidad.Equipo} · {unidad.Rol}\r\n" +
              $"PV {unidad.Estadisticas.Vida}/{unidad.Estadisticas.VidaMaxima} · Escudo {unidad.Escudo}\r\n" +
              $"Estado {unidad.Estado.Tipo}\r\nPosición {unidad.Posicion} · Alcance {unidad.Estadisticas.Alcance}"
            : "Agregá una unidad a la misión desde Preparación.";
    }

    private void Ejecutar()
    {
        if (_unidad.SelectedItem is not Unidad unidad) return;
        try
        {
            ResultadoTurno resultado = _servicio.EjecutarTurno(unidad.Id);
            _historial.DataSource = _servicio.Historial.ToArray();
            _historial.SelectedItem = resultado;
            MostrarResultado(resultado);
            _registrar(new(resultado.Exito, $"Turno {resultado.Numero} · {resultado.NombreUnidad}: {resultado.Resumen}",
                resultado.Eventos.Select(e => e.Mensaje).ToArray()));
            Actualizar();
        }
        catch (ArgumentException ex) { _registrar(ResultadoAccion.Rechazado(ex.Message)); }
    }

    private void MostrarResultado(ResultadoTurno resultado)
    {
        _etapas.BeginUpdate();
        _etapas.Nodes.Clear();
        foreach (EtapaTurno etapa in Enum.GetValues<EtapaTurno>())
        {
            string nombre = etapa switch
            {
                EtapaTurno.Decision => "Decisión",
                EtapaTurno.Accion => "Acción",
                EtapaTurno.Finalizacion => "Finalización",
                _ => "Inicio"
            };
            TreeNode nodo = _etapas.Nodes.Add($"{(int)etapa + 1}. {nombre}");
            foreach (EventoTurno evento in resultado.Eventos.Where(e => e.Etapa == etapa)) nodo.Nodes.Add(evento.Mensaje);
        }
        _etapas.ExpandAll();
        _etapas.EndUpdate();
    }
}
