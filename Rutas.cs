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

// ---------- Rutas de los archivos (carpeta "datos" junto al programa) ----------
static class Rutas
{
    public static readonly string Carpeta;

    static Rutas()
    {
        Carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datos");
        Directory.CreateDirectory(Carpeta);
    }

    public static string Libros { get { return Path.Combine(Carpeta, "libros.csv"); } }
    public static string Prestamos { get { return Path.Combine(Carpeta, "prestamos.csv"); } }
    public static string Reservas { get { return Path.Combine(Carpeta, "reservas.csv"); } }
    public static string Usuarios { get { return Path.Combine(Carpeta, "usuarios.txt"); } }
    public static string Bitacora { get { return Path.Combine(Carpeta, "bitacora.log"); } }
}
