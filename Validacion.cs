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

// ---------- Excepcion propia para datos invalidos ----------
class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}

// ---------- MODULO DE VALIDACIONES (se usa en toda la aplicacion) ----------
static class Validador
{
    static readonly char[] Prohibidos = { ';', '|', '"', '<', '>' };

    public static string Texto(string valor, string campo, int max)
    {
        if (valor == null) valor = "";
        valor = valor.Trim();
        if (valor.Length == 0)
            throw new ValidacionException("El campo " + campo + " es obligatorio");
        if (valor.Length > max)
            throw new ValidacionException("El campo " + campo + " no puede superar " + max + " caracteres");
        if (valor.IndexOfAny(Prohibidos) >= 0)
            throw new ValidacionException("El campo " + campo + " contiene caracteres no permitidos ( ; | \" < > )");
        foreach (char c in valor)
        {
            if (char.IsControl(c))
                throw new ValidacionException("El campo " + campo + " contiene caracteres de control");
        }
        // Evita la inyeccion de formulas al abrir los archivos en Excel
        if ("=+-@".IndexOf(valor[0]) >= 0)
            throw new ValidacionException("El campo " + campo + " no puede empezar con = + - o @");
        return valor;
    }

    public static string IdLibro(string valor)
    {
        valor = (valor ?? "").Trim().ToUpper();
        if (!Regex.IsMatch(valor, "^[A-Z0-9]{3,10}$"))
            throw new ValidacionException("El codigo del libro debe tener de 3 a 10 letras o numeros (sin espacios)");
        return valor;
    }

    public static int Anio(int anio)
    {
        if (anio < 1000 || anio > DateTime.Now.Year)
            throw new ValidacionException("El anio debe estar entre 1000 y " + DateTime.Now.Year);
        return anio;
    }

    public static int AnioTexto(string valor)
    {
        int anio;
        if (!int.TryParse((valor ?? "").Trim(), out anio))
            throw new ValidacionException("El anio debe ser un numero entero");
        return Anio(anio);
    }

    public static string Carnet(string valor)
    {
        valor = (valor ?? "").Trim();
        if (!Regex.IsMatch(valor, "^[0-9]{5,10}$"))
            throw new ValidacionException("El carnet del socio debe tener de 5 a 10 digitos");
        return valor;
    }

    public static string Usuario(string valor)
    {
        valor = (valor ?? "").Trim();
        if (!Regex.IsMatch(valor, "^[a-zA-Z0-9_]{4,20}$"))
            throw new ValidacionException("El usuario debe tener de 4 a 20 letras, numeros o guion bajo");
        return valor;
    }

    public static void Clave(string clave)
    {
        clave = clave ?? "";
        bool mayus = false, minus = false, digito = false;
        foreach (char c in clave)
        {
            if (char.IsUpper(c)) mayus = true;
            if (char.IsLower(c)) minus = true;
            if (char.IsDigit(c)) digito = true;
        }
        if (clave.Length < 8 || clave.Length > 64 || !mayus || !minus || !digito)
            throw new ValidacionException("La clave debe tener 8 o mas caracteres, con mayuscula, minuscula y numero");
    }

    public static string RutaImportacion(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            throw new ValidacionException("Selecciona un archivo para importar");
        string ext = Path.GetExtension(ruta).ToLower();
        if (ext != ".txt" && ext != ".csv")
            throw new ValidacionException("Solo se permiten archivos .txt o .csv");
        if (!File.Exists(ruta))
            throw new ValidacionException("El archivo seleccionado no existe");
        if (new FileInfo(ruta).Length > 1024 * 1024)
            throw new ValidacionException("El archivo supera el tamano maximo permitido (1 MB)");
        return Path.GetFullPath(ruta);
    }
}
