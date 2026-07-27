namespace GestorTareasApp.Models;

/// <summary>
/// Contrato para cualquier clase que pueda exportarse como texto plano.
/// </summary>
public interface IExportable
{
    /// <summary>
    /// Devuelve una representación exportable en formato:
    /// "ID|Titulo|Prioridad|Completada"
    /// </summary>
    string Exportar();
}
