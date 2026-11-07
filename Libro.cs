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

// ---------- MODELO: Libro ----------
class Libro
{
    private string id;
    private string titulo;
    private string autor;
    private int anio;
    private bool disponible = true;

    public Libro(string id, string titulo, string autor, int anio)
    {
        this.id = Validador.IdLibro(id);
        this.titulo = Validador.Texto(titulo, "titulo", 80);
        this.autor = Validador.Texto(autor, "autor", 60);
        this.anio = Validador.Anio(anio);
    }

    public string Id { get { return id; } }
    public string Titulo { get { return titulo; } }
    public string Autor { get { return autor; } }
    public int Anio { get { return anio; } }
    public bool Disponible
    {
        get { return disponible; }
        set { disponible = value; }
    }

    public string ALinea()
    {
        return id + ";" + titulo + ";" + autor + ";" + anio;
    }

    public static Libro DesdeLinea(string linea)
    {
        string[] p = linea.Split(';');
        if (p.Length != 4)
            throw new ValidacionException("Linea de libro con formato incorrecto");
        return new Libro(p[0], p[1], p[2], Validador.AnioTexto(p[3]));
    }
}
