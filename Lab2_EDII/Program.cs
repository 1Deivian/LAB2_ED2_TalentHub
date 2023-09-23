
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static BinaryTree;

public class DPICodec
{
    private Dictionary<string, string> encodedDPIMap = new Dictionary<string, string>();
    private Dictionary<string, string> decodedDPIMap = new Dictionary<string, string>();
    private const string SecretKey = "TuClaveSecreta"; // Cambia esto a una clave secreta segura
    private const string LogFilePath = "bitacora.txt"; // Ubicación del archivo de bitácora

    public string EncodeDPI(string dpi, string empresa)
    {
        // Genera una clave única para la empresa y el DPI
        string uniqueKey = $"{empresa}-{dpi}";

        // Codifica el DPI utilizando una función de hash
        string encodedDPI = ComputeHash(uniqueKey);

        // Almacena la relación entre la clave única y el DPI codificado
        encodedDPIMap[uniqueKey] = encodedDPI;

        return encodedDPI;
    }

    public string DecodeDPI(string encodedDPI)
    {
        // Busca el DPI codificado en el diccionario
        foreach (var entry in encodedDPIMap)
        {
            if (entry.Value == encodedDPI)
            {
                // Divide la clave única en DPI y compañía
                string[] parts = entry.Key.Split('-');
                if (parts.Length == 2)
                {
                    // La primera parte es la empresa, la segunda es el DPI
                    string empresa = parts[0];
                    string originalDPI = parts[1];
                    return $"DPI: {originalDPI}, Compañía: {empresa}";
                }
                break; // Detén la búsqueda después de encontrar una coincidencia
            }
        }

        // Si no se encontró ninguna coincidencia, devuelve null
        return null;
    }

    // Función para calcular el hash de una cadena utilizando una clave secreta
    private string ComputeHash(string input)
    {
        using (var sha256 = new System.Security.Cryptography.SHA256Managed())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input + SecretKey);
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    // Método para registrar una entrada en la bitácora
    public void LogBitacora(BitacoraEntry bitacoraEntry)
    {
        using (StreamWriter writer = File.AppendText(LogFilePath))
        {
            var logEntry = JsonConvert.SerializeObject(bitacoraEntry);
            writer.WriteLine(logEntry);
        }
    }

    // Resto del código
}

// Clase para los datos de Persona
public class Person
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string datebirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; } // Agregar una propiedad para las compañías

    public override string ToString()
    {
        return $"{Name} (DPI: {DPI}, datebirth: {datebirth}, Empresas: {string.Join(", ", Companies)})";
    }
}

//Clase para los nodos del árbol 
public class Node
{
    public Person Data { get; set; }
    public Node Left { get; set; }
    public Node Right { get; set; }

    public Node(Person person)
    {
        Data = person;
        Left = null;
        Right = null;
    }
}

//Clase para el arbol binario 
public class BinaryTree
{
    private Node root;

    public BinaryTree()
    {
        root = null;
        personsList = new List<Person>();
    }
    public void Insert(Person person)
    {
        root = InsertRec(root, person);
        if (root != null) // Si se insertó correctamente
        {
            personsList.Add(person); // Agregar a la lista
        }
    }

