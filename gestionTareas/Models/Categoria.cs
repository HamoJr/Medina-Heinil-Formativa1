namespace GestorTareasApp.Models;

/// <summary>
/// Representa una categoría que puede agruparse visualmente con un color.
/// Nota: en Tarea, la categoría se guarda como string (nombre) para simplificar
/// la persistencia, pero esta clase se puede usar para manejar un catálogo
/// de categorías con su color y descripción.
/// </summary>
public class Categoria
{
    public string Nombre { get; set; }
    public string Color { get; set; }
    public string Descripcion { get; set; }

    public Categoria(string nombre, string color, string descripcion)
    {
        Nombre = nombre;
        Color = color;
        Descripcion = descripcion;
    }

    public override string ToString()
    {
        return $"{Nombre} ({Color}) - {Descripcion}";
    }
}
