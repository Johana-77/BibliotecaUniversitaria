using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// ---------- LOGICA PRINCIPAL ----------
// Estructuras de datos elegidas:
//   Dictionary<string, Libro>         -> busqueda por codigo en O(1)
//   Dictionary<string, Prestamo>      -> prestamo activo de cada libro en O(1)
//   Dictionary<string, Queue<string>> -> cola de reservas (FIFO: el primero en reservar es el primero en recibir)
//   Dictionary<string, int>           -> cantidad de prestamos por socio en O(1)
class Biblioteca
{
    private const int MaxPrestamosPorSocio = 3;

    private Dictionary<string, Libro> libros = new Dictionary<string, Libro>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Prestamo> prestamos = new Dictionary<string, Prestamo>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Queue<string>> reservas = new Dictionary<string, Queue<string>>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, int> prestamosPorSocio = new Dictionary<string, int>();

    public int TotalLibros { get { return libros.Count; } }

    public void AgregarLibro(Libro libro)
    {
        if (libros.ContainsKey(libro.Id))
            throw new ValidacionException("Ya existe un libro con el codigo " + libro.Id);
        libros.Add(libro.Id, libro);
    }

    public Libro ObtenerLibro(string id)
    {
        Libro libro;
        if (!libros.TryGetValue((id ?? "").Trim(), out libro))
            throw new ValidacionException("No existe el libro con codigo " + id);
        return libro;
    }

    public Prestamo PrestamoDe(string id)
    {
        Prestamo p;
        prestamos.TryGetValue(id, out p);
        return p;
    }

    public List<Libro> Buscar(string texto)
    {
        texto = (texto ?? "").Trim();
        List<Libro> resultado = new List<Libro>();
        foreach (Libro l in libros.Values)
        {
            if (texto.Length == 0
                || l.Id.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || l.Titulo.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || l.Autor.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                resultado.Add(l);
            }
        }
        resultado.Sort(delegate(Libro a, Libro b) { return string.Compare(a.Titulo, b.Titulo, StringComparison.OrdinalIgnoreCase); });
        return resultado;
    }

    public void Prestar(string id, string socio)
    {
        Libro libro = ObtenerLibro(id);
        socio = Validador.Carnet(socio);
        if (!libro.Disponible)
            throw new ValidacionException("El libro ya esta prestado. Puedes reservarlo");
        if (CantidadDe(socio) >= MaxPrestamosPorSocio)
            throw new ValidacionException("El socio ya tiene " + MaxPrestamosPorSocio + " libros prestados (maximo permitido)");
        RegistrarPrestamo(new Prestamo(libro.Id, socio, DateTime.Now));
        ManejadorErrores.Registrar("PRESTAMO", "Libro " + libro.Id + " prestado al carnet " + socio);
    }

    // Devuelve un mensaje para mostrar en pantalla
    public string Devolver(string id)
    {
        Libro libro = ObtenerLibro(id);
        if (libro.Disponible)
            throw new ValidacionException("Ese libro no esta prestado");

        QuitarPrestamo(libro.Id);
        ManejadorErrores.Registrar("DEVOLUCION", "Libro " + libro.Id + " devuelto");

        Queue<string> cola;
        if (reservas.TryGetValue(libro.Id, out cola) && cola.Count > 0)
        {
            string siguiente = cola.Dequeue();
            RegistrarPrestamo(new Prestamo(libro.Id, siguiente, DateTime.Now));
            return "Devuelto. Se presto automaticamente al carnet " + siguiente + " (primero en la cola de reservas)";
        }
        return "Libro devuelto y disponible";
    }

    public void Reservar(string id, string socio)
    {
        Libro libro = ObtenerLibro(id);
        socio = Validador.Carnet(socio);
        if (libro.Disponible)
            throw new ValidacionException("El libro esta disponible: puedes prestarlo directamente");
        Prestamo actual = PrestamoDe(libro.Id);
        if (actual != null && actual.Socio == socio)
            throw new ValidacionException("Ese socio ya tiene este libro prestado");

        Queue<string> cola;
        if (!reservas.TryGetValue(libro.Id, out cola))
        {
            cola = new Queue<string>();
            reservas.Add(libro.Id, cola);
        }
        if (cola.Contains(socio))
            throw new ValidacionException("Ese socio ya reservo este libro");
        cola.Enqueue(socio);
        ManejadorErrores.Registrar("RESERVA", "Libro " + libro.Id + " reservado por el carnet " + socio);
    }

