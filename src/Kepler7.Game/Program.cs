using Kepler7.Game.Formularios;

namespace Kepler7.Game;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(ComposicionAplicacion.Crear()));
    }
}
