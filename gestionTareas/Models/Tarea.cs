namespace GestorTareasApp.Models;

/// <summary>
/// Clase base que representa una tarea genérica dentro del sistema.
/// </summary>
public class Tarea : IExportable
{
    // Contador estático usado para generar IDs autoincrementales.
    private static int _contador = 0;

    public int Id { get; private set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Prioridad Prioridad { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Completada { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    /// <summary>
    /// Constructor usado al crear una tarea nueva desde el menú.
    /// Genera un Id nuevo automáticamente y fija la fecha de creación a "ahora".
    /// </summary>
    public Tarea(string titulo, string descripcion, Prioridad prioridad, string categoria)
    {
        Id = ++_contador;
        Titulo = titulo;
        Descripcion = descripcion;
        Prioridad = prioridad;
        Categoria = categoria;
        Completada = false;
        FechaCreacion = DateTime.Now;
    }

    /// <summary>
    /// Constructor "protegido" usado para reconstruir una tarea que viene desde
    /// el archivo JSON (ya tiene Id, estado y fecha de creación definidos).
    /// También sincroniza el contador estático para evitar Ids duplicados.
    /// </summary>
    protected Tarea(int id, string titulo, string descripcion, Prioridad prioridad,
                     string categoria, bool completada, DateTime fechaCreacion)
    {
        Id = id;
        Titulo = titulo;
        Descripcion = descripcion;
        Prioridad = prioridad;
        Categoria = categoria;
        Completada = completada;
        FechaCreacion = fechaCreacion;

        SincronizarContador(id);
    }

    /// <summary>
    /// Fábrica pública que permite a GestorTareas reconstruir una Tarea simple
    /// (no vencimiento) a partir de los datos leídos del JSON.
    /// </summary>
    public static Tarea DesdeDatos(int id, string titulo, string descripcion, Prioridad prioridad,
                                    string categoria, bool completada, DateTime fechaCreacion)
    {
        return new Tarea(id, titulo, descripcion, prioridad, categoria, completada, fechaCreacion);
    }

    protected static void SincronizarContador(int idUsado)
    {
        if (idUsado > _contador)
        {
            _contador = idUsado;
        }
    }

    public void Completar()
    {
        Completada = true;
    }

    /// <summary>
    /// Muestra la información de la tarea en consola. Puede ser sobrescrito
    /// por clases hijas para agregar información adicional (polimorfismo).
    /// </summary>
    public virtual void MostrarInfo()
    {
        Console.WriteLine($"[{Id}] {Titulo} | Prioridad: {Prioridad} | Categoria: {Categoria} | " +
                           $"Completada: {(Completada ? "Si" : "No")} | Creada: {FechaCreacion:dd/MM/yyyy HH:mm}");
        if (!string.IsNullOrWhiteSpace(Descripcion))
        {
            Console.WriteLine($"    Descripcion: {Descripcion}");
        }
    }

    /// <summary>
    /// Implementación de IExportable. Formato: "ID|Titulo|Prioridad|Completada"
    /// </summary>
    public virtual string Exportar()
    {
        return $"{Id}|{Titulo}|{Prioridad}|{Completada}";
    }
}
