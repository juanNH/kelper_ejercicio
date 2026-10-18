namespace Kepler7.Core.Patrones.Command;

// Siguiente etapa: cuatro comandos concretos y GestorComandos con undo/redo.
public interface IComando
{
    string Descripcion { get; }
    void Ejecutar();
    void Deshacer();
}
