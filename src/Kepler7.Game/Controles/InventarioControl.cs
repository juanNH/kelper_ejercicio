using Kepler7.Core.Contratos;
using Kepler7.Core.Modelos;

namespace Kepler7.Game.Controles;

public sealed class InventarioControl : UserControl
{
    private readonly IServicioCombate _combate;
    private readonly ComboBox _unidades = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
    private readonly TextBox _contenido = ElementosUi.Salida(600, 240);

    public InventarioControl(IServicioCombate combate)
    {
        BindingContext = new BindingContext();
        _combate = combate;
        var columna = ElementosUi.Columna();
        columna.Controls.Add(ElementosUi.Texto("Inventario · base para Visitor", 600));
        columna.Controls.Add(ElementosUi.Texto("Los objetos de las unidades ya usan IItem y Aceptar(IVisitor). El siguiente paso es implementar CalculadorPrecio, ContadorArmas y AplicadorVeneno, y conectar IServicioInventario.", 600));
        columna.Controls.Add(_unidades);
        columna.Controls.Add(_contenido);
        _unidades.SelectedIndexChanged += (_, _) => MostrarInventario();
        Controls.Add(columna);
        Actualizar();
    }

    public void Actualizar()
    {
        Guid? id = (_unidades.SelectedItem as Unidad)?.Id;
        _unidades.DataSource = _combate.Mision.Unidades.ToArray();
        if (id.HasValue) _unidades.SelectedItem = _combate.Mision.Unidades.FirstOrDefault(u => u.Id == id);
        MostrarInventario();
    }

    private void MostrarInventario()
    {
        if (_unidades.SelectedItem is Unidad unidad)
            _contenido.Text = string.Join(Environment.NewLine, unidad.Inventario.Select(i => $"{i.Nombre} · Valor {i.Precio}"));
    }
}
