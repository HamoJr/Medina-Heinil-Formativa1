using GestorTareasApp.Models;

namespace GestorTareasApp;

internal static class Program
{
    private const string ArchivoJson = "tareas.json";

    private static void Main()
    {
        var gestor = new GestorTareas();
        gestor.CargarDeJSON(ArchivoJson);

        bool salir = false;

        while (!salir)
        {
            MostrarMenu();
            string opcion = Console.ReadLine() ?? string.Empty;

            switch (opcion.Trim())
            {
                case "1":
                    AgregarTarea(gestor);
                    break;
                case "2":
                    ListarPolimorficamente(gestor.Tareas);
                    break;
                case "3":
                    ListarPorCategoria(gestor);
                    break;
                case "4":
                    ListarPorPrioridad(gestor);
                    break;
                case "5":
                    MarcarCompletada(gestor);
                    break;
                case "6":
                    MostrarVencidas(gestor);
                    break;
                case "7":
                    EliminarTarea(gestor);
                    break;
                case "8":
                    ExportarAJson(gestor);
                    break;
                case "9":
                    salir = true;
                    break;
                default:
                    Console.WriteLine("Opcion invalida. Intenta de nuevo.");
                    break;
            }
        }

        // Persistencia automatica al cerrar el programa.
        gestor.GuardarEnJSON(ArchivoJson);
        Console.WriteLine("Datos guardados en tareas.json. Hasta luego!");
    }

    private static void MostrarMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== GESTOR DE TAREAS ===");
        Console.WriteLine("1. Agregar tarea");
        Console.WriteLine("2. Listar todas");
        Console.WriteLine("3. Listar por categoria");
        Console.WriteLine("4. Listar por prioridad");
        Console.WriteLine("5. Marcar como completada");
        Console.WriteLine("6. Mostrar tareas vencidas");
        Console.WriteLine("7. Eliminar tarea");
        Console.WriteLine("8. Exportar a JSON");
        Console.WriteLine("9. Salir");
        Console.Write("Selecciona una opcion: ");
    }

    private static void AgregarTarea(GestorTareas gestor)
    {
        Console.Write("Titulo: ");
        string titulo = Console.ReadLine() ?? string.Empty;

        Console.Write("Descripcion: ");
        string descripcion = Console.ReadLine() ?? string.Empty;

        Prioridad prioridad = PedirPrioridad();

        Console.Write("Categoria: ");
        string categoria = Console.ReadLine() ?? string.Empty;

        Console.Write("Tiene fecha de vencimiento? (s/n): ");
        string respuesta = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

        if (respuesta == "s")
        {
            Console.Write("Fecha de vencimiento (dd/MM/yyyy): ");
            string fechaTexto = Console.ReadLine() ?? string.Empty;

            if (DateTime.TryParse(fechaTexto, out DateTime fechaVencimiento))
            {
                var tarea = new TareaConVencimiento(titulo, descripcion, prioridad, categoria, fechaVencimiento);
                gestor.Agregar(tarea);
                Console.WriteLine($"Tarea con vencimiento creada (Id: {tarea.Id}).");
            }
            else
            {
                Console.WriteLine("Fecha invalida. Se creara la tarea sin vencimiento.");
                var tarea = new Tarea(titulo, descripcion, prioridad, categoria);
                gestor.Agregar(tarea);
                Console.WriteLine($"Tarea creada (Id: {tarea.Id}).");
            }
        }
        else
        {
            var tarea = new Tarea(titulo, descripcion, prioridad, categoria);
            gestor.Agregar(tarea);
            Console.WriteLine($"Tarea creada (Id: {tarea.Id}).");
        }
    }

    private static Prioridad PedirPrioridad()
    {
        Console.WriteLine("Prioridad: 1) Baja  2) Media  3) Alta  4) Critica");
        Console.Write("Selecciona: ");
        string entrada = (Console.ReadLine() ?? string.Empty).Trim();

        return entrada switch
        {
            "1" => Prioridad.Baja,
            "2" => Prioridad.Media,
            "3" => Prioridad.Alta,
            "4" => Prioridad.Critica,
            _ => Prioridad.Media
        };
    }

    private static void ListarPolimorficamente(IReadOnlyList<Tarea> tareas)
    {
        if (tareas.Count == 0)
        {
            Console.WriteLine("No hay tareas registradas.");
            return;
        }

        Console.WriteLine($"\n--- Listado de tareas ({tareas.Count}) ---");
        foreach (Tarea tarea in tareas)
        {
            tarea.MostrarInfo();
            Console.WriteLine(new string('-', 40));
        }
    }

    private static void ListarPorCategoria(GestorTareas gestor)
    {
        Console.Write("Categoria a buscar: ");
        string categoria = Console.ReadLine() ?? string.Empty;
        List<Tarea> resultado = gestor.ListarPorCategoria(categoria);
        ListarPolimorficamente(resultado);
    }

    private static void ListarPorPrioridad(GestorTareas gestor)
    {
        Prioridad prioridad = PedirPrioridad();
        List<Tarea> resultado = gestor.ListarPorPrioridad(prioridad);
        ListarPolimorficamente(resultado);
    }

    private static void MarcarCompletada(GestorTareas gestor)
    {
        Console.Write("Id de la tarea a completar: ");
        if (int.TryParse(Console.ReadLine(), out int id) && gestor.Completar(id))
        {
            Console.WriteLine("Tarea marcada como completada.");
        }
        else
        {
            Console.WriteLine("No se encontro una tarea con ese Id.");
        }
    }

    private static void MostrarVencidas(GestorTareas gestor)
    {
        List<Tarea> vencidas = gestor.ObtenerVencidas();
        if (vencidas.Count == 0)
        {
            Console.WriteLine("No hay tareas vencidas.");
            return;
        }

        Console.WriteLine($"\n--- Tareas vencidas ({vencidas.Count}) ---");
        foreach (Tarea tarea in vencidas)
        {
            tarea.MostrarInfo();
            Console.WriteLine(new string('-', 40));
        }
    }

    private static void EliminarTarea(GestorTareas gestor)
    {
        Console.Write("Id de la tarea a eliminar: ");
        if (int.TryParse(Console.ReadLine(), out int id) && gestor.Eliminar(id))
        {
            Console.WriteLine("Tarea eliminada.");
        }
        else
        {
            Console.WriteLine("No se encontro una tarea con ese Id.");
        }
    }

    private static void ExportarAJson(GestorTareas gestor)
    {
        gestor.GuardarEnJSON(ArchivoJson);
        Console.WriteLine($"Tareas exportadas a '{ArchivoJson}'.");

        Console.WriteLine("Representacion via IExportable:");
        foreach (Tarea tarea in gestor.Tareas)
        {
            Console.WriteLine(tarea.Exportar());
        }
    }
}