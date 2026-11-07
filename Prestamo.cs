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

// ---------- MODELO: Prestamo ----------
class Prestamo
{
    private string idLibro;
    private string socio;
    private DateTime fecha;

    public Prestamo(string idLibro, string socio, DateTime fecha)
    {
        this.idLibro = Validador.IdLibro(idLibro);
        this.socio = Validador.Carnet(socio);
        this.fecha = fecha;
    }

    public string IdLibro { get { return idLibro; } }
    public string Socio { get { return socio; } }
    public DateTime Fecha { get { return fecha; } }
    public DateTime FechaLimite { get { return fecha.AddDays(14); } }

    public string ALinea()
    {
        return idLibro + ";" + socio + ";" + fecha.ToString("yyyy-MM-dd");
    }

    public static Prestamo DesdeLinea(string linea)
    {
        string[] p = linea.Split(';');
        DateTime f;
        if (p.Length != 3 || !DateTime.TryParseExact(p[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out f))
            throw new ValidacionException("Linea de prestamo con formato incorrecto");
        return new Prestamo(p[0], p[1], f);
    }
}

// ---------- RESULTADO DE LA MIGRACION ----------
class ResultadoImportacion
{
    public int Importados = 0;
    public List<string> Rechazados = new List<string>();
}