    private Node InsertRec(Node root, Person person)
    {
        if (root == null)
        {
            root = new Node(person);
            Console.WriteLine("Inserción exitosa: " + person.Name);
        }
        else
        {
            int compareResult = string.Compare(person.Name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
            if (compareResult < 0)
            {
                root.Left = InsertRec(root.Left, person);
            }
            else if (compareResult > 0)
            {
                root.Right = InsertRec(root.Right, person);
            }
            else
            {
                // La persona ya existe en el árbol, actualiza la lista de compañías
                root.Data.Companies = person.Companies; // Actualiza la lista de compañías
                Console.WriteLine("Persona con el mismo nombre ya existe: " + person.Name);
            }
        }
        return root;
    }
    public void Update(string name, Person updatedPerson)
    {
        root = UpdateRec(root, name, updatedPerson);
    }

    private Node UpdateRec(Node root, string name, Person updatedPerson)
    {
        if (root == null)
        {
            Console.WriteLine("No se encontró la persona a actualizar: " + name);
        }
        else
        {
            int compareResult = string.Compare(name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
            if (compareResult == 0)
            {
                root.Data = updatedPerson;
                Console.WriteLine("Actualización exitosa para: " + name);
            }
            else if (compareResult < 0)
            {
                root.Left = UpdateRec(root.Left, name, updatedPerson);
            }
            else
            {
                root.Right = UpdateRec(root.Right, name, updatedPerson);
            }
        }
        return root;
    }

    public void InOrderTraversal()
    {
        Console.WriteLine("Listado de Personas:");
        InOrderTraversalRec(root);
    }

    private void InOrderTraversalRec(Node root)
    {
        if (root != null)
        {
            InOrderTraversalRec(root.Left);
            Console.WriteLine(root.Data);
            InOrderTraversalRec(root.Right);
        }
    }
    //Eliminación de datos
    public void Delete(string nameToDelete)
    {
        root = DeleteRec(root, nameToDelete);
        if (root != null) // Si se eliminó correctamente
        {
            personsList.RemoveAll(p => p.Name.Equals(nameToDelete, StringComparison.OrdinalIgnoreCase)); // Eliminar de la lista
        }
    }

    private Node DeleteRec(Node root, string nameToDelete)
    {
        if (root == null)
        {
            // No se encontró la persona a eliminar
            Console.WriteLine("No se encontró la persona a eliminar: " + nameToDelete);
            return root;
        }

        int compareResult = string.Compare(nameToDelete, root.Data.Name, StringComparison.OrdinalIgnoreCase);
        if (compareResult < 0)
        {
            root.Left = DeleteRec(root.Left, nameToDelete);
        }
        else if (compareResult > 0)
        {
            root.Right = DeleteRec(root.Right, nameToDelete);
        }
        else
        {
            // Se encontró la persona a eliminar
            Console.WriteLine("Eliminación exitosa: " + nameToDelete);

            // Caso 1: No tiene hijos o solo un hijo
            if (root.Left == null)
            {
                return root.Right;
            }
            else if (root.Right == null)
            {
                return root.Left;
            }

            // Caso 2: Tiene dos hijos, se encuentra el sucesor inmediato
            root.Data = FindMinValue(root.Right);

            // Elimina el sucesor inmediato
            root.Right = DeleteRec(root.Right, root.Data.Name);
        }
        return root;
    }

    private Person FindMinValue(Node node)
    {
        Person minValue = node.Data;
        while (node.Left != null)
        {
            minValue = node.Left.Data;
            node = node.Left;
        }
        return minValue;
    }
    //Busqueda de datos
    public Person Search(string name)
    {
        return SearchRec(root, name);
    }

    private Person SearchRec(Node root, string name)
    {
        if (root == null)
        {
            // No se encontró la persona
            return null;
        }

        int compareResult = string.Compare(name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
        if (compareResult == 0)
        {
            // Se encontró la persona
            return root.Data;
        }
        else if (compareResult < 0)
        {
            // La persona podría estar en el subárbol izquierdo
            return SearchRec(root.Left, name);
        }
        else
        {
            // La persona podría estar en el subárbol derecho
            return SearchRec(root.Right, name);
        }
    }
    //Clase para los datos de Bitacora
    public class BitacoraEntry
    {
        public string Name { get; set; }
        public string DPI { get; set; }
        public string DateOfBirth { get; set; }
        public string Address { get; set; }
        public List<string> Companies { get; set; } // Propiedad para las compañías

        public BitacoraEntry(Person person)
        {
            Name = person.Name;
            DPI = person.DPI;
            DateOfBirth = person.datebirth;
            Address = person.Address;

            // Inicializa la lista de compañías y copia las compañías de la persona
            Companies = new List<string>();
            if (person.Companies != null)
            {
                Companies.AddRange(person.Companies);
            }
        }
    }

    //Listado de personas después del proceso de lectura (Personas después de la eliminación)
    private List<Person> personsList;

    public List<Person> GetPersonsList()
    {
        return personsList; // Obtener la lista de personas
    }
}

class Program
{
    static void Main()
    {
        Dictionary<string, List<Person>> peopleByName = new Dictionary<string, List<Person>>();
        DPICodec dpiCodec = new DPICodec();
        // Obtener la ubicación del archivo CSV
        string inputFilePath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Datos.csv"; // Ruta del archivo CSV

        // Obtener la carpeta donde se encuentra el archivo CSV
        string csvFolder = Path.GetDirectoryName(inputFilePath);

        // Construir la ruta completa para el archivo de bitácora en la misma carpeta
        string bitacoraFilePath = Path.Combine(csvFolder, "bitacora.txt");


        // Cargar datos desde el archivo CSV
        if (File.Exists(inputFilePath))
        {
            List<string> lines = ReadCsvFile(inputFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2)
                {
                    string action = parts[0].Trim();
                    string data = parts[1].Trim();
                    switch (action)
                    {
                        case "INSERT":
                            var personData = JsonConvert.DeserializeObject<Person>(data);
                            // Verificar si ya existe una lista de personas con este nombre
                            if (!peopleByName.ContainsKey(personData.Name))
                            {
                                peopleByName[personData.Name] = new List<Person>();
                            }
                            peopleByName[personData.Name].Add(personData);
                            break;
                        case "PATCH":
                            var updatedPersonData = JsonConvert.DeserializeObject<Person>(data);
                            if (peopleByName.ContainsKey(updatedPersonData.Name))
                            {
                                // Actualizar la primera persona con ese nombre
                                var personToUpdate = peopleByName[updatedPersonData.Name].First();
                                personToUpdate.DPI = updatedPersonData.DPI;
                                personToUpdate.datebirth = updatedPersonData.datebirth;
                                personToUpdate.Address = updatedPersonData.Address;
                                personToUpdate.Companies = updatedPersonData.Companies; // Actualizar las compañías
                            }
                            else
                            {
                                Console.WriteLine("No se encontró la persona para actualizar.");
                            }
                            break;
                        case "DELETE":
                            var deleteData = JsonConvert.DeserializeObject<Person>(data);
                            if (peopleByName.ContainsKey(deleteData.Name))
                            {
                                // Eliminar la primera persona con ese nombre
                                peopleByName[deleteData.Name].RemoveAt(0);
                            }
                            else
                            {
                                Console.WriteLine("No se encontró la persona para eliminar.");
                            }
                            break;

                        default:
                            Console.WriteLine("Acción no reconocida: " + action);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Formato incorrecto en línea: " + line);
                }
            }
            Console.WriteLine("Datos cargados correctamente desde CSV.");
        }
        else
        {
            Console.WriteLine("El archivo CSV no existe en la ubicación especificada.");
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Menú:");
            Console.WriteLine("1. Ver listado de personas");
            Console.WriteLine("2. Buscar persona por DPI");
            Console.WriteLine("3. Codificar DPI");
            Console.WriteLine("4. Decodificar DPI");
            Console.WriteLine("X. Salir");

            Console.Write("Selecciona una opción: ");
            string option = Console.ReadLine();
            Console.Clear();

            if (option.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                Console.WriteLine("Listado de Personas:");

                // Crear una lista para almacenar la información de las personas a mostrar y guardar en el archivo
                List<string> personasParaMostrar = new List<string>();

                foreach (var pair in peopleByName)
                {
                    foreach (var person in pair.Value)
                    {
                        string personaInfo = $"Nombre: {person.Name} - DPI: {person.DPI}";
                        Console.WriteLine(personaInfo);
                        personasParaMostrar.Add(personaInfo);
                    }
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();

                
                string listadoFilePath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Listado.txt";

                try
                {
                    // Abre un archivo para escribir (creará uno nuevo o sobrescribirá si ya existe)
                    using (StreamWriter writer = new StreamWriter(listadoFilePath))
                    {
                        // Escribe la información de las personas en el archivo
                        foreach (string personaInfo in personasParaMostrar)
                        {
                            writer.WriteLine(personaInfo);
                        }
                    }

                    Console.WriteLine("Archivo de listado creado exitosamente en: " + listadoFilePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Error al escribir en el archivo de listado: " + e.Message);
                }
            }
            else if (option.Equals("2", StringComparison.OrdinalIgnoreCase))
            {

                Console.Clear();
                Console.WriteLine("Ingresa el DPI a buscar:");
                Console.WriteLine("-------------------------------");
                string dpiToSearch = Console.ReadLine();
                bool found = false;

                foreach (var personList in peopleByName.Values)
                {
                    foreach (var person in personList)
                    {
                        if (person.DPI.Equals(dpiToSearch, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("-------------------------------");
                            Console.WriteLine("Persona encontrada:");
                            Console.WriteLine("-------------------------------");
                            Console.WriteLine("Nombre: " + person.Name);
                            Console.WriteLine("DPI: " + person.DPI);

                            // Extraer solo la fecha de nacimiento (sin tiempo)
                            if (DateTime.TryParse(person.datebirth, out DateTime birthDate))
                            {
                                Console.WriteLine("Fecha de Nacimiento: " + birthDate.Date.ToString("yyyy-MM-dd"));
                                Console.WriteLine("Hora de Nacimiento: " + birthDate.ToString("HH:mm:ss"));
                            }
                            else
                            {
                                Console.WriteLine("Fecha de Nacimiento: " + person.datebirth);
                            }

                            Console.WriteLine("Dirección: " + person.Address);

                            if (person.Companies != null && person.Companies.Count > 0)
                            {
                                Console.WriteLine("Compañías:");
                                foreach (var company in person.Companies)
                                {
                                    Console.WriteLine("- " + company);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No se encontraron compañías asociadas.");
                            }
                            Console.WriteLine("-------------------------------");
                            Console.WriteLine();

                            // Registrar la operación en la bitácora
                            LogBitacora(new BitacoraEntry(person), bitacoraFilePath);

                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                
                if (!found)
                {
                    Console.WriteLine("No se encontró ninguna persona con ese DPI.");
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            
        }
            else if (option.Equals("3", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                Console.WriteLine("Ingrese DPI a codificar:");
                string dpiToEncode = Console.ReadLine();
                Console.WriteLine("Ingrese nombre de la empresa:");
                string empresaToEncode = Console.ReadLine();
                string encodedDPI = dpiCodec.EncodeDPI(dpiToEncode, empresaToEncode);
                Console.WriteLine($"DPI Codificado: {encodedDPI}");

                // Guardar el código generado en un archivo de texto

                string CodigoFilePath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Codigos.txt";

                try
                {
                    using (StreamWriter writer = new StreamWriter(CodigoFilePath))
                    {
                        writer.WriteLine($"DPI Codificado: {encodedDPI}");
                    }

                    Console.WriteLine("Código generado guardado en: " + CodigoFilePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Error al guardar el código en el archivo: " + e.Message);
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
            else if (option.Equals("4", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                Console.WriteLine("Ingrese el código a buscar:");
                string codeToDecode = Console.ReadLine();

                // Decodificar el DPI utilizando solo el código
                string decodedDPI = dpiCodec.DecodeDPI(codeToDecode);

                if (decodedDPI != null)
                {
                    Console.WriteLine($"DPI Decodificado: {decodedDPI}");
                }
                else
                {
                    Console.WriteLine("No se pudo decodificar el DPI para este código.");
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
            else if (option.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                break; // Salir del programa
            }
            else
            {
                Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
            }
        }
    }
    // Método para registrar una entrada en la bitácora
    private static void LogBitacora(BitacoraEntry bitacoraEntry, string bitacoraFilePath)
    {
        using (StreamWriter writer = File.AppendText(bitacoraFilePath))
        {
            var logEntry = JsonConvert.SerializeObject(bitacoraEntry);
            writer.WriteLine(logEntry);
        }
    }

    // Método para leer datos desde un archivo CSV y devolverlos como una lista de cadenas
    static List<string> ReadCsvFile(string filePath)
    {
        List<string> lines = new List<string>();

        using (TextFieldParser parser = new TextFieldParser(filePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";"); 

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                if (fields != null)
                {
                    string line = string.Join(";", fields); 
                    lines.Add(line);
                }
            }
        }

        return lines;
    }
}