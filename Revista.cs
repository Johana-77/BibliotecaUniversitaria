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

// ---------- MODELO: Revista (hereda de Material) ----------
// Las revistas tienen mas demanda y se prestan por menos tiempo.
class Revista : Material
{
    public const string Codigo = "REVISTA";

    public Revista(string id, string titulo, string autor, int anio)
        : base(id, titulo, autor, anio)
    {
    }

    public override string Tipo { get { return Codigo; } }
    public override string NombreTipo { get { return "Revista"; } }
    public override int DiasPrestamo { get { return 7; } }
}
