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

// ---------- MODELO: Libro (hereda de Material) ----------
class Libro : Material
{
    public const string Codigo = "LIBRO";

    public Libro(string id, string titulo, string autor, int anio)
        : base(id, titulo, autor, anio)
    {
    }

    public override string Tipo { get { return Codigo; } }
    public override string NombreTipo { get { return "Libro"; } }
    public override int DiasPrestamo { get { return 14; } }
}

// ---------- MODELO: LibroReferencia (hereda de Libro) ----------
// Diccionarios, enciclopedias, atlas: no salen de la biblioteca.
class LibroReferencia : Libro
{
    public const string CodigoReferencia = "REFERENCIA";

    public LibroReferencia(string id, string titulo, string autor, int anio)
        : base(id, titulo, autor, anio)
    {
    }

    public override string Tipo { get { return CodigoReferencia; } }
    public override string NombreTipo { get { return "Referencia"; } }
    public override int DiasPrestamo { get { return 0; } }

    // Polimorfismo: cambia la regla heredada, este material nunca se presta
    public override bool PermitePrestamo { get { return false; } }

    public override string EstadoTexto(Prestamo prestamo)
    {
        return Disponible ? "Solo consulta en sala" : base.EstadoTexto(prestamo);
    }
}
