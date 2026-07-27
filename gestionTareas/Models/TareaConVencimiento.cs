namespace GestorTareasApp.Models;

/// <summary>
/// Tarea que además tiene una fecha límite de vencimiento.
/// </summary>
public class TareaConVencimiento : Tarea
{
    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Días restantes hasta el vencimiento. Se calcula en tiempo real cada
    /// vez que se consulta la propiedad (no se almacena).
    /// Si la tarea ya venció, el valor será negativo.
    /// </summary>
    public int DiasRestantes
    {
        get
        {
            TimeSpan diferencia = FechaVencimiento.Date - DateTime.Now.Date;
            return diferencia.Days;
        }
    }

    /// <summary>
    /// Constructor para crear una tarea nueva con vencimiento.
    /// Llama al constructor base para inicializar los campos comunes.
    /// </summary>
    public TareaConVencimiento(string titulo, string descripcion, Prioridad prioridad,
                                string categoria, DateTime fechaVencimiento)
        : base(titulo, descripcion, prioridad, categoria)
    {
        FechaVencimiento = fechaVencimiento;
    }

    /// <summary>
    /// Constructor protegido para reconstruir desde JSON, reutilizando el
    /// constructor protegido de la clase base.
    /// </summary>
    private TareaConVencimiento(int id, string titulo, string descripcion, Prioridad prioridad,
                                 string categoria, bool completada, DateTime fechaCreacion,
                                 DateTime fechaVencimiento)
        : base(id, titulo, descripcion, prioridad, categoria, completada, fechaCreacion)
    {
        FechaVencimiento = fechaVencimiento;
    }

    public static TareaConVencimiento DesdeDatos(int id, string titulo, string descripcion, Prioridad prioridad,
                                                  string categoria, bool completada, DateTime fechaCreacion,
                                                  DateTime fechaVencimiento)
    {
        return new TareaConVencimiento(id, titulo, descripcion, prioridad, categoria,
                                        completada, fechaCreacion, fechaVencimiento);
    }

    /// <summary>
    /// True si la fecha de vencimiento ya pasó y la tarea no está completada.
    /// </summary>
    public bool EstaVencida()
    {
        return !Completada && DateTime.Compare(DateTime.Now, FechaVencimiento) > 0;
    }

    /// <summary>
    /// Sobrescribe MostrarInfo para agregar la fecha de vencimiento y los
    /// días restantes (demuestra polimorfismo respecto a Tarea).
    /// </summary>
    public override void MostrarInfo()
    {
        base.MostrarInfo();

        string estadoVencimiento;
        if (Completada)
        {
            estadoVencimiento = "N/A (completada)";
        }
        else if (DiasRestantes < 0)
        {
            estadoVencimiento = $"VENCIDA hace {Math.Abs(DiasRestantes)} dia(s)";
        }
        else if (DiasRestantes == 0)
        {
            estadoVencimiento = "Vence HOY";
        }
        else
        {
            estadoVencimiento = $"{DiasRestantes} dia(s) restante(s)";
        }

        Console.WriteLine($"    Vence: {FechaVencimiento:dd/MM/yyyy} | Estado: {estadoVencimiento}");
    }

    public override string Exportar()
    {
        return $"{base.Exportar()}|{FechaVencimiento:yyyy-MM-dd}";
    }
}
