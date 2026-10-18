#nullable enable

namespace Kepler7.Game.Formularios;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    private TabControl navegacion = null!;
    private ListBox registro = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var estructura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
        var encabezado = new Label
        {
            Text = "KEPLER-7  |  Primera etapa\r\nPreparación · Estación · Combate táctico",
            Dock = DockStyle.Fill, Padding = new Padding(14, 8, 8, 8),
            BackColor = Color.FromArgb(25, 42, 66), ForeColor = Color.White
        };
        navegacion = new TabControl { Dock = DockStyle.Fill };
        registro = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };
        estructura.Controls.Add(encabezado, 0, 0);
        estructura.Controls.Add(navegacion, 0, 1);
        estructura.Controls.Add(registro, 0, 2);
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1220, 880);
        MinimumSize = new Size(1100, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Kepler-7 · Base de desarrollo";
        Controls.Add(estructura);
        ResumeLayout(false);
    }
}
