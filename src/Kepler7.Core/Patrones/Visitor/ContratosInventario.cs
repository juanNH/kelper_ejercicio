namespace Kepler7.Core.Patrones.Visitor;

public interface IVisitor
{
    void Visitar(Arma arma);
    void Visitar(Pocion pocion);
    void Visitar(Armadura armadura);
}

public interface IItem
{
    string Nombre { get; }
    int Precio { get; }
    void Aceptar(IVisitor visitante);
    IItem Copiar();
}

public sealed record Arma(string Nombre, int Precio, int Potencia) : IItem
{
    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
    public IItem Copiar() => this with { };
}

public sealed record Pocion(string Nombre, int Precio, int Curacion) : IItem
{
    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
    public IItem Copiar() => this with { };
}

public sealed record Armadura(string Nombre, int Precio, int Proteccion) : IItem
{
    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
    public IItem Copiar() => this with { };
}
