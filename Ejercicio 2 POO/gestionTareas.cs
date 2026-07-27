using System.Text.Json;
using System.Text.Json.Serialization;
using GestorTareasApp.Models;

namespace GestorTareasApp;

/// <summary>
/// DTO (objeto de transferencia) usado únicamente para serializar/deserializar
/// a JSON. Se usa un campo "Tipo" como discriminador manual, ya que
/// System.Text.Json no maneja polimorfismo de forma automática sin
/// configuración especial.
/// </summary>
internal class TareaDto
{
    public string Tipo { get; set; } = "Tarea";
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Prioridad Prioridad { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Completada { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Solo aplica cuando Tipo == "TareaConVencimiento"
    public DateTime? FechaVencimiento { get; set; }
}

/// <summary>
/// Administra la colección de tareas: alta, baja, filtros y persistencia en JSON.
/// </summary>
public class GestorTareas
{
    private readonly List<Tarea> _tareas = new();

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Acceso de solo lectura a la lista completa de tareas (encapsulamiento).
    /// </summary>
    public IReadOnlyList<Tarea> Tareas => _tareas.AsReadOnly();

    public void Agregar(Tarea tarea)
    {
        ArgumentNullException.ThrowIfNull(tarea);
        _tareas.Add(tarea);
    }

    /// <summary>
    /// Marca como completada la tarea con el Id indicado.
    /// Devuelve true si la tarea fue encontrada y actualizada.
    /// </summary>
    public bool Completar(int id)
    {
        Tarea? tarea = _tareas.FirstOrDefault(t => t.Id == id);
        if (tarea is null)
        {
            return false;
        }

        tarea.Completar();
        return true;
    }

    public List<Tarea> ListarPorCategoria(string categoria)
    {
        return _tareas
            .Where(t => string.Equals(t.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Tarea> ListarPorPrioridad(Prioridad prioridad)
    {
        return _tareas.Where(t => t.Prioridad == prioridad).ToList();
    }

    /// <summary>
    /// Devuelve las tareas con vencimiento cuya fecha límite ya pasó y que
    /// aún no han sido completadas.
    /// </summary>
    public List<Tarea> ObtenerVencidas()
    {
        return _tareas
            .OfType<TareaConVencimiento>()
            .Where(t => t.EstaVencida())
            .Cast<Tarea>()
            .ToList();
    }

    /// <summary>
    /// Elimina la tarea con el Id indicado. Devuelve true si existía.
    /// </summary>
    public bool Eliminar(int id)
    {
        Tarea? tarea = _tareas.FirstOrDefault(t => t.Id == id);
        if (tarea is null)
        {
            return false;
        }

        _tareas.Remove(tarea);
        return true;
    }

    /// <summary>
    /// Guarda todas las tareas en un archivo JSON, incluyendo un discriminador
    /// de tipo para poder reconstruir Tarea vs TareaConVencimiento al cargar.
    /// </summary>
    public void GuardarEnJSON(string archivo)
    {
        List<TareaDto> dtos = _tareas.Select(MapearADto).ToList();
        string json = JsonSerializer.Serialize(dtos, OpcionesJson);
        File.WriteAllText(archivo, json);
    }

    /// <summary>
    /// Carga las tareas desde un archivo JSON. Si el archivo no existe o está
    /// corrupto, se maneja el error, se informa por consola y se devuelve una
    /// lista vacía sin interrumpir la ejecución del programa.
    /// </summary>
    public List<Tarea> CargarDeJSON(string archivo)
    {
        _tareas.Clear();

        try
        {
            if (!File.Exists(archivo))
            {
                return _tareas;
            }

            string json = File.ReadAllText(archivo);
            if (string.IsNullOrWhiteSpace(json))
            {
                return _tareas;
            }

            List<TareaDto>? dtos = JsonSerializer.Deserialize<List<TareaDto>>(json, OpcionesJson);
            if (dtos is null)
            {
                return _tareas;
            }

            foreach (TareaDto dto in dtos)
            {
                _tareas.Add(MapearDesdeDto(dto));
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Aviso: el archivo '{archivo}' esta corrupto y no se pudo leer ({ex.Message}). " +
                               "Se iniciara con una lista vacia.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Aviso: no se pudo leer el archivo '{archivo}' ({ex.Message}). " +
                               "Se iniciara con una lista vacia.");
        }

        return _tareas;
    }

    private static TareaDto MapearADto(Tarea tarea)
    {
        var dto = new TareaDto
        {
            Tipo = tarea is TareaConVencimiento ? "TareaConVencimiento" : "Tarea",
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Prioridad = tarea.Prioridad,
            Categoria = tarea.Categoria,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion
        };

        if (tarea is TareaConVencimiento conVencimiento)
        {
            dto.FechaVencimiento = conVencimiento.FechaVencimiento;
        }

        return dto;
    }

    private static Tarea MapearDesdeDto(TareaDto dto)
    {
        if (dto.Tipo == "TareaConVencimiento" && dto.FechaVencimiento.HasValue)
        {
            return TareaConVencimiento.DesdeDatos(
                dto.Id, dto.Titulo, dto.Descripcion, dto.Prioridad, dto.Categoria,
                dto.Completada, dto.FechaCreacion, dto.FechaVencimiento.Value);
        }

        return Tarea.DesdeDatos(
            dto.Id, dto.Titulo, dto.Descripcion, dto.Prioridad, dto.Categoria,
            dto.Completada, dto.FechaCreacion);
    }
}
