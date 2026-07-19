using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ValidadorTarjetas
{
    
    class RegistroTarjeta
    {
        public string Numero { get; set; } = "";
        public bool EsValida { get; set; }
        public string Marca { get; set; } = "";
    }

    class Program
    {
        
        static List<RegistroTarjeta> historial = new List<RegistroTarjeta>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            int opcion;

            do
            {
                MostrarMenu();
                opcion = LeerOpcionMenu();

                try
                {
                    switch (opcion)
                    {
                        case 1:
                            OpcionValidarUnaTarjeta();
                            break;
                        case 2:
                            OpcionValidarDesdeArchivo();
                            break;
                        case 3:
                            OpcionGenerarNumeroValido();
                            break;
                        case 4:
                            MostrarEstadisticas();
                            break;
                        case 5:
                            Console.WriteLine("\n ¡Hasta luego!");
                            break;
                        default:
                            Console.WriteLine("\nOpcion invalida. Intente nuevamente.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"\n Ocurrio un error inesperado: {ex.Message}");
                }

                if (opcion != 5)
                {
                    Console.WriteLine("\nPresione ENTER para continuar...");
                    Console.ReadLine();
                }

            } while (opcion != 5);
        }

       

        static void MostrarMenu()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                
                
            }

            Console.WriteLine("=== VALIDADOR DE TARJETAS ===");
            Console.WriteLine("1. Validar una tarjeta");
            Console.WriteLine("2. Validar desde archivo");
            Console.WriteLine("3. Generar número válido");
            Console.WriteLine("4. Estadísticas");
            Console.WriteLine("5. Salir");
            Console.Write("\nSeleccione una opción: ");
        }

        static int LeerOpcionMenu()
        {
            string? entrada = Console.ReadLine();
            if (int.TryParse(entrada, out int opcion))
                return opcion;

            
            return -1;
        }

    

        static void OpcionValidarUnaTarjeta()
        {
            Console.Write("\nIngrese el numero de tarjeta: ");
            string numero = Console.ReadLine() ?? "";
            ProcesarTarjeta(numero);
        }

        static void OpcionValidarDesdeArchivo()
        {
            Console.Write("\nIngrese la ruta del archivo: ");
            string ruta = Console.ReadLine() ?? "";
            ValidarDesdeArchivo(ruta);
        }

        static void OpcionGenerarNumeroValido()
        {
            string numeroGenerado = GenerarNumeroValido();
            string marca = IdentificarMarca(numeroGenerado);

            Console.WriteLine("\n--- Numero generado ---");
            Console.WriteLine($"Numero: {numeroGenerado}");
            Console.WriteLine($"Marca: {marca}");
            Console.WriteLine("Estado:  VALIDA (generado para pasar Luhn)");
        }

    
        static bool ProcesarTarjeta(string numeroOriginal)
        {
            string numeroLimpio = LimpiarNumero(numeroOriginal);

            if (!EsNumerico(numeroLimpio) || numeroLimpio.Length == 0)
            {
                Console.WriteLine($"\nNumero: {numeroOriginal}");
                Console.WriteLine("Estado:  INVALIDA (formato no numerico)");
                historial.Add(new RegistroTarjeta { Numero = numeroOriginal, EsValida = false, Marca = "Desconocida" });
                return false;
            }

            bool valida = ValidarTarjeta(numeroLimpio);
            string marca = IdentificarMarca(numeroLimpio);

            Console.WriteLine($"\nNumero: {numeroLimpio}");
            Console.WriteLine($"Marca: {marca}");
            Console.WriteLine(valida ? "Estado:  VALIDA" : "Estado:  INVALIDA");

            historial.Add(new RegistroTarjeta { Numero = numeroLimpio, EsValida = valida, Marca = marca });
            return valida;
        }

      
        static bool ValidarTarjeta(string numero)
        {
            string limpio = LimpiarNumero(numero);

            if (!EsNumerico(limpio) || limpio.Length < 13 || limpio.Length > 19)
                return false;

            char[] invertido = limpio.Reverse().ToArray();
            int suma = 0;

            for (int i = 0; i < invertido.Length; i++)
            {
                int digito = (int)char.GetNumericValue(invertido[i]);

                if ((i + 1) % 2 == 0)
                {
                    digito *= 2;
                    if (digito >= 10)
                        digito = digito / 10 + digito % 10;
                }

                suma += digito;
            }

            return suma % 10 == 0;
        }

        
        static string IdentificarMarca(string numero)
        {
            string limpio = LimpiarNumero(numero);
            int len = limpio.Length;

            if (!EsNumerico(limpio) || len == 0)
                return "Desconocida";

            
            if (limpio.StartsWith("4") && (len == 13 || len == 16))
                return "Visa";

            
            if (len == 16 && len >= 2)
            {
                int prefijo2 = int.Parse(limpio.Substring(0, 2));
                if (prefijo2 >= 51 && prefijo2 <= 55)
                    return "Mastercard";
            }

            
            if ((limpio.StartsWith("34") || limpio.StartsWith("37")) && len == 15)
                return "American Express";

            
            if (len >= 16 && len <= 19)
            {
                if (limpio.StartsWith("6011") || limpio.StartsWith("65"))
                    return "Discover";

                if (limpio.Length >= 3)
                {
                    string tresDigitos = limpio.Substring(0, 3);
                    string[] prefijosDiscover644a649 = { "644", "645", "646", "647", "648", "649" };
                    if (prefijosDiscover644a649.Contains(tresDigitos))
                        return "Discover";
                }

                if (limpio.Length >= 6)
                {
                    int prefijo6 = int.Parse(limpio.Substring(0, 6));
                    if (prefijo6 >= 622126 && prefijo6 <= 622925)
                        return "Discover";
                }
            }

            return "Desconocida";
        }

        
        static void ValidarDesdeArchivo(string ruta)
        {
            if (!File.Exists(ruta))
            {
                Console.WriteLine($"\n El archivo '{ruta}' no existe. Verifique la ruta e intente de nuevo.");
                return;
            }

            string[] lineas = File.ReadAllLines(ruta);
            int validas = 0;
            int invalidas = 0;

            Console.WriteLine("\n--- Procesando archivo ---");

            foreach (string linea in lineas)
            {
                string numero = linea.Trim();

                if (string.IsNullOrWhiteSpace(numero))
                    continue; 

                bool esValida = ProcesarTarjeta(numero);
                if (esValida)
                    validas++;
                else
                    invalidas++;
            }

            Console.WriteLine("\n--- Resumen del archivo ---");
            Console.WriteLine($"Total procesadas: {validas + invalidas}");
            Console.WriteLine($"Validas: {validas}");
            Console.WriteLine($"Invalidas: {invalidas}");
        }

        
        static string GenerarNumeroValido()
        {
            Random rnd = new Random();

         
            (string prefijo, int longitud)[] opciones =
            {
                ("4", 16),      // Visa
                ("51", 16),     // Mastercard
                ("34", 15),     // American Express
                ("6011", 16)    // Discover
            };

            var elegido = opciones[rnd.Next(opciones.Length)];
            StringBuilder sb = new StringBuilder(elegido.prefijo);

            
            while (sb.Length < elegido.longitud - 1)
                sb.Append(rnd.Next(0, 10));

            int digitoControl = CalcularDigitoControl(sb.ToString());
            return sb.ToString() + digitoControl;
        }

        
        static int CalcularDigitoControl(string numeroParcial)
        {
            
            string conCeroTemporal = numeroParcial + "0";
            char[] invertido = conCeroTemporal.Reverse().ToArray();
            int suma = 0;

            for (int i = 0; i < invertido.Length; i++)
            {
                int digito = (int)char.GetNumericValue(invertido[i]);

                if ((i + 1) % 2 == 0)
                {
                    digito *= 2;
                    if (digito >= 10)
                        digito = digito / 10 + digito % 10;
                }

                suma += digito;
            }

            int residuo = suma % 10;
            return residuo == 0 ? 0 : 10 - residuo;
        }

       
        static void MostrarEstadisticas()
        {
            if (historial.Count == 0)
            {
                Console.WriteLine("\nAun no se han procesado tarjetas en esta sesion.");
                return;
            }

            int validas = historial.Count(h => h.EsValida);
            int invalidas = historial.Count - validas;

            Console.WriteLine("\n=== ESTADiSTICAS ===");
            Console.WriteLine($"Total procesadas: {historial.Count}");
            Console.WriteLine($"Validas: {validas}");
            Console.WriteLine($"Invalidas: {invalidas}");

            Console.WriteLine("\nDesglose por marca:");
            var porMarca = historial
                .GroupBy(h => h.Marca)
                .OrderByDescending(g => g.Count());

            foreach (var grupo in porMarca)
            {
                Console.WriteLine($"  {grupo.Key}: {grupo.Count()}");
            }
        }

    
        static string LimpiarNumero(string numero)
        {
            return numero.Replace(" ", "").Replace("-", "").Trim();
        }

       
        static bool EsNumerico(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return false;

            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }
    }
}

