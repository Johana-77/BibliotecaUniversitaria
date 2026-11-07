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
//   Dictionary<string, Material>      -> busqueda por codigo en O(1) (guarda Libro, Revista o LibroReferencia)
//   Dictionary<string, Prestamo>      -> prestamo activo de cada material en O(1)
//   Dictionary<string, Queue<string>> -> cola de reservas (FIFO: el primero en reservar es el primero en recibir)
//   Dictionary<string, int>           -> cantidad de prestamos por socio en O(1)
//
// Polimorfismo: esta clase solo conoce la clase base Material. Cuando llama a
// ValidarPrestamo(), DiasPrestamo o ALinea() se ejecuta el codigo de la clase real.
class Biblioteca
{
    public const int MaxPrestamosPorSocio = 3;
    public const string SocioLegado = "00000";

    private Dictionary<string, Material> libros = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Prestamo> prestamos = new Dictionary<string, Prestamo>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Queue<string>> reservas = new Dictionary<string, Queue<string>>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, int> prestamosPorSocio = new Dictionary<string, int>();

    public int TotalLibros { get { return libros.Count; } }

    public void AgregarLibro(Material libro)
    {
        if (libros.ContainsKey(libro.Id))
            throw new ValidacionException("Ya existe un libro con el codigo " + libro.Id);
        libros.Add(libro.Id, libro);
    }

    public Material ObtenerLibro(string id)
    {
        Material libro;
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

    public List<Material> Buscar(string texto)
    {
        texto = (texto ?? "").Trim();
        List<Material> resultado = new List<Material>();
        foreach (Material l in libros.Values)
        {
            if (texto.Length == 0
                || l.Id.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || l.Titulo.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || l.Autor.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || l.NombreTipo.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                resultado.Add(l);
            }
        }
        resultado.Sort(delegate(Material a, Material b) { return string.Compare(a.Titulo, b.Titulo, StringComparison.OrdinalIgnoreCase); });
        return resultado;
    }

    public int CantidadDe(string socio)
    {
        int n;
        return prestamosPorSocio.TryGetValue(socio ?? "", out n) ? n : 0;
    }

    public void Prestar(string id, string socio)
    {
        Material libro = ObtenerLibro(id);
        socio = Validador.Carnet(socio);
        libro.ValidarPrestamo();   // polimorfico: un LibroReferencia lo rechaza
        if (CantidadDe(socio) >= MaxPrestamosPorSocio)
            throw new ReglaNegocioException("El socio ya tiene " + MaxPrestamosPorSocio + " libros prestados (maximo permitido)");
        RegistrarPrestamo(new Prestamo(libro.Id, socio, DateTime.Now, libro.DiasPrestamo));
        ManejadorErrores.Registrar("PRESTAMO", libro.NombreTipo + " " + libro.Id + " prestado al carnet " + socio
            + " por " + libro.DiasPrestamo + " dias");
    }

    // Devuelve un mensaje para mostrar en pantalla
    public string Devolver(string id)
    {
        Material libro = ObtenerLibro(id);
        if (libro.Disponible)
            throw new ReglaNegocioException("Ese libro no esta prestado");

        QuitarPrestamo(libro.Id);
        ManejadorErrores.Registrar("DEVOLUCION", libro.NombreTipo + " " + libro.Id + " devuelto");

        Queue<string> cola;
        if (reservas.TryGetValue(libro.Id, out cola) && cola.Count > 0)
        {
            string siguiente = cola.Dequeue();
            RegistrarPrestamo(new Prestamo(libro.Id, siguiente, DateTime.Now, libro.DiasPrestamo));
            return "Devuelto. Se presto automaticamente al carnet " + siguiente + " (primero en la cola de reservas)";
        }
        return "Libro devuelto y disponible";
    }

    public void Reservar(string id, string socio)
    {
        Material libro = ObtenerLibro(id);
        socio = Validador.Carnet(socio);
        if (!libro.PermitePrestamo)
            throw new ReglaNegocioException("Ese material es solo de consulta en sala: no se presta ni se reserva");
        if (libro.Disponible)
            throw new ReglaNegocioException("El libro esta disponible: puedes prestarlo directamente");
        Prestamo actual = PrestamoDe(libro.Id);
        if (actual != null && actual.Socio == socio)
            throw new ReglaNegocioException("Ese socio ya tiene este libro prestado");

        Queue<string> cola;
        if (!reservas.TryGetValue(libro.Id, out cola))
        {
            cola = new Queue<string>();
            reservas.Add(libro.Id, cola);
        }
        if (cola.Contains(socio))
            throw new ReglaNegocioException("Ese socio ya reservo este libro");
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
        Material libro = ObtenerLibro(id);
        RegistrarPrestamo(new Prestamo(libro.Id, SocioLegado, DateTime.Now, libro.DiasPrestamo));
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
        List<RegistroArchivo> l = new List<RegistroArchivo>(libros.Values);
        EscribirSeguro(Rutas.Libros, l);

        List<RegistroArchivo> p = new List<RegistroArchivo>(prestamos.Values);
        EscribirSeguro(Rutas.Prestamos, p);

        List<RegistroArchivo> r = new List<RegistroArchivo>();
        foreach (KeyValuePair<string, Queue<string>> par in reservas)
        {
            foreach (string socio in par.Value) r.Add(new Reserva(par.Key, socio));
        }
        EscribirSeguro(Rutas.Reservas, r);
    }

    public void Cargar()
    {
        if (File.Exists(Rutas.Libros))
        {
            foreach (string linea in File.ReadAllLines(Rutas.Libros))
            {
                // Material.DesdeLinea crea un Libro, Revista o LibroReferencia segun el tipo guardado
                try { AgregarLibro(Material.DesdeLinea(linea)); }
                catch (ValidacionException ex) { ManejadorErrores.Registrar("DATOS", "Libro descartado al cargar: " + ex.Message); }
            }
        }
        if (File.Exists(Rutas.Prestamos))
        {
            foreach (string linea in File.ReadAllLines(Rutas.Prestamos))
            {
                try
                {
                    Prestamo pr = Prestamo.DesdeLinea(linea, delegate(string id)
                    {
                        Material m;
                        return libros.TryGetValue(id, out m) ? m.DiasPrestamo : 0;
                    });
                    if (libros.ContainsKey(pr.IdLibro) && !prestamos.ContainsKey(pr.IdLibro)) RegistrarPrestamo(pr);
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
                    Reserva rs = Reserva.DesdeLinea(linea);
                    if (!libros.ContainsKey(rs.IdLibro)) continue;
                    Queue<string> cola;
                    if (!reservas.TryGetValue(rs.IdLibro, out cola)) { cola = new Queue<string>(); reservas.Add(rs.IdLibro, cola); }
                    if (!cola.Contains(rs.Socio)) cola.Enqueue(rs.Socio);
                }
                catch (ValidacionException ex) { ManejadorErrores.Registrar("DATOS", "Reserva descartada al cargar: " + ex.Message); }
            }
        }
    }

    // Escribe primero en un archivo temporal para no dejar archivos a medias si algo falla.
    // Recibe la clase base: cada objeto (Material, Prestamo, Reserva) sabe escribirse con ALinea().
    private static void EscribirSeguro(string ruta, List<RegistroArchivo> registros)
    {
        List<string> lineas = new List<string>();
        foreach (RegistroArchivo reg in registros) lineas.Add(reg.ALinea());

        string temporal = ruta + ".tmp";
        File.WriteAllLines(temporal, lineas, Encoding.UTF8);
        File.Copy(temporal, ruta, true);
        File.Delete(temporal);
    }
}