    public string[] ObtenerCola(string id)
    {
        Queue<string> cola;
        if (reservas.TryGetValue(id, out cola)) return cola.ToArray();
        return new string[0];
    }

    // Usado por la migracion: el sistema viejo no guardaba quien tenia el libro
    public void MarcarPrestadoLegado(string id)
    {
        RegistrarPrestamo(new Prestamo(id, "00000", DateTime.Now));
    }

    private int CantidadDe(string socio)
    {
        int n;
        return prestamosPorSocio.TryGetValue(socio, out n) ? n : 0;
    }

    private void RegistrarPrestamo(Prestamo p)
    {
        prestamos[p.IdLibro] = p;
        libros[p.IdLibro].Disponible = false;
        prestamosPorSocio[p.Socio] = CantidadDe(p.Socio) + 1;
    }

    private void QuitarPrestamo(string id)
    {
        Prestamo p = prestamos[id];
        prestamos.Remove(id);
        libros[id].Disponible = true;
        int n = CantidadDe(p.Socio) - 1;
        if (n <= 0) prestamosPorSocio.Remove(p.Socio);
        else prestamosPorSocio[p.Socio] = n;
    }

    // ----- Persistencia en archivos -----
    public void Guardar()
    {
        List<string> l = new List<string>();
        foreach (Libro x in libros.Values) l.Add(x.ALinea());
        EscribirSeguro(Rutas.Libros, l);

        List<string> p = new List<string>();
        foreach (Prestamo x in prestamos.Values) p.Add(x.ALinea());
        EscribirSeguro(Rutas.Prestamos, p);

        List<string> r = new List<string>();
        foreach (KeyValuePair<string, Queue<string>> par in reservas)
        {
            foreach (string socio in par.Value) r.Add(par.Key + ";" + socio);
        }
        EscribirSeguro(Rutas.Reservas, r);
    }

    public void Cargar()
    {
        if (File.Exists(Rutas.Libros))
        {
            foreach (string linea in File.ReadAllLines(Rutas.Libros))
            {
                try { AgregarLibro(Libro.DesdeLinea(linea)); }
                catch (ValidacionException ex) { ManejadorErrores.Registrar("DATOS", "Libro descartado al cargar: " + ex.Message); }
            }
        }
        if (File.Exists(Rutas.Prestamos))
        {
            foreach (string linea in File.ReadAllLines(Rutas.Prestamos))
            {
                try
                {
                    Prestamo pr = Prestamo.DesdeLinea(linea);
                    if (libros.ContainsKey(pr.IdLibro)) RegistrarPrestamo(pr);
                }
                catch (ValidacionException ex) { ManejadorErrores.Registrar("DATOS", "Prestamo descartado al cargar: " + ex.Message); }
            }
        }
        if (File.Exists(Rutas.Reservas))
        {
            foreach (string linea in File.ReadAllLines(Rutas.Reservas))
            {
                try
                {
                    string[] p = linea.Split(';');
                    if (p.Length != 2) throw new ValidacionException("Linea de reserva con formato incorrecto");
                    string id = Validador.IdLibro(p[0]);
                    string socio = Validador.Carnet(p[1]);
                    if (!libros.ContainsKey(id)) continue;
                    Queue<string> cola;
                    if (!reservas.TryGetValue(id, out cola)) { cola = new Queue<string>(); reservas.Add(id, cola); }
                    cola.Enqueue(socio);
                }
                catch (ValidacionException ex) { ManejadorErrores.Registrar("DATOS", "Reserva descartada al cargar: " + ex.Message); }
            }
        }
    }

    // Escribe primero en un archivo temporal para no dejar archivos a medias si algo falla
    private static void EscribirSeguro(string ruta, List<string> lineas)
    {
        string temporal = ruta + ".tmp";
        File.WriteAllLines(temporal, lineas, Encoding.UTF8);
        File.Copy(temporal, ruta, true);
        File.Delete(temporal);
    }
}
