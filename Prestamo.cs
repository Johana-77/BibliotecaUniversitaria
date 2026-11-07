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

// ---------- MODELO: Prestamo (hereda de RegistroArchivo) ----------
class Prestamo : RegistroArchivo
{
    private string idLibro;
    private string socio;
    private DateTime fecha;
    private int dias;

    public Prestamo(string idLibro, string socio, DateTime fecha, int dias)
    {
        this.idLibro = Validador.IdLibro(idLibro);
        this.socio = Validador.Carnet(socio);
        this.fecha = fecha;
        this.dias = dias;
    }

    public string IdLibro { get { return idLibro; } }
    public string Socio { get { return socio; } }
    public DateTime Fecha { get { return fecha; } }
    public int Dias { get { return dias; } }
    // El plazo depende del tipo de material (Libro 14 dias, Revista 7 dias)
    public DateTime FechaLimite { get { return fecha.AddDays(dias); } }

    public override string ALinea()
    {
        return idLibro + ";" + socio + ";" + fecha.ToString("yyyy-MM-dd");
    }

    // Los dias se toman del material al que pertenece el prestamo
    public static Prestamo DesdeLinea(string linea, Func<string, int> diasDelMaterial)
    {
        string[] p = linea.Split(';');
        DateTime f;
        if (p.Length != 3 || !DateTime.TryParseExact(p[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out f))
            throw new ValidacionException("Linea de prestamo con formato incorrecto");
        string id = Validador.IdLibro(p[0]);
        return new Prestamo(id, p[1], f, diasDelMaterial(id));
    }
}

// ---------- MODELO: Reserva (hereda de RegistroArchivo) ----------
class Reserva : RegistroArchivo
{
    private string idLibro;
    private string socio;

    public Reserva(string idLibro, string socio)
    {
        this.idLibro = Validador.IdLibro(idLibro);
        this.socio = Validador.Carnet(socio);
    }

    public string IdLibro { get { return idLibro; } }
    public string Socio { get { return socio; } }

    public override string ALinea()
    {
        return idLibro + ";" + socio;
    }

    public static Reserva DesdeLinea(string linea)
    {
        string[] p = linea.Split(';');
        if (p.Length != 2) throw new ValidacionException("Linea de reserva con formato incorrecto");
        return new Reserva(p[0], p[1]);
    }
}

// ---------- RESULTADO DE LA MIGRACION ----------
class ResultadoImportacion
{
    public int Importados = 0;
    public List<string> Rechazados = new List<string>();
}
