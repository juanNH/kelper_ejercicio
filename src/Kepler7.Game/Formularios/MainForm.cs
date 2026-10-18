using Kepler7.Core.Modelos;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Formularios;

public partial class MainForm : Form
{
    public MainForm() : this(ComposicionAplicacion.Crear()) { }

    internal MainForm(ServiciosAplicacion servicios)
    {
        InitializeComponent();
        var combate = new CombateControl(servicios.Combate, Registrar);
        var inventario = new InventarioControl(servicios.Combate);
        var preparacion = new PreparacionControl(servicios.Preparacion, unidad =>
        {
            Registrar(servicios.Combate.IncorporarUnidad(unidad));
            combate.Actualizar();
            inventario.Actualizar();
        }, resultado =>
        {
            Registrar(resultado);
            combate.Actualizar();
            inventario.Actualizar();
        }, servicios.Turnos);
        AgregarPagina("Preparación", preparacion);
        AgregarPagina("Estación", new EstacionControl(servicios.Estacion, servicios.Comunicaciones, Registrar));
        AgregarPagina("Combate", combate);
        AgregarPagina("Inventario", inventario);
        AgregarPagina("Próximos pasos", new ProximosPasosControl());
        navegacion.SelectedIndexChanged += (_, _) => { combate.Actualizar(); inventario.Actualizar(); };
        Registrar(ResultadoAccion.Correcto("Base lista. Construí o cloná una unidad; luego abrí Combate."));
    }

    private void AgregarPagina(string titulo, UserControl control)
    {
        var pagina = new TabPage(titulo);
        control.Dock = DockStyle.Fill;
        pagina.Controls.Add(control);
        navegacion.TabPages.Add(pagina);
    }

    private void Registrar(ResultadoAccion resultado)
    {
        registro.Items.Add($"{DateTime.Now:HH:mm:ss}  {(resultado.Exito ? "OK" : "AVISO")}  {resultado.Mensaje}");
        foreach (string evento in resultado.Eventos) registro.Items.Add($"       {evento}");
        registro.TopIndex = registro.Items.Count - 1;
    }
}
